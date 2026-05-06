using UnityEngine;

namespace Tests.Runtime
{
    /// <summary>
    /// Автоматический сборщик статистики во время игры и тестов
    /// Прикрепите этот скрипт к любому объекту на сцене для сбора данных
    /// </summary>
    public class GameplayStatsCollector : MonoBehaviour
    {
        [Header("Настройки сбора")]
        public bool collectWeaponStats = true;
        public bool collectPlayerStats = true;
        public bool collectInventoryStats = true;
        public bool collectGrenadeStats = true;
        
        [Header("Интервал сохранения (сек)")]
        public float saveInterval = 30f;
        
        private float lastSaveTime;
        private int shotsFired;
        private int shotsHit;
        private int reloadsPerformed;
        private int grenadesThrown;
        private int weaponsPickedUp;
        private int weaponsDropped;
        private float totalDistanceMoved;
        private Vector3 lastPlayerPosition;
        private int playerDeaths;
        private int enemiesKilled;

        private void Start()
        {
            lastSaveTime = Time.time;
            if (collectPlayerStats && GameObject.FindGameObjectWithTag("Player") != null)
            {
                lastPlayerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
            }
            
            TestLogger.Log(TestLogger.LogType.Gameplay, "Stats collector initialized");
        }

        private void Update()
        {
            // Авто-сохранение по таймеру
            if (Time.time - lastSaveTime >= saveInterval)
            {
                SaveCurrentStats();
                lastSaveTime = Time.time;
            }
            
            // Сбор статистики движения игрока
            if (collectPlayerStats)
            {
                CollectPlayerMovement();
            }
        }

        private void CollectPlayerMovement()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(player.transform.position, lastPlayerPosition);
                totalDistanceMoved += distance;
                lastPlayerPosition = player.transform.position;
            }
        }

        // Публичные методы для вызова из других скриптов
        public void RecordShot(bool hit)
        {
            shotsFired++;
            if (hit) shotsHit++;
            if (collectWeaponStats)
                TestLogger.LogWeapon("Shot recorded", hit ? 1f : 0f);
        }

        public void RecordReload()
        {
            reloadsPerformed++;
            if (collectWeaponStats)
                TestLogger.LogWeapon("Reload performed");
        }

        public void RecordGrenadeThrow()
        {
            grenadesThrown++;
            if (collectGrenadeStats)
                TestLogger.LogGrenade("Grenade thrown");
        }

        public void RecordWeaponPickup()
        {
            weaponsPickedUp++;
            if (collectInventoryStats)
                TestLogger.LogInventory("Weapon picked up");
        }

        public void RecordWeaponDrop()
        {
            weaponsDropped++;
            if (collectInventoryStats)
                TestLogger.LogInventory("Weapon dropped");
        }

        public void RecordDeath()
        {
            playerDeaths++;
            if (collectPlayerStats)
                TestLogger.LogPlayer("Player died");
        }

        public void RecordKill()
        {
            enemiesKilled++;
            if (collectPlayerStats)
                TestLogger.LogPlayer("Enemy killed");
        }

        private void SaveCurrentStats()
        {
            string statsSummary = $@"
=== GAMEPLAY STATS SUMMARY ===
Time: {System.DateTime.Now:HH:mm:ss}
Session Duration: {(Time.time / 60f):F2} minutes

WEAPON STATS:
- Shots Fired: {shotsFired}
- Shots Hit: {shotsHit}
- Accuracy: {(shotsFired > 0 ? (shotsHit * 100f / shotsFired) : 0):F2}%
- Reloads: {reloadsPerformed}

PLAYER STATS:
- Distance Moved: {totalDistanceMoved:F2} units
- Deaths: {playerDeaths}
- Kills: {enemiesKilled}

INVENTORY STATS:
- Weapons Picked Up: {weaponsPickedUp}
- Weapons Dropped: {weaponsDropped}

GRENADE STATS:
- Grenades Thrown: {grenadesThrown}
===============================
";
            TestLogger.Log(TestLogger.LogType.Gameplay, statsSummary);
        }

        private void OnApplicationQuit()
        {
            SaveCurrentStats();
            TestLogger.Log(TestLogger.LogType.Gameplay, "Final stats saved on quit");
        }

        private void OnDestroy()
        {
            SaveCurrentStats();
        }
    }
}
