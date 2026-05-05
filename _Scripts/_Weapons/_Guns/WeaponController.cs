using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LightSide;

public enum FireMode { Single, Burst, Auto }

public abstract class WeaponController : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Точка выстрела (Transform на геометрии оружия)")] 
    public Transform firePoint;

    [Header("Hit-Scan Settings")]
    public float range = 100f;
    public float damage = 30f;

    [Tooltip("Скорострельность (выстрелов в минуту)")]
    public float fireRate = 10f;

    [Header("Recoil Settings")]
    public Vector2 recoilImpulse = new Vector2(0.5f, 1f);
    public float recoilDuration = 0.15f;
    public float maxArmRecoilOffset = 0.4f;

    [Header("Fire Mode")]
    [Tooltip("Текущий режим огня")]
    public FireMode currentFireMode = FireMode.Single;

    [Tooltip("Какие режимы огня доступны для этого оружия")]
    public List<FireMode> availableFireModes = new List<FireMode> { FireMode.Single };

    [Tooltip("Длина очереди")]
    public int burstLength = 3;

    [Tooltip("Время на расход очереди")]
    public float burstDuration = 0.5f;
    protected float burstInterval;

    [Header("Spread System")]
    [Tooltip("Количество гарантированно точных выстрелов")]
    public int accurateShots = 3;

    [Tooltip("Максимальный разброс (в градусах)")]
    public float maxSpread = 5f;

    [Tooltip("Скорость накопления разброса (градусов за выстрел)")]
    public float spreadIncreaseRate = 0.5f;

    [Tooltip("Время восстановления разброса (секунды)")]
    public float spreadRecoveryTime = 1f;

    [Header("Shotgun Settings")]
    [Tooltip("Количество пуль за один выстрел")]
    public int pelletsPerShot = 8;
    [Tooltip("Максимальный разброс для дроби (в градусах)")]
    public float shotgunSpread = 15f;

    [Header("Aiming")]
    [Tooltip("Множитель точных выстрелов при прицеливании")]
    public float aimAccurateShotsMultiplier = 2f;

    [Tooltip("Замедление движения при прицеливании (0 = без замедления, 1 = полная остановка)")]
    [Range(0f, 1f)]
    public float aimMovementSlowdown = 0.5f;

    [Tooltip("Уменьшение разброса при прицеливании (множитель)")]
    public float aimSpreadReduction = 0.5f;

    [Header("Reload")]
    [Tooltip("Время полной перезарядки")]
    public float reloadTimeFull = 2.5f;
    [Tooltip("Время тактической перезарядки")]
    public float reloadTimeTactical = 1.8f;
    protected bool isTacticalReload = false;

    [Header("Ammo")]
    [Tooltip("Текущий магазин")]
    public int currentAmmo = 30;

    [Tooltip("Максимальный магазин")]
    public int maxAmmo = 30;

    [Header("Shell Ejection")]
    public bool useShells = true;
    public GameObject shellPrefab;
    public Transform ejectPoint;
    public float shellForce = 5f;
    public float shellUpward = 2f;

    [Header("Tracer Settings")]
    public GameObject tracerPrefab;
    public Color tracerColor = Color.yellow;
    public float tracerWidth = 0.05f;
    public float tracerLifetime = 0.2f;
    public float tracerFadeSpeed = 0.5f;

    [Tooltip("Время отображения подсказки")]
    public float hintDisplayTime = 2f;

    [Header("References")]
    protected PlayerMovement playerMovement;
    protected ArmsController dynamicArms;
    protected PlayerInput playerInput;
    protected UniText fireModeHint;
    protected Animator animator;
    protected UniText ammoHint;

    protected int shotsFired = 0;
    protected float currentSpread = 0f;
    protected float lastFireTime = 0f;
    protected float fireInterval;
    protected bool isReloading = false;
    protected bool isAiming = false;
    
    private string currentAmmoStatus = ""; 
    private float hintTimer = 0f;

    private AnimationCurve recoilCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    protected Coroutine reloadCoroutine;

    protected virtual void Awake()
    {
        dynamicArms = GetComponentInParent<WeaponComponentsTransform>().armController;
        animator = GetComponentInParent<Animator>();
    }

    protected virtual void Start()
    {
        playerMovement = dynamicArms.playerData.playerMovement;
        fireModeHint = dynamicArms.playerData.fireModeHint;
        playerInput = dynamicArms.playerData.playerInput;
        ammoHint = dynamicArms.playerData.ammoHint;

        if (fireModeHint != null) 
            fireModeHint.gameObject.SetActive(false);

        burstInterval = burstDuration / Mathf.Max(1, burstLength);
        fireInterval = 60f / fireRate;
    }

    protected virtual void Update()
    {
        HandleInput();
        UpdateSpread();
        UpdateUI();
    }

    protected virtual void OnDisable()
    {
        if (reloadCoroutine != null) 
            StopCoroutine(reloadCoroutine);
    }

    protected virtual void HandleInput()
    {
        InitialFunctions();

        ShouldFire();
    }

    protected virtual void InitialFunctions()
    {
        if (playerInput.reloadPressed && !isReloading && currentAmmo < maxAmmo)
            StartReload();

        if (isReloading) return;

        if (playerInput.cycleFireModePressed && availableFireModes.Count > 1)
            SwitchFireMode();

        isAiming = playerInput.aimHeld;
    }

    protected abstract void Fire();
    protected abstract void ShouldFire();


    protected virtual void ApplyRecoil()
    {
        Vector2 recoilImpulseLocal = recoilImpulse;
        recoilImpulseLocal.y = Mathf.Min(recoilImpulseLocal.y, maxArmRecoilOffset);

        if (dynamicArms != null)
            dynamicArms.ApplyRecoilImpulse(recoilImpulseLocal, recoilDuration, recoilCurve);

        if (playerMovement != null)
        {
            var cameraFollow = dynamicArms.playerData.cameraFollow;
            if (cameraFollow != null)
                cameraFollow.ApplyRecoilImpulse(recoilImpulse * 0.3f);
        }
    }

    protected virtual Vector3 GetFireDirection(float spreadAngle)
    {
        Vector3 baseDirection = firePoint.forward;

        if (shotsFired < GetAccurateShotsCount()) return baseDirection;

        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomRadius = Mathf.Sqrt(Random.Range(0f, 1f)) * spreadAngle;
        float spreadRad = randomRadius * Mathf.Deg2Rad;

        Vector3 right = firePoint.right;
        Vector3 up = firePoint.up;

        float offsetX = Mathf.Tan(spreadRad) * Mathf.Cos(randomAngle);
        float offsetY = Mathf.Tan(spreadRad) * Mathf.Sin(randomAngle);

        Vector3 spreadDirection = baseDirection + right * offsetX + up * offsetY;

        return spreadDirection.normalized;
    }

    protected virtual void PerformHitScan(Vector3 direction)
    {
        Vector3 startPoint = firePoint.position;
        RaycastHit hit;

        if (Physics.Raycast(startPoint, direction, out hit, range))
        {
            Debug.DrawRay(startPoint, direction * hit.distance, Color.red, 0.5f);
            CreateTracer(startPoint, hit.point);
        }
        else
        {
            Debug.DrawRay(startPoint, direction * range, Color.yellow, 0.5f);
            CreateTracer(startPoint, startPoint + direction * range);
        }
    }

    protected virtual float CalculateSpread()
    {
        float spread = currentSpread;
        if (isAiming) spread *= aimSpreadReduction;
        return spread;
    }

    protected virtual int GetAccurateShotsCount()
    {
        int baseAccurate = accurateShots;
        if (isAiming) baseAccurate = Mathf.RoundToInt(accurateShots * aimAccurateShotsMultiplier);
        return baseAccurate;
    }

    protected virtual void UpdateSpread()
    {
        if (currentSpread > 0f)
        {
            float timeSinceLastShot = Time.time - lastFireTime;
            if (timeSinceLastShot > 0.1f)
            {
                float recoveryRate = maxSpread / spreadRecoveryTime;
                currentSpread = Mathf.Max(0f, currentSpread - recoveryRate * Time.deltaTime);
            }
        }
    }

    protected virtual void StartReload()
    {
        isReloading = true;
        currentSpread = 0f;
        shotsFired = 0;

        float reloadDuration = (currentAmmo == 0) ? reloadTimeFull : reloadTimeTactical;
        string reloadAnim = (currentAmmo == 0) ? "Reload" : "TacticalReload";

        animator.SetTrigger(reloadAnim);
        reloadCoroutine = StartCoroutine(ReloadRoutine(reloadDuration));
    }

    protected virtual IEnumerator ReloadRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        FinishReload();
    }

    protected virtual void FinishReload()
    {
        currentAmmo = isTacticalReload ? maxAmmo + 1 : maxAmmo;

        isReloading = false;
        isTacticalReload = false;
        TryShowAmmoHint();
    }

    protected virtual void SwitchFireMode()
    {
        int currectIndex = availableFireModes.IndexOf(currentFireMode);
        int nextIndex = (currectIndex + 1) % availableFireModes.Count;
        currentFireMode = availableFireModes[nextIndex];

        ShowFireModeHint();
    }

    protected virtual void EjectShell()
    {
        if (!useShells || shellPrefab == null || ejectPoint == null) return;

        GameObject shell = Instantiate(shellPrefab, ejectPoint.position, ejectPoint.rotation);
        Rigidbody rb = shell.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = ejectPoint.right * Random.Range(0.5f, 1.0f)
                              + ejectPoint.up * Random.Range(0.3f, 0.7f)
                              + (-ejectPoint.forward) * Random.Range(0.2f, 0.5f);

            rb.AddForce(direction * shellForce + Vector3.up * shellUpward, ForceMode.Impulse);

            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        Destroy(shell, 5f);
    }

    protected virtual void CreateTracer(Vector3 start, Vector3 end)
    {
        GameObject tracerObj;
        tracerObj = Instantiate(tracerPrefab, start, Quaternion.identity);
        Tracer tracer = tracerObj.GetComponent<Tracer>();

        tracer.Initialize(start, end, tracerColor, tracerWidth);
        tracer.lifetime = tracerLifetime;
        tracer.fadeSpeed = tracerFadeSpeed;
    }

    protected virtual void UpdateUI()
    {
        if (hintTimer <= 0f) return;

        hintTimer -= Time.deltaTime;
        if (hintTimer <= 0f && (fireModeHint != null || ammoHint != null))
        {
            fireModeHint.gameObject.SetActive(false);
            ammoHint.gameObject.SetActive(false);
        }
    }

    protected virtual void ShowFireModeHint()
    {
        if (fireModeHint == null) return;

        string modeName = "";
        switch (currentFireMode)
        {
            case FireMode.Single:
                modeName = "Одиночный";
                break;
            case FireMode.Burst:
                modeName = $"Очередь ({burstLength})";
                break;
            case FireMode.Auto:
                modeName = "Автомат";
                break;
        }

        fireModeHint.Text = $"Режим огня: {modeName}";
        fireModeHint.gameObject.SetActive(true);
        hintTimer = hintDisplayTime;
    }

    protected virtual void TryShowAmmoHint()
    {
        string newStatus = GetAmmoStatusText();
        if (newStatus == currentAmmoStatus) return;

        currentAmmoStatus = newStatus;
        ammoHint.Text = $"Магазин: {newStatus}";
        ammoHint.gameObject.SetActive(true);
        hintTimer = hintDisplayTime;
    }

    protected virtual string GetAmmoStatusText()
    {
        float percent = (float)currentAmmo / maxAmmo * 100f;

        if (percent >= 100f) 
            return "Full";
        else if (percent >= 50f) 
            return "Half-full";
        else if (percent < 50f && percent > 1f) 
            return "Half-empty";
        else 
            return "Empty";
    }

    public bool IsAiming() => isAiming;
    public bool IsReloading() => isReloading;
    public float GetAimMovementSlowdown() => (isAiming) ? aimMovementSlowdown : 0f;
}
