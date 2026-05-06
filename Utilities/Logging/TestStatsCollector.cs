using UnityEngine;
using GameUtilities.Logging;

namespace GameUtilities.Logging
{
    /// <summary>
    /// Глобальный менеджер для сбора статистики тестов и геймплея
    /// Агрегирует данные из всех логгеров и предоставляет сводную информацию
    /// </summary>
    public class TestStatsCollector : MonoBehaviour
    {
        [Header("Настройки")]
        [Tooltip("Автоматически выводить статистику при остановке сцены")]
        [SerializeField] private bool autoPrintOnStop = true;
        
        [Tooltip("Сохранять статистику в отдельный файл")]
        [SerializeField] private bool saveStatsToFile = true;

        private GameLogger _logger;
        private TestSessionStats _currentSession;
        private bool _isInitialized;

        [System.Serializable]
        public class TestSessionStats
        {
            public int totalTests;
            public int passedTests;
            public int failedTests;
            public int totalWeaponFires;
            public int totalReloads;
            public int totalGrenadeThrows;
            public int totalDamageDealt;
            public int totalDamageReceived;
            public int totalInventoryChanges;
            public float sessionDuration;
            public float startTime;
            
            public void Reset()
            {
                totalTests = 0;
                passedTests = 0;
                failedTests = 0;
                totalWeaponFires = 0;
                totalReloads = 0;
                totalGrenadeThrows = 0;
                totalDamageDealt = 0;
                totalDamageReceived = 0;
                totalInventoryChanges = 0;
                sessionDuration = 0f;
                startTime = Time.time;
            }
            
            public void CalculateDuration()
            {
                sessionDuration = Time.time - startTime;
            }
        }

        private static TestStatsCollector _instance;
        public static TestStatsCollector Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("TestStatsCollector");
                    _instance = go.AddComponent<TestStatsCollector>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

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

            _logger = GameLogger.Instance;
            _currentSession = new TestSessionStats();
            _currentSession.Reset();
            _isInitialized = true;

            // Подписка на события логгера
            _logger.OnLogEventAdded += HandleLogEvent;

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            #endif

            Debug.Log("[TestStatsCollector] Initialized");
        }

        private void HandleLogEvent(LogEvent logEvent)
        {
            switch (logEvent.type)
            {
                case LogEventType.TestStart:
                    _currentSession.totalTests++;
                    break;
                case LogEventType.TestPass:
                    _currentSession.passedTests++;
                    break;
                case LogEventType.TestFail:
                    _currentSession.failedTests++;
                    break;
                case LogEventType.WeaponFire:
                    _currentSession.totalWeaponFires++;
                    break;
                case LogEventType.WeaponReload:
                    _currentSession.totalReloads++;
                    break;
                case LogEventType.GrenadeThrow:
                    _currentSession.totalGrenadeThrows++;
                    break;
                case LogEventType.DamageDealt:
                    _currentSession.totalDamageDealt++;
                    break;
                case LogEventType.DamageReceived:
                    _currentSession.totalDamageReceived++;
                    break;
                case LogEventType.InventoryChange:
                    _currentSession.totalInventoryChanges++;
                    break;
            }
        }

        #if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                _currentSession.CalculateDuration();
                
                if (autoPrintOnStop)
                {
                    PrintStats();
                }
                
                if (saveStatsToFile)
                {
                    SaveStatsToFile();
                }
                
                // Сброс статистики для следующей сессии
                _currentSession.Reset();
            }
        }
        #endif

        /// <summary>
        /// Вывести статистику в консоль
        /// </summary>
        public void PrintStats()
        {
            _currentSession.CalculateDuration();
            
            Debug.Log("========================================");
            Debug.Log("       TEST & GAMEPLAY STATISTICS      ");
            Debug.Log("========================================");
            Debug.Log($"Session Duration: {_currentSession.sessionDuration:F2}s");
            Debug.Log("----------------------------------------");
            Debug.Log($"Total Tests: {_currentSession.totalTests}");
            Debug.Log($"Passed: {_currentSession.passedTests}");
            Debug.Log($"Failed: {_currentSession.failedTests}");
            if (_currentSession.totalTests > 0)
            {
                float passRate = (_currentSession.passedTests / (float)_currentSession.totalTests) * 100f;
                Debug.Log($"Pass Rate: {passRate:F1}%");
            }
            Debug.Log("----------------------------------------");
            Debug.Log($"Weapon Fires: {_currentSession.totalWeaponFires}");
            Debug.Log($"Reloads: {_currentSession.totalReloads}");
            Debug.Log($"Grenade Throws: {_currentSession.totalGrenadeThrows}");
            Debug.Log($"Damage Events (Dealt): {_currentSession.totalDamageDealt}");
            Debug.Log($"Damage Events (Received): {_currentSession.totalDamageReceived}");
            Debug.Log($"Inventory Changes: {_currentSession.totalInventoryChanges}");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Сохранить статистику в файл
        /// </summary>
        private void SaveStatsToFile()
        {
            _currentSession.CalculateDuration();
            
            string folderPath = System.IO.Path.Combine(Application.persistentDataPath, "GameLogs");
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
            
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = System.IO.Path.Combine(folderPath, $"Stats_{timestamp}.txt");
            
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
                {
                    writer.WriteLine("TEST & GAMEPLAY STATISTICS");
                    writer.WriteLine("========================================");
                    writer.WriteLine($"Timestamp: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"Session Duration: {_currentSession.sessionDuration:F2}s");
                    writer.WriteLine("");
                    writer.WriteLine("TEST RESULTS:");
                    writer.WriteLine($"  Total Tests: {_currentSession.totalTests}");
                    writer.WriteLine($"  Passed: {_currentSession.passedTests}");
                    writer.WriteLine($"  Failed: {_currentSession.failedTests}");
                    if (_currentSession.totalTests > 0)
                    {
                        float passRate = (_currentSession.passedTests / (float)_currentSession.totalTests) * 100f;
                        writer.WriteLine($"  Pass Rate: {passRate:F1}%");
                    }
                    writer.WriteLine("");
                    writer.WriteLine("GAMEPLAY ACTIONS:");
                    writer.WriteLine($"  Weapon Fires: {_currentSession.totalWeaponFires}");
                    writer.WriteLine($"  Reloads: {_currentSession.totalReloads}");
                    writer.WriteLine($"  Grenade Throws: {_currentSession.totalGrenadeThrows}");
                    writer.WriteLine($"  Damage Events (Dealt): {_currentSession.totalDamageDealt}");
                    writer.WriteLine($"  Damage Events (Received): {_currentSession.totalDamageReceived}");
                    writer.WriteLine($"  Inventory Changes: {_currentSession.totalInventoryChanges}");
                    writer.WriteLine("========================================");
                }
                
                Debug.Log($"[TestStatsCollector] Stats saved to: {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TestStatsCollector] Failed to save stats: {e.Message}");
            }
        }

        /// <summary>
        /// Получить текущую статистику
        /// </summary>
        public TestSessionStats GetCurrentStats()
        {
            _currentSession.CalculateDuration();
            return _currentSession;
        }

        /// <summary>
        /// Сбросить статистику
        /// </summary>
        public void ResetStats()
        {
            _currentSession.Reset();
            Debug.Log("[TestStatsCollector] Stats reset");
        }

        private void OnDestroy()
        {
            if (_logger != null)
            {
                _logger.OnLogEventAdded -= HandleLogEvent;
            }

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            #endif
        }
    }
}
