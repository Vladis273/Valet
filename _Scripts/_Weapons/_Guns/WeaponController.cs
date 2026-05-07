using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LightSide;
using LightSide.Core;

/// <summary>
/// Базовый контроллер оружия с полной поддержкой ScriptableObjects
/// и оптимизированной архитектурой
/// </summary>
public abstract class WeaponController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Core References")]
    [Tooltip("Данные оружия из ScriptableObject - обязательно для настройки")]
    public WeaponData weaponData;
    #endregion

    #region Runtime State
    // Состояние боекомплекта
    protected int _currentAmmo;
    protected int _maxAmmo;
    protected int _reserveAmmo;
    
    // Состояние стрельбы
    protected int _shotsFired;
    protected float _currentSpread;
    protected float _lastFireTime;
    protected bool _isReloading;
    protected bool _isAiming;
    protected FireMode _currentFireMode;
    
    // UI таймер
    private float _hintTimer;
    private string _currentAmmoStatus;
    
    // Корутины
    protected Coroutine _reloadCoroutine;
    private AnimationCurve _recoilCurve;
    #endregion

    #region Cached References
    protected PlayerMovement _playerMovement;
    protected IPlayerStateProvider _playerStateProvider;
    protected ArmsController _dynamicArms;
    protected PlayerInput _playerInput;
    protected UniText _fireModeHint;
    protected UniText _ammoHint;
    protected Animator _animator;
    protected FollowCamera.CameraFollow _cameraFollow;
    protected WeaponComponentsTransform _componentsTransform;
    #endregion

    #region Cached Data from ScriptableObject
    // Кэшированные значения для производительности
    protected float _fireInterval;
    protected float _burstInterval;
    protected float _range;
    protected float _damage;
    protected Vector2 _recoilImpulse;
    protected float _recoilDuration;
    protected float _maxArmRecoilOffset;
    protected int _burstLength;
    protected int _accurateShots;
    protected float _maxSpread;
    protected float _spreadIncreaseRate;
    protected float _spreadRecoveryTime;
    protected float _aimAccurateShotsMultiplier;
    protected float _aimSpreadReduction;
    protected float _aimMovementSlowdown;
    protected float _reloadTimeFull;
    protected float _reloadTimeTactical;
    protected float _hintDisplayTime;
    protected List<FireMode> _availableFireModes;
    #endregion

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        // Кэшируем компоненты
        _componentsTransform = GetComponentInParent<WeaponComponentsTransform>();
        if (_componentsTransform == null)
        {
            Debug.LogError($"[WeaponController] WeaponComponentsTransform not found on {gameObject.name}");
            enabled = false;
            return;
        }
        
        _dynamicArms = _componentsTransform.armController;
        _animator = GetComponentInParent<Animator>();
        _recoilCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Валидация weaponData
        if (weaponData == null)
        {
            Debug.LogError($"[WeaponController] WeaponData not assigned on {gameObject.name}. Weapon disabled.");
            enabled = false;
            return;
        }

        // Инициализация из ScriptableObject
        InitializeFromData();
    }

    protected virtual void Start()
    {
        // Кэшируем ссылки на зависимости
        if (_dynamicArms?.playerData != null)
        {
            _playerMovement = _dynamicArms.playerData.playerMovement;
            _playerInput = _dynamicArms.playerData.playerInput;
            _fireModeHint = _dynamicArms.playerData.fireModeHint;
            _ammoHint = _dynamicArms.playerData.ammoHint;
            _cameraFollow = _dynamicArms.playerData.cameraFollow;
            
            // Получаем провайдер состояния игрока для определения позы
            _playerStateProvider = _playerMovement as IPlayerStateProvider;
        }

        // Скрываем подсказки при старте
        if (_fireModeHint != null) 
            _fireModeHint.gameObject.SetActive(false);
        if (_ammoHint != null)
            _ammoHint.gameObject.SetActive(false);

        // Устанавливаем начальный режим огня
        _currentFireMode = weaponData.defaultFireMode;
        
        // Инициализируем состояние
        _currentAmmo = _maxAmmo;
        _reserveAmmo = weaponData.maxReserveAmmo;
        _shotsFired = 0;
        _currentSpread = 0f;
        _isReloading = false;
        _isAiming = false;
    }

    protected virtual void Update()
    {
        HandleInput();
        UpdateSpread();
        UpdateUI();
    }

    protected virtual void OnEnable()
    {
        // Подписка на события (если нужно)
    }

    protected virtual void OnDisable()
    {
        if (_reloadCoroutine != null) 
            StopCoroutine(_reloadCoroutine);
    }

    protected virtual void OnDestroy()
    {
        // Отписка от событий
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Инициализирует все параметры оружия из ScriptableObject
    /// Вызывается один раз при Awake
    /// </summary>
    protected virtual void InitializeFromData()
    {
        // Magazine & Ammo - приоритет MagazineData, иначе legacy
        if (weaponData.magazineData != null)
        {
            _maxAmmo = weaponData.magazineData.capacity;
            _reloadTimeFull = weaponData.reloadTimeFullOverride > 0f 
                ? weaponData.reloadTimeFullOverride 
                : weaponData.magazineData.reloadTime;
            _reloadTimeTactical = weaponData.reloadTimeTacticalOverride > 0f 
                ? weaponData.reloadTimeTacticalOverride 
                : weaponData.magazineData.tacticalReloadTime;
        }
        else
        {
            // Legacy support для старых настроек
            _maxAmmo = weaponData.legacyMagazineCapacity;
        }
        
        _reserveAmmo = weaponData.maxReserveAmmo;
        
        // Basic stats
        _fireInterval = weaponData.FireInterval;
        _burstInterval = weaponData.BurstInterval;
        
        // Combat stats
        _range = weaponData.range;
        _damage = weaponData.damage;
        
        // Recoil
        _recoilImpulse = weaponData.recoilImpulse;
        _recoilDuration = weaponData.recoilDuration;
        _maxArmRecoilOffset = weaponData.maxArmRecoilOffset;
        
        // Fire modes
        _burstLength = weaponData.burstLength;
        _availableFireModes = new List<FireMode>(weaponData.availableFireModes);
        
        // Spread
        _accurateShots = weaponData.accurateShots;
        _maxSpread = weaponData.maxSpread;
        _spreadIncreaseRate = weaponData.spreadIncreaseRate;
        _spreadRecoveryTime = weaponData.spreadRecoveryTime;
        
        // Aiming
        _aimAccurateShotsMultiplier = weaponData.aimAccurateShotsMultiplier;
        _aimSpreadReduction = weaponData.aimSpreadReduction;
        _aimMovementSlowdown = weaponData.aimMovementSlowdown;
        
        // UI
        _hintDisplayTime = weaponData.hintDisplayTime;
    }
    #endregion

    #region Input Handling
    protected virtual void HandleInput()
    {
        ProcessReloadInput();
        
        if (_isReloading) return;

        ProcessFireModeInput();
        
        _isAiming = _playerInput?.aimHeld ?? false;
        
        ShouldFire();
    }

    private void ProcessReloadInput()
    {
        if (_playerInput?.reloadPressed == true && !_isReloading && _currentAmmo < _maxAmmo)
        {
            StartReload();
        }
    }

    private void ProcessFireModeInput()
    {
        if (_playerInput?.cycleFireModePressed == true && _availableFireModes.Count > 1)
        {
            SwitchFireMode();
        }
    }
    #endregion

    #region Abstract Methods
    protected abstract void Fire();
    protected abstract void ShouldFire();
    #endregion

    #region Firing System
    protected virtual void ApplyRecoil()
    {
        Vector2 recoilImpulseLocal = _recoilImpulse;
        recoilImpulseLocal.y = Mathf.Min(recoilImpulseLocal.y, _maxArmRecoilOffset);

        _dynamicArms?.ApplyRecoilImpulse(recoilImpulseLocal, _recoilDuration, _recoilCurve);

        if (_playerMovement != null && _cameraFollow != null)
        {
            // Получаем текущую позу игрока для модификации отдачи
            float poseMultiplier = GetPoseRecoilMultiplier();
            
            // Увеличенный множитель отдачи камеры (0.5 вместо 0.3)
            Vector2 cameraRecoil = _recoilImpulse * 0.5f * poseMultiplier;
            _cameraFollow.ApplyRecoilImpulse(cameraRecoil);
            
            // Добавляем тряску камеры в момент выстрела
            float shakeIntensity = GetRecoilShakeIntensity(poseMultiplier);
            _cameraFollow.AddInstantShake(shakeIntensity);
        }
        
        // Событие выстрела
        EventBus.InvokeWeaponFired(weaponData);
    }
    
    /// <summary>
    /// Возвращает множитель отдачи в зависимости от позы игрока
    /// </summary>
    private float GetPoseRecoilMultiplier()
    {
        if (_playerStateProvider == null) return 1f;
        
        PlayerPose currentPose = _playerStateProvider.GetCurrentPose();
        
        switch (currentPose)
        {
            case PlayerPose.Prone:
                // В положении лежа отдача меньше (упор)
                return 0.6f;
            case PlayerPose.Crouch:
            case PlayerPose.AltCrouch:
                // В приседе отдача средняя
                return 0.8f;
            default:
                // Стоя - полная отдача
                return 1f;
        }
    }
    
    /// <summary>
    /// Возвращает интенсивность тряски камеры при выстреле в зависимости от позы
    /// </summary>
    private float GetRecoilShakeIntensity(float poseMultiplier)
    {
        // Базовая интенсивность тряски от выстрела
        float baseShake = 0.08f;
        return baseShake * poseMultiplier;
    }

    /// <summary>
    /// Вычисляет направление выстрела с учётом разброса
    /// Оптимизировано: использует кэшированные значения
    /// </summary>
    protected virtual Vector3 GetFireDirection(float spreadAngle)
    {
        Vector3 baseDirection = _componentsTransform.firePoint.forward;

        // Первые выстрелы без разброса
        if (_shotsFired < GetAccurateShotsCount()) 
            return baseDirection;

        // Быстрый расчёт разброса без лишних тригонометрических функций
        float randomAngle = Random.Value * Mathf.PI * 2f;
        float randomRadius = Mathf.Sqrt(Random.Value) * spreadAngle;
        float spreadRad = randomRadius * Mathf.Deg2Rad;

        Vector3 right = _componentsTransform.firePoint.right;
        Vector3 up = _componentsTransform.firePoint.up;

        float cos = Mathf.Cos(randomAngle);
        float sin = Mathf.Sin(randomAngle);
        float tan = Mathf.Tan(spreadRad);

        Vector3 spreadDirection = baseDirection + right * (tan * cos) + up * (tan * sin);

        return spreadDirection.normalized;
    }

    /// <summary>
    /// Выполняет hit-scan проверку попадания
    /// </summary>
    protected virtual void PerformHitScan(Vector3 direction)
    {
        Vector3 startPoint = _componentsTransform.firePoint.position;
        
        if (Physics.Raycast(startPoint, direction, out RaycastHit hit, _range))
        {
            Debug.DrawRay(startPoint, direction * hit.distance, Color.red, 0.5f);
            CreateTracer(startPoint, hit.point);
            
            // Попытка применить урон через интерфейс
            var damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(_damage, hit.point, direction);
        }
        else
        {
            Debug.DrawRay(startPoint, direction * _range, Color.yellow, 0.5f);
            CreateTracer(startPoint, startPoint + direction * _range);
        }
    }

    protected virtual float CalculateSpread()
    {
        float spread = _currentSpread;
        if (_isAiming) spread *= _aimSpreadReduction;
        return spread;
    }

    protected virtual int GetAccurateShotsCount()
    {
        int baseAccurate = _accurateShots;
        if (_isAiming) 
            baseAccurate = Mathf.RoundToInt(_accurateShots * _aimAccurateShotsMultiplier);
        return baseAccurate;
    }

    /// <summary>
    /// Обновляет разброс оружия
    /// Оптимизировано: проверяет время только когда нужно
    /// </summary>
    protected virtual void UpdateSpread()
    {
        if (_currentSpread <= 0f) return;
        
        float timeSinceLastShot = Time.time - _lastFireTime;
        if (timeSinceLastShot <= 0.1f) return;

        float recoveryRate = _maxSpread / _spreadRecoveryTime;
        _currentSpread = Mathf.Max(0f, _currentSpread - recoveryRate * Time.deltaTime);
    }
    #endregion

    #region Reload System
    /// <summary>
    /// Начинает перезарядку
    /// </summary>
    protected virtual void StartReload()
    {
        _isReloading = true;
        _currentSpread = 0f;
        _shotsFired = 0;

        bool isTactical = _currentAmmo > 0;
        float reloadDuration = isTactical ? _reloadTimeTactical : _reloadTimeFull;
        string reloadAnim = isTactical ? "TacticalReload" : "Reload";

        _animator?.SetTrigger(reloadAnim);
        
        // Событие начала перезарядки
        EventBus.InvokeReloadStarted(weaponData);
        
        _reloadCoroutine = StartCoroutine(ReloadRoutine(reloadDuration));
    }

    protected virtual IEnumerator ReloadRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        FinishReload();
    }

    /// <summary>
    /// Завершает перезарядку
    /// Логика: при тактической перезарядке сохраняется патрон в патроннике
    /// </summary>
    protected virtual void FinishReload()
    {
        // Полная перезарядка магазина
        _currentAmmo = _maxAmmo;

        _isReloading = false;
        _reloadCoroutine = null;
        
        TryShowAmmoHint();
        
        // Событие завершения перезарядки
        EventBus.InvokeReloadCompleted(weaponData);
    }
    #endregion

    #region Fire Mode System
    protected virtual void SwitchFireMode()
    {
        int currentIndex = _availableFireModes.IndexOf(_currentFireMode);
        int nextIndex = (currentIndex + 1) % _availableFireModes.Count;
        _currentFireMode = _availableFireModes[nextIndex];

        ShowFireModeHint();
        
        // Событие смены режима
        EventBus.InvokeFireModeChanged(weaponData, _currentFireMode);
    }
    #endregion

    #region Visual Effects
    protected virtual void EjectShell()
    {
        if (_componentsTransform.shellPrefab == null || _componentsTransform.ejectPoint == null) return;

        GameObject shell = Instantiate(_componentsTransform.shellPrefab, _componentsTransform.ejectPoint.position, _componentsTransform.ejectPoint.rotation);
        Rigidbody rb = shell.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            Vector3 direction = _componentsTransform.ejectPoint.right * Random.Range(0.5f, 1.0f)
                              + _componentsTransform.ejectPoint.up * Random.Range(0.3f, 0.7f)
                              + (-_componentsTransform.ejectPoint.forward) * Random.Range(0.2f, 0.5f);

            rb.AddForce(direction * _componentsTransform.shellForce + Vector3.up * _componentsTransform.shellUpward, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        Destroy(shell, 5f);
    }

    protected virtual void CreateTracer(Vector3 start, Vector3 end)
    {
        if (_componentsTransform.tracerPrefab == null) return;
        
        GameObject tracerObj = Instantiate(_componentsTransform.tracerPrefab, start, Quaternion.identity);
        Tracer tracer = tracerObj.GetComponent<Tracer>();

        tracer?.Initialize(start, end, _componentsTransform.tracerColor, _componentsTransform.tracerWidth);
        
        if (tracer != null)
        {
            tracer.lifetime = _componentsTransform.tracerLifetime;
            tracer.fadeSpeed = _componentsTransform.tracerFadeSpeed;
        }
    }
    #endregion

    #region UI System
    protected virtual void UpdateUI()
    {
        if (_hintTimer <= 0f) return;

        _hintTimer -= Time.deltaTime;
        
        if (_hintTimer <= 0f)
        {
            _fireModeHint?.gameObject.SetActive(false);
            _ammoHint?.gameObject.SetActive(false);
        }
    }

    protected virtual void ShowFireModeHint()
    {
        if (_fireModeHint == null) return;

        string modeName = _currentFireMode switch
        {
            FireMode.Single => "Одиночный",
            FireMode.Burst => $"Очередь ({_burstLength})",
            FireMode.Auto => "Автомат",
            _ => "Неизвестно"
        };

        _fireModeHint.Text = $"Режим огня: {modeName}";
        _fireModeHint.gameObject.SetActive(true);
        _hintTimer = _hintDisplayTime;
    }

    protected virtual void TryShowAmmoHint()
    {
        string newStatus = GetAmmoStatusText();
        if (newStatus == _currentAmmoStatus) return;

        _currentAmmoStatus = newStatus;
        _ammoHint.Text = $"Магазин: {newStatus}";
        _ammoHint.gameObject.SetActive(true);
        _hintTimer = _hintDisplayTime;
        
        // Событие изменения патронов
        EventBus.InvokeAmmoChanged(weaponData, _currentAmmo, _maxAmmo);
    }

    protected virtual string GetAmmoStatusText()
    {
        float percent = (float)_currentAmmo / _maxAmmo * 100f;

        return percent switch
        {
            >= 100f => "Full",
            >= 50f => "Half-full",
            > 1f => "Half-empty",
            _ => "Empty"
        };
    }
    #endregion

    #region Public API
    public bool IsAiming() => _isAiming;
    public bool IsReloading() => _isReloading;
    public float GetAimMovementSlowdown() => _isAiming ? _aimMovementSlowdown : 0f;
    public int CurrentAmmo => _currentAmmo;
    public int MaxAmmo => _maxAmmo;
    public FireMode CurrentFireMode => _currentFireMode;
    
    /// <summary>
    /// Добавляет патроны в резерв (для подбора с земли)
    /// </summary>
    public bool AddReserveAmmo(int amount)
    {
        int oldReserve = _reserveAmmo;
        _reserveAmmo = Mathf.Min(_reserveAmmo + amount, weaponData.maxReserveAmmo);
        return _reserveAmmo > oldReserve;
    }
    
    /// <summary>
    /// Тратит патроны из резерва для перезарядки
    /// </summary>
    public int TakeFromReserve(int needed)
    {
        int taken = Mathf.Min(needed, _reserveAmmo);
        _reserveAmmo -= taken;
        return taken;
    }
    #endregion
}
