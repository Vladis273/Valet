using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LightSide;
using LightSide.Core;

/// <summary>
/// Базовый контроллер оружия с полной поддержкой ScriptableObjects
/// и оптимизированной архитектурой
/// Интегрирован с MagazineManager для управления магазинами
/// </summary>
public abstract class WeaponController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Core References")]
    [Tooltip("Данные оружия из ScriptableObject - обязательно для настройки")]
    public WeaponData weaponData;
    public MagazineData magazineData;
    
    [Tooltip("Точка вылета пуль")]
    public Transform firePoint;

    // Visual effects
    public GameObject _shellPrefab;
    public Transform _ejectPoint;
    public float _shellForce;
    public float _shellUpward;
    public GameObject _tracerPrefab;
    public Color _tracerColor;
    public float _tracerWidth;
    public float _tracerLifetime;
    public float _tracerFadeSpeed;
    public bool _useShells;
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
    
    // Magazine Manager integration
    protected MagazineManager.MagazineState _currentMagazineState;
    protected string _weaponPoolId;
    #endregion

    #region Cached References
    protected PlayerMovement _playerMovement;
    protected ArmsController _dynamicArms;
    protected PlayerInput _playerInput;
    protected UniText _fireModeHint;
    protected UniText _ammoHint;
    protected Animator _animator;
    protected FollowCamera.CameraFollow _cameraFollow;
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
        var componentsTransform = GetComponentInParent<WeaponComponentsTransform>();
        if (componentsTransform == null)
        {
            Debug.LogError($"[WeaponController] WeaponComponentsTransform not found on {gameObject.name}");
            enabled = false;
            return;
        }
        
        _dynamicArms = componentsTransform.armController;
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
        }

        // Скрываем подсказки при старте
        if (_fireModeHint != null) 
            _fireModeHint.gameObject.SetActive(false);
        if (_ammoHint != null)
            _ammoHint.gameObject.SetActive(false);

        // Устанавливаем начальный режим огня
        _currentFireMode = weaponData.defaultFireMode;
        
        // Генерируем уникальный ID для пула магазинов этого оружия
        _weaponPoolId = $"{gameObject.name}_{GetInstanceID()}";
        
        // Регистрируем пул магазинов для этого оружия
        InitializeMagazinePool();
        
        // Инициализируем состояние
        _shotsFired = 0;
        _currentSpread = 0f;
        _isReloading = false;
        _isAiming = false;
    }
    
    /// <summary>
    /// Инициализирует пул магазинов для этого оружия через MagazineManager
    /// </summary>
    protected virtual void InitializeMagazinePool()
    {
        if (MagazineManager.Instance == null)
        {
            Debug.LogWarning($"[WeaponController] MagazineManager not found in scene. Using fallback ammo system for {gameObject.name}");
            _currentAmmo = _maxAmmo;
            _reserveAmmo = weaponData.maxReserveAmmo;
            return;
        }
        
        // Регистрируем новый пул с 3 магазинами
        MagazineManager.Instance.RegisterWeaponPool(_weaponPoolId, magazineData, 3);
        
        // Получаем первый магазин как активный
        _currentMagazineState = MagazineManager.Instance.GetCurrentMagazine(_weaponPoolId);
        
        if (_currentMagazineState != null)
        {
            _currentAmmo = _currentMagazineState.currentAmmo;
        }
        else
        {
            _currentAmmo = _maxAmmo;
        }
        
        _reserveAmmo = weaponData.maxReserveAmmo;
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
        // Basic stats
        _fireInterval = weaponData.FireInterval;
        _burstInterval = weaponData.BurstInterval;
        _maxAmmo = magazineData.magazineCapacity;
        _reserveAmmo = weaponData.maxReserveAmmo;
        
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
        
        // Reload
        _reloadTimeFull = weaponData.reloadTimeFull;
        _reloadTimeTactical = weaponData.reloadTimeTactical;
        
        // UI
        _hintDisplayTime = weaponData.hintDisplayTime;
        
        // Visual effects
        _shellPrefab = weaponData.shellPrefab;
        _ejectPoint = weaponData.ejectPoint;
        _shellForce = weaponData.shellForce;
        _shellUpward = weaponData.shellUpward;
        _tracerPrefab = weaponData.tracerPrefab;
        _tracerColor = weaponData.tracerColor;
        _tracerWidth = weaponData.tracerWidth;
        _tracerLifetime = weaponData.tracerLifetime;
        _tracerFadeSpeed = weaponData.tracerFadeSpeed;
        _useShells = weaponData.shellPrefab != null;
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
    /// <summary>
    /// Потребляет патрон из текущего магазина
    /// Возвращает true если патрон был успешно потреблен
    /// </summary>
    protected virtual bool ConsumeAmmo(int amount = 1)
    {
        if (_currentMagazineState != null)
        {
            int consumed = _currentMagazineState.ConsumeAmmo(amount);
            _currentAmmo = _currentMagazineState.currentAmmo;
            return consumed >= amount;
        }
        
        // Fallback без MagazineManager
        if (_currentAmmo >= amount)
        {
            _currentAmmo -= amount;
            return true;
        }
        return false;
    }
    
    protected virtual void ApplyRecoil()
    {
        Vector2 recoilImpulseLocal = _recoilImpulse;
        recoilImpulseLocal.y = Mathf.Min(recoilImpulseLocal.y, _maxArmRecoilOffset);

        _dynamicArms?.ApplyRecoilImpulse(recoilImpulseLocal, _recoilDuration, _recoilCurve);

        if (_playerMovement != null && _cameraFollow != null)
        {
            _cameraFollow.ApplyRecoilImpulse(_recoilImpulse * 0.3f);
        }
        
        // Событие выстрела
        EventBus.InvokeWeaponFired(weaponData);
    }

    /// <summary>
    /// Вычисляет направление выстрела с учётом разброса
    /// Оптимизировано: использует кэшированные значения
    /// </summary>
    protected virtual Vector3 GetFireDirection(float spreadAngle)
    {
        Vector3 baseDirection = firePoint.forward;

        // Первые выстрелы без разброса
        if (_shotsFired < GetAccurateShotsCount()) 
            return baseDirection;

        // Быстрый расчёт разброса без лишних тригонометрических функций
        float randomAngle = Random.value * Mathf.PI * 2f;
        float randomRadius = Mathf.Sqrt(Random.value) * spreadAngle;
        float spreadRad = randomRadius * Mathf.Deg2Rad;

        Vector3 right = firePoint.right;
        Vector3 up = firePoint.up;

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
        Vector3 startPoint = firePoint.position;
        
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
        // Нельзя перезаряжаться если магазин полон
        if (_currentAmmo >= _maxAmmo) return;
        
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
    /// Использует MagazineManager для переключения на следующий магазин
    /// </summary>
    protected virtual void FinishReload()
    {
        if (MagazineManager.Instance != null && _currentMagazineState != null)
        {
            // Если текущий магазин не полон, переключаемся на следующий с максимальным числом патронов
            if (!_currentMagazineState.isFull)
            {
                var nextMagazine = SwitchToBestMagazine();
                if (nextMagazine != null)
                {
                    _currentMagazineState = nextMagazine;
                    _currentAmmo = _currentMagazineState.currentAmmo;
                }
                else
                {
                    // Если нет магазинов с патронами, перезаряжаем текущий из резерва
                    ReloadCurrentMagazineFromReserve();
                }
            }
        }
        else
        {
            // Fallback без MagazineManager - просто заполняем магазин
            _currentAmmo = _maxAmmo;
        }

        _isReloading = false;
        _reloadCoroutine = null;
        
        TryShowAmmoHint();
        
        // Событие завершения перезарядки
        EventBus.InvokeReloadCompleted(weaponData);
    }
    
    /// <summary>
    /// Переключается на лучший доступный магазин (с максимальным числом патронов)
    /// Соответствует ТЗ: выбор магазина с максимальным числом патронов
    /// </summary>
    protected MagazineManager.MagazineState SwitchToBestMagazine()
    {
        if (MagazineManager.Instance == null || string.IsNullOrEmpty(_weaponPoolId))
            return null;
        
        var pool = MagazineManager.Instance.GetMagazinePool(_weaponPoolId);
        if (pool == null || pool.magazines.Count == 0)
            return null;
        
        // Находим магазин с максимальным числом патронов (не пустой)
        MagazineManager.MagazineState bestMagazine = null;
        int maxAmmoInMag = -1;
        
        for (int i = 0; i < pool.magazines.Count; i++)
        {
            var mag = pool.magazines[i];
            if (!mag.isEmpty && mag.currentAmmo > maxAmmoInMag)
            {
                maxAmmoInMag = mag.currentAmmo;
                bestMagazine = mag;
                pool.activeMagazineIndex = i;
            }
        }
        
        return bestMagazine;
    }
    
    /// <summary>
    /// Перезаряжает текущий магазин из резерва боеприпасов
    /// </summary>
    protected virtual void ReloadCurrentMagazineFromReserve()
    {
        if (_currentMagazineState == null) return;
        
        int ammoNeeded = _currentMagazineState.capacity - _currentMagazineState.currentAmmo;
        if (ammoNeeded <= 0 || _reserveAmmo <= 0) return;
        
        int ammoToTransfer = Mathf.Min(ammoNeeded, _reserveAmmo);
        _currentMagazineState.currentAmmo += ammoToTransfer;
        _currentAmmo = _currentMagazineState.currentAmmo;
        _reserveAmmo -= ammoToTransfer;
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
        if (!_useShells || _shellPrefab == null || _ejectPoint == null) return;

        GameObject shell = Instantiate(_shellPrefab, _ejectPoint.position, _ejectPoint.rotation);
        Rigidbody rb = shell.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            Vector3 direction = _ejectPoint.right * Random.Range(0.5f, 1.0f)
                              + _ejectPoint.up * Random.Range(0.3f, 0.7f)
                              + (-_ejectPoint.forward) * Random.Range(0.2f, 0.5f);

            rb.AddForce(direction * _shellForce + Vector3.up * _shellUpward, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }

        Destroy(shell, 5f);
    }

    protected virtual void CreateTracer(Vector3 start, Vector3 end)
    {
        if (_tracerPrefab == null) return;
        
        GameObject tracerObj = Instantiate(_tracerPrefab, start, Quaternion.identity);
        Tracer tracer = tracerObj.GetComponent<Tracer>();

        tracer?.Initialize(start, end, _tracerColor, _tracerWidth);
        
        if (tracer != null)
        {
            tracer.lifetime = _tracerLifetime;
            tracer.fadeSpeed = _tracerFadeSpeed;
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
    public int ReserveAmmo => _reserveAmmo;
    public MagazineManager.MagazineState CurrentMagazineState => _currentMagazineState;
    public string WeaponPoolId => _weaponPoolId;

    // Публичный доступ к текущему патрону для инвентаря
    public int currentAmmo
    {
        get => _currentAmmo;
        set
        {
            _currentAmmo = Mathf.Clamp(value, 0, _maxAmmo);
            // Синхронизируем с магазином если используется MagazineManager
            if (_currentMagazineState != null)
            {
                _currentMagazineState.currentAmmo = _currentAmmo;
            }
        }
    }

    /// <summary>
    /// Устанавливает текущее количество патронов (для подбора с земли)
    /// </summary>    
    public void SetCurrentAmmo(int amount)
    {
        _currentAmmo = Mathf.Clamp(amount, 0, _maxAmmo);
        if (_currentMagazineState != null)
        {
            _currentMagazineState.currentAmmo = _currentAmmo;
        }
        EventBus.InvokeAmmoChanged(weaponData, _currentAmmo, _maxAmmo);
    }

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
    
    /// <summary>
    /// Получает состояние магазинов для передачи при дропе оружия
    /// </summary>
    public List<MagazineManager.MagazineState> GetAllMagazineStates()
    {
        if (MagazineManager.Instance == null || string.IsNullOrEmpty(_weaponPoolId))
            return null;
            
        var pool = MagazineManager.Instance.GetMagazinePool(_weaponPoolId);
        return pool?.magazines;
    }
    #endregion
}
