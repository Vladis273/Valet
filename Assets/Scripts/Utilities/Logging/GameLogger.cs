using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameUtilities.Logging
{
    /// <summary>
    /// Типы событий для логирования
    /// </summary>
    public enum LogEventType
    {
        TestStart,
        TestEnd,
        TestPass,
        TestFail,
        GameplayAction,
        WeaponFire,
        WeaponReload,
        PlayerMove,
        PlayerJump,
        InventoryChange,
        GrenadeThrow,
        DamageDealt,
        DamageReceived,
        Error,
        Warning,
        Custom
    }

    /// <summary>
    /// Структура события лога
    /// </summary>
    [Serializable]
    public class LogEvent
    {
        public long timestamp;
        public string timestampFormatted;
        public LogEventType type;
        public string category;
        public string message;
        public string details;
        public float gameTime;
        public int frameCount;
        
        public LogEvent(LogEventType type, string category, string message, string details = "")
        {
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            this.timestampFormatted = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            this.type = type;
            this.category = category;
            this.message = message;
            this.details = details;
            this.gameTime = Time.time;
            this.frameCount = Time.frameCount;
        }
        
        public override string ToString()
        {
            return $"[{timestampFormatted}] [{type}] [{category}] {message}" + 
                   (string.IsNullOrEmpty(details) ? "" : $" | {details}");
        }
        
        public string ToCSV()
        {
            return $"{timestamp},{timestampFormatted},{type},{category},\"{message}\",\"{details}\",{gameTime},{frameCount}";
        }
    }

    /// <summary>
    /// Менеджер логирования для тестов и геймплея
    /// </summary>
    public class GameLogger : MonoBehaviour
    {
        private static GameLogger _instance;
        public static GameLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("GameLogger");
                    _instance = go.AddComponent<GameLogger>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Настройки логирования")]
        [Tooltip("Включить логирование в редакторе")]
        [SerializeField] private bool enableInEditor = true;
        
        [Tooltip("Включить логирование во время игры")]
        [SerializeField] private bool enableInGame = true;
        
        [Tooltip("Путь для сохранения логов (относительно Application.persistentDataPath)")]
        [SerializeField] private string logFolderPath = "GameLogs";
        
        [Tooltip("Максимальный размер файла лога в МБ перед созданием нового")]
        [SerializeField] private int maxLogFileSizeMB = 10;
        
        [Tooltip("Автоматически сохранять лог при остановке сцены")]
        [SerializeField] private bool autoSaveOnStop = true;

        private List<LogEvent> _currentSessionEvents = new List<LogEvent>();
        private string _currentLogFilePath;
        private bool _isInitialized;
        private object _lockObject = new object();

        // События для подписки
        public event Action<LogEvent> OnLogEventAdded;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;
            
            string fullPath = Path.Combine(Application.persistentDataPath, logFolderPath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            
            _currentLogFilePath = GetNewLogFilePath(fullPath);
            _isInitialized = true;
            
            Log(LogEventType.TestStart, "System", "Logger initialized", $"Log path: {_currentLogFilePath}");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            #endif
        }

        private string GetNewLogFilePath(string folderPath)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"GameLog_{timestamp}.csv";
            return Path.Combine(folderPath, fileName);
        }

        #if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode && autoSaveOnStop)
            {
                SaveLog();
                Log(LogEventType.TestEnd, "System", "Play mode stopped, log saved");
            }
        }
        #endif

        /// <summary>
        /// Записать событие в лог
        /// </summary>
        public void Log(LogEventType type, string category, string message, string details = "")
        {
            if (!ShouldLog()) return;
            
            var logEvent = new LogEvent(type, category, message, details);
            
            lock (_lockObject)
            {
                _currentSessionEvents.Add(logEvent);
            }
            
            OnLogEventAdded?.Invoke(logEvent);
            
            // Вывод в консоль для важных событий
            if (type == LogEventType.Error || type == LogEventType.Warning || type == LogEventType.TestFail)
            {
                Debug.LogWarning(logEvent.ToString());
            }
            else if (type == LogEventType.TestPass)
            {
                Debug.Log(logEvent.ToString());
            }
        }

        /// <summary>
        /// Записать событие теста (начало)
        /// </summary>
        public void LogTestStart(string testName, string description = "")
        {
            Log(LogEventType.TestStart, "Test", $"Test started: {testName}", description);
        }

        /// <summary>
        /// Записать событие теста (успех)
        /// </summary>
        public void LogTestPass(string testName, string details = "")
        {
            Log(LogEventType.TestPass, "Test", $"Test passed: {testName}", details);
        }

        /// <summary>
        /// Записать событие теста (провал)
        /// </summary>
        public void LogTestFail(string testName, string reason, string details = "")
        {
            Log(LogEventType.TestFail, "Test", $"Test failed: {testName}", $"Reason: {reason}. {details}");
        }

        /// <summary>
        /// Записать игровое действие
        /// </summary>
        public void LogGameplayAction(string action, string details = "")
        {
            Log(LogEventType.GameplayAction, "Gameplay", action, details);
        }

        /// <summary>
        /// Сохранить текущий лог в файл
        /// </summary>
        public void SaveLog()
        {
            lock (_lockObject)
            {
                if (_currentSessionEvents.Count == 0)
                {
                    Debug.Log("[GameLogger] No events to save.");
                    return;
                }

                try
                {
                    // Проверка размера файла
                    if (File.Exists(_currentLogFilePath))
                    {
                        FileInfo fileInfo = new FileInfo(_currentLogFilePath);
                        if (fileInfo.Length > maxLogFileSizeMB * 1024 * 1024)
                        {
                            _currentLogFilePath = GetNewLogFilePath(Path.GetDirectoryName(_currentLogFilePath));
                            Debug.Log($"[GameLogger] Log file size exceeded. Created new file: {_currentLogFilePath}");
                        }
                    }

                    bool fileExists = File.Exists(_currentLogFilePath);
                    
                    using (StreamWriter writer = new StreamWriter(_currentLogFilePath, true))
                    {
                        // Запись заголовка если файл новый
                        if (!fileExists)
                        {
                            writer.WriteLine("Timestamp,DateTime,Type,Category,Message,Details,GameTime,FrameCount");
                        }
                        
                        foreach (var logEvent in _currentSessionEvents)
                        {
                            writer.WriteLine(logEvent.ToCSV());
                        }
                    }
                    
                    Debug.Log($"[GameLogger] Log saved to: {_currentLogFilePath} ({_currentSessionEvents.Count} events)");
                    _currentSessionEvents.Clear();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[GameLogger] Failed to save log: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Получить все события текущей сессии
        /// </summary>
        public List<LogEvent> GetCurrentSessionEvents()
        {
            lock (_lockObject)
            {
                return new List<LogEvent>(_currentSessionEvents);
            }
        }

        /// <summary>
        /// Очистить текущую сессию
        /// </summary>
        public void ClearCurrentSession()
        {
            lock (_lockObject)
            {
                _currentSessionEvents.Clear();
            }
        }

        private bool ShouldLog()
        {
            #if UNITY_EDITOR
            return enableInEditor;
            #else
            return enableInGame;
            #endif
        }

        private void OnDestroy()
        {
            if (autoSaveOnStop)
            {
                SaveLog();
            }
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            #endif
        }
    }

    /// <summary>
    /// Расширения для удобного логирования
    /// </summary>
    public static class GameLoggerExtensions
    {
        public static void LogWeaponFire(this GameLogger logger, string weaponName, int ammoRemaining, Vector3 hitPoint = default)
        {
            string details = $"Ammo: {ammoRemaining}";
            if (hitPoint != default)
            {
                details += $", Hit: {hitPoint}";
            }
            logger.Log(LogEventType.WeaponFire, "Weapon", $"Fired: {weaponName}", details);
        }

        public static void LogWeaponReload(this GameLogger logger, string weaponName, int ammoBefore, int ammoAfter, bool tactical)
        {
            logger.Log(LogEventType.WeaponReload, "Weapon", 
                $"Reloaded: {weaponName}", 
                $"Before: {ammoBefore}, After: {ammoAfter}, Tactical: {tactical}");
        }

        public static void LogInventoryChange(this GameLogger logger, string itemName, string action, int slotIndex = -1)
        {
            string details = slotIndex >= 0 ? $"Slot: {slotIndex}" : "";
            logger.Log(LogEventType.InventoryChange, "Inventory", $"{action}: {itemName}", details);
        }

        public static void LogPlayerAction(this GameLogger logger, string action, Vector3 position)
        {
            logger.Log(LogEventType.PlayerMove, "Player", action, $"Position: {position}");
        }

        public static void LogDamage(this GameLogger logger, string target, float damage, string source = "")
        {
            logger.Log(LogEventType.DamageDealt, "Combat", 
                $"Damage dealt to {target}", 
                $"Amount: {damage}, Source: {source}");
        }
    }
}
