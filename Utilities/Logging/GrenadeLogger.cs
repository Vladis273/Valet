using UnityEngine;
using GameUtilities.Logging;

namespace GameUtilities.Logging
{
    /// <summary>
    /// Автоматический логгер для гранат
    /// Логирует бросок, полет и взрыв гранат
    /// </summary>
    public class GrenadeLogger : MonoBehaviour
    {
        [Header("Настройки")]
        [Tooltip("Тип гранаты для логов")]
        [SerializeField] private string grenadeType = "Fragmentation";
        
        [Tooltip("Имя бросившего для логов")]
        [SerializeField] private string throwerName = "Player";
        
        [Tooltip("Логировать каждую секунду полета")]
        [SerializeField] private bool logFlight = false;
        
        [Tooltip("Интервал логирования полета в секундах")]
        [SerializeField] private float flightLogInterval = 1f;

        private GameLogger _logger;
        private float _throwTime;
        private Vector3 _throwPosition;
        private bool _isInitialized;
        private float _lastFlightLogTime;
        private bool _hasExploded;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            _logger = GameLogger.Instance;
            _throwTime = Time.time;
            _throwPosition = transform.position;
            _hasExploded = false;
            _isInitialized = true;
        }

        /// <summary>
        /// Вызвать при броске гранаты
        /// </summary>
        public void LogThrow(Vector3 position, Vector3 velocity)
        {
            if (!_isInitialized) Initialize();
            
            _throwTime = Time.time;
            _throwPosition = position;
            
            _logger.Log(LogEventType.GrenadeThrow, "Grenade", 
                $"Grenade thrown: {grenadeType}", 
                $"Thrower: {throwerName}, Position: {position}, Velocity: {velocity}");
        }

        private void Update()
        {
            if (!_isInitialized || _hasExploded) return;

            // Логирование полета
            if (logFlight && Time.time - _lastFlightLogTime >= flightLogInterval)
            {
                _logger.Log(LogEventType.Custom, "Grenade", 
                    $"Grenade in flight: {grenadeType}", 
                    $"Position: {transform.position}, Time airborne: {Time.time - _throwTime:F2}s");
                _lastFlightLogTime = Time.time;
            }
        }

        /// <summary>
        /// Вызвать при взрыве гранаты
        /// </summary>
        public void LogExplosion(Vector3 position, int affectedTargets = 0, float damage = 0f)
        {
            if (!_isInitialized) Initialize();
            if (_hasExploded) return;
            
            _hasExploded = true;
            
            string details = $"Position: {position}, Thrower: {throwerName}";
            if (affectedTargets > 0)
            {
                details += $", Affected targets: {affectedTargets}";
            }
            if (damage > 0f)
            {
                details += $", Damage: {damage}";
            }
            
            _logger.Log(LogEventType.GrenadeThrow, "Grenade", 
                $"Grenade exploded: {grenadeType}", details);
                
            _logger.Log(LogEventType.TestEnd, "Grenade", 
                $"Grenade lifetime: {Time.time - _throwTime:F2}s");
        }

        /// <summary>
        /// Вызвать при попадании гранаты в цель (до взрыва)
        /// </summary>
        public void LogImpact(Vector3 position, string targetName)
        {
            if (!_isInitialized) Initialize();
            
            _logger.Log(LogEventType.Custom, "Grenade", 
                $"Grenade impacted: {targetName}", 
                $"Type: {grenadeType}, Position: {position}, Thrower: {throwerName}");
        }

        /// <summary>
        /// Вызвать при отскоке гранаты
        /// </summary>
        public void LogBounce(Vector3 position, float velocity)
        {
            if (!_isInitialized || !logFlight) return;
            
            _logger.Log(LogEventType.Custom, "Grenade", 
                $"Grenade bounced", 
                $"Type: {grenadeType}, Position: {position}, Velocity: {velocity}");
        }

        private void OnDestroy()
        {
            if (!_hasExploded)
            {
                _logger?.Log(LogEventType.Warning, "Grenade", 
                    $"Grenade destroyed without explosion: {grenadeType}", 
                    $"Possible error, Position: {transform.position}");
            }
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(grenadeType))
            {
                grenadeType = "Fragmentation";
            }
            
            if (string.IsNullOrEmpty(throwerName))
            {
                throwerName = "Unknown";
            }
        }
    }
}
