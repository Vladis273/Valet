using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Tests.Runtime
{
    /// <summary>
    /// Центральный менеджер для сбора и сохранения логов тестов и геймплея
    /// </summary>
    public class TestLogger : MonoBehaviour
    {
        private static TestLogger instance;
        private static string logFilePath;
        private static List<string> logEntries = new List<string>();
        
        // Пути для разных типов логов
        private const string BASE_PATH = "Assets/Tests/Runtime/Logs";
        
        public enum LogType
        {
            Test,
            Gameplay,
            Weapon,
            Player,
            Inventory,
            Grenade,
            Error
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLogFile();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeLogFile()
        {
            // Создаем директорию если не существует
            if (!Directory.Exists(BASE_PATH))
            {
                Directory.CreateDirectory(BASE_PATH);
            }
            
            // Генерируем имя файла с временной меткой
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            logFilePath = Path.Combine(BASE_PATH, $"test_log_{timestamp}.txt");
            
            Log(LogType.Test, "=== TEST SESSION STARTED ===");
        }

        public static void Log(LogType type, string message)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] [{type}] {message}";
            
            logEntries.Add(logEntry);
            Debug.Log(logEntry);
            
            // Сохраняем каждые 10 записей или по таймеру
            if (logEntries.Count % 10 == 0)
            {
                SaveLogs();
            }
        }

        public static void LogWeapon(string action, float value = 0f)
        {
            Log(LogType.Weapon, $"{action} (Value: {value:F2})");
        }

        public static void LogPlayer(string action, Vector3 position = default)
        {
            Log(LogType.Player, $"{action} at {position}");
        }

        public static void LogInventory(string action, string itemName = "")
        {
            Log(LogType.Inventory, $"{action}: {itemName}");
        }

        public static void LogGrenade(string action, string grenadeType = "")
        {
            Log(LogType.Grenade, $"{action}: {grenadeType}");
        }

        public static void LogError(string error)
        {
            Log(LogType.Error, $"ERROR: {error}");
        }

        private static void SaveLogs()
        {
            if (string.IsNullOrEmpty(logFilePath) || logEntries.Count == 0)
                return;

            try
            {
                File.WriteAllLines(logFilePath, logEntries);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save logs: {e.Message}");
            }
        }

        private void OnApplicationQuit()
        {
            SaveLogs();
            Log(LogType.Test, "=== TEST SESSION ENDED ===");
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SaveLogs();
            }
        }
    }
}
