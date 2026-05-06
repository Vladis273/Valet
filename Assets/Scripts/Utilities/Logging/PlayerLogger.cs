using UnityEngine;
using GameUtilities.Logging;

namespace GameUtilities.Logging
{
    /// <summary>
    /// Автоматический логгер для игрока
    /// Логирует движения, прыжки, взаимодействие и другие действия
    /// </summary>
    public class PlayerLogger : MonoBehaviour
    {
        [Header("Настройки")]
        [Tooltip("Ссылка на PlayerMovement (если не назначена, попробует найти автоматически)")]
        [SerializeField] private MonoBehaviour playerMovement;
        
        [Tooltip("Имя игрока для логов")]
        [SerializeField] private string playerName = "Player";
        
        [Tooltip("Логировать каждое движение (может быть много данных)")]
        [SerializeField] private bool logEveryMove = false;
        
        [Tooltip("Интервал логирования движений в секундах (если logEveryMove = false)")]
        [SerializeField] private float moveLogInterval = 1f;

        private GameLogger _logger;
        private Vector3 _lastPosition;
        private bool _isInitialized;
        private float _lastMoveLogTime;
        private bool _wasGrounded;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            _logger = GameLogger.Instance;
            
            if (playerMovement == null)
            {
                playerMovement = GetComponent<MonoBehaviour>();
            }

            _lastPosition = transform.position;
            _wasGrounded = true; // Предполагаем, что игрок на земле при старте
            _isInitialized = true;
            
            _logger.Log(LogEventType.TestStart, "Player", $"Player logger initialized: {playerName}");
        }

        private void Update()
        {
            if (!_isInitialized) return;

            // Проверка движения
            if (Vector3.Distance(transform.position, _lastPosition) > 0.01f)
            {
                if (logEveryMove || Time.time - _lastMoveLogTime >= moveLogInterval)
                {
                    LogMovement();
                    _lastMoveLogTime = Time.time;
                }
                _lastPosition = transform.position;
            }
        }

        private void LogMovement()
        {
            _logger.LogPlayerAction($"Moving", transform.position);
        }

        /// <summary>
        /// Вызвать при прыжке
        /// </summary>
        public void LogJump()
        {
            if (!_isInitialized) Initialize();
            _logger.Log(LogEventType.PlayerJump, "Player", 
                $"Jumped: {playerName}", 
                $"Position: {transform.position}, Velocity Y: {GetVerticalVelocity()}");
        }

        /// <summary>
        /// Вызвать при приземлении
        /// </summary>
        public void LogLand()
        {
            if (!_isInitialized) Initialize();
            _logger.Log(LogEventType.GameplayAction, "Player", 
                $"Landed: {playerName}", 
                $"Position: {transform.position}");
        }

        /// <summary>
        /// Вызвать при взаимодействии
        /// </summary>
        public void LogInteraction(string objectType, string action)
        {
            if (!_isInitialized) Initialize();
            _logger.LogGameplayAction($"Interacted with {objectType}: {action}", 
                $"Player: {playerName}, Position: {transform.position}");
        }

        /// <summary>
        /// Вызвать при получении урона
        /// </summary>
        public void LogDamageReceived(float damage, string source)
        {
            if (!_isInitialized) Initialize();
            _logger.Log(LogEventType.DamageReceived, "Combat", 
                $"Player received damage", 
                $"Amount: {damage}, Source: {source}, Health: {GetCurrentHealth()}");
        }

        /// <summary>
        /// Вызвать при смене оружия
        /// </summary>
        public void LogWeaponSwitch(string oldWeapon, string newWeapon)
        {
            if (!_isInitialized) Initialize();
            _logger.LogInventoryChange(newWeapon, "Switched Weapon");
            _logger.LogGameplayAction($"Weapon switched from {oldWeapon} to {newWeapon}", 
                $"Player: {playerName}");
        }

        /// <summary>
        /// Вызвать при броске гранаты
        /// </summary>
        public void LogGrenadeThrow(string grenadeType)
        {
            if (!_isInitialized) Initialize();
            _logger.Log(LogEventType.GrenadeThrow, "Player", 
                $"Threw grenade: {grenadeType}", 
                $"Player: {playerName}, Position: {transform.position}");
        }

        #region Helper Methods
        
        private float GetVerticalVelocity()
        {
            // Заглушка - нужно адаптировать под реальный PlayerMovement
            // Можно получить через рефлексию или интерфейс
            Rigidbody rb = GetComponent<Rigidbody>();
            return rb != null ? rb.velocity.y : 0f;
        }

        private string GetCurrentHealth()
        {
            // Заглушка - нужно адаптировать под реальную систему здоровья
            return "Unknown";
        }

        #endregion

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = gameObject.name;
            }
        }

        private void OnDestroy()
        {
            _logger?.Log(LogEventType.TestEnd, "Player", $"Player logger destroyed: {playerName}");
        }
    }
}
