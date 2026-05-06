using UnityEngine;
using LightSide.Core;

/// <summary>
/// Компонент здоровья игрока с поддержкой регенерации и состояний
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [Tooltip("Максимальное здоровье игрока")]
    public float maxHealth = 100f;
    
    [Tooltip("Скорость регенерации здоровья в секунду")]
    public float healthRegenRate = 5f;
    
    [Tooltip("Задержка перед началом регенерации после получения урона")]
    public float regenDelay = 3f;
    
    [Tooltip("Минимальное здоровье для начала регенерации (порог)")]
    public float regenThreshold = 30f;
    
    [Header("Downed State")]
    [Tooltip("Состояние 'ранен' - игрок не может двигаться, но ещё жив")]
    public bool enableDownedState = true;
    
    [Tooltip("Здоровье при котором игрок переходит в состояние 'ранен'")]
    public float downedHealthThreshold = 0f;
    
    [Header("Damage Feedback")]
    public float damageFlashDuration = 0.3f;
    public Color damageFlashColor = Color.red;
    
    #region Private State
    private float _currentHealth;
    private float _lastDamageTime;
    private bool _isRegenerating;
    private bool _isDowned;
    private bool _isDead;
    
    private PlayerMovement _playerMovement;
    private PlayerInput _playerInput;
    private ArmsController _armsController;
    
    // Кэширование для оптимизации
    private float _cachedMaxHealth;
    private int _frameCounter;
    #endregion
    
    #region Public Properties
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _cachedMaxHealth;
    public bool IsAlive => !_isDead;
    public bool IsDowned => _isDowned;
    public bool IsRegenerating => _isRegenerating && Time.time - _lastDamageTime > regenDelay;
    #endregion
    
    #region Unity Lifecycle
    void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerInput = GetComponent<PlayerInput>();
        _armsController = GetComponentInChildren<ArmsController>();
        
        _cachedMaxHealth = maxHealth;
    }
    
    void Start()
    {
        _currentHealth = _cachedMaxHealth;
        _isDowned = false;
        _isDead = false;
        _isRegenerating = false;
        _lastDamageTime = -regenDelay; // Разрешить регенерацию сразу при старте
    }
    
    void Update()
    {
        if (_isDead || _isDowned) return;
        
        HandleRegeneration();
    }
    #endregion
    
    #region Damage System
    /// <summary>
    /// Получить урон
    /// </summary>
    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (_isDead || _isDowned) return;
        
        // Применяем урон
        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        _lastDamageTime = Time.time;
        _isRegenerating = false;
        
        // Событие получения урона
        EventBus.InvokePlayerHealthChanged(_currentHealth / _cachedMaxHealth * 100f);
        
        // Проверяем состояние
        if (_currentHealth <= 0f)
        {
            HandleDeath(hitPoint, hitDirection);
        }
        else if (enableDownedState && _currentHealth <= downedHealthThreshold)
        {
            EnterDownedState();
        }
        else
        {
            // Обратная связь о получении урона
            OnDamageReceived(hitPoint, hitDirection);
        }
    }
    
    /// <summary>
    /// Лечение игрока
    /// </summary>
    public void Heal(float amount)
    {
        if (_isDead || _isDowned) return;
        
        float oldHealth = _currentHealth;
        _currentHealth = Mathf.Min(_cachedMaxHealth, _currentHealth + amount);
        
        if (_currentHealth > oldHealth && _currentHealth < _cachedMaxHealth)
        {
            _isRegenerating = true;
        }
        
        EventBus.InvokePlayerHealthChanged(_currentHealth / _cachedMaxHealth * 100f);
    }
    
    /// <summary>
    /// Мгновенное восстановление до полного здоровья
    /// </summary>
    public void FullyHeal()
    {
        if (_isDead || _isDowned) return;
        
        _currentHealth = _cachedMaxHealth;
        _isRegenerating = false;
        _lastDamageTime = -regenDelay;
        
        EventBus.InvokePlayerHealthChanged(100f);
    }
    #endregion
    
    #region Regeneration
    private void HandleRegeneration()
    {
        float timeSinceDamage = Time.time - _lastDamageTime;
        
        if (timeSinceDamage < regenDelay) return;
        
        if (_currentHealth < _cachedMaxHealth && _currentHealth > regenThreshold)
        {
            _currentHealth = Mathf.Min(_cachedMaxHealth, _currentHealth + healthRegenRate * Time.deltaTime);
            _isRegenerating = true;
            
            // Оптимизация: обновляем событие не каждый кадр
            if (++_frameCounter % 10 == 0)
            {
                EventBus.InvokePlayerHealthChanged(_currentHealth / _cachedMaxHealth * 100f);
            }
        }
    }
    #endregion
    
    #region Downed & Death States
    private void EnterDownedState()
    {
        if (!enableDownedState) return;
        
        _isDowned = true;
        
        // Отключаем управление
        if (_playerInput != null)
            _playerInput.enabled = false;
        
        // TODO: Анимация падения, переход в режим ползания
        Debug.Log("Игрок ранен! Требуется помощь союзников.");
    }
    
    /// <summary>
    /// Воскрешение из состояния 'ранен'
    /// </summary>
    public void Revive(float reviveHealth = 30f)
    {
        if (!_isDowned && !_isDead) return;
        
        _isDowned = false;
        _isDead = false;
        _currentHealth = reviveHealth;
        _lastDamageTime = -regenDelay;
        
        // Включаем управление
        if (_playerInput != null)
            _playerInput.enabled = true;
        
        EventBus.InvokePlayerHealthChanged(_currentHealth / _cachedMaxHealth * 100f);
        Debug.Log("Игрок воскрешён!");
    }
    
    private void HandleDeath(Vector3 hitPoint, Vector3 hitDirection)
    {
        _isDead = true;
        _currentHealth = 0f;
        
        // Отключаем управление
        if (_playerInput != null)
            _playerInput.enabled = false;
        
        // Событие смерти
        EventBus.InvokePlayerDied();
        
        Debug.Log("Игрок погиб!");
        
        // TODO: Здесь будет логика смены тела/респавна
    }
    #endregion
    
    #region Feedback
    private void OnDamageReceived(Vector3 hitPoint, Vector3 hitDirection)
    {
        // TODO: Добавить визуальные эффекты попадания
        // - Вспышка экрана
        // - Звуковой эффект
        // - Кровь/частицы
        
        Debug.Log($"Получен урон: {hitDirection}, точка: {hitPoint}");
    }
    #endregion
    
    #region Public API
    /// <summary>
    /// Добавляет максимальное здоровье (бонус от перков)
    /// </summary>
    public void AddMaxHealth(float amount)
    {
        _cachedMaxHealth += amount;
        maxHealth = _cachedMaxHealth;
        
        if (_currentHealth > _cachedMaxHealth)
            _currentHealth = _cachedMaxHealth;
            
        EventBus.InvokePlayerHealthChanged(_currentHealth / _cachedMaxHealth * 100f);
    }
    
    /// <summary>
    /// Устанавливает множитель регенерации (от способностей)
    /// </summary>
    public void SetRegenMultiplier(float multiplier)
    {
        healthRegenRate = 5f * multiplier; // Базовая скорость 5
    }
    #endregion
}
