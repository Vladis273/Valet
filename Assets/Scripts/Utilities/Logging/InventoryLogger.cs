using UnityEngine;
using GameUtilities.Logging;

namespace GameUtilities.Logging
{
    /// <summary>
    /// Автоматический логгер для инвентаря
    /// Логирует подбор, выбрасывание и смену предметов
    /// </summary>
    public class InventoryLogger : MonoBehaviour
    {
        [Header("Настройки")]
        [Tooltip("Ссылка на WeaponInventory (если не назначена, попробует найти автоматически)")]
        [SerializeField] private MonoBehaviour weaponInventory;
        
        [Tooltip("Имя владельца для логов")]
        [SerializeField] private string ownerName = "Player";

        private GameLogger _logger;
        private bool _isInitialized;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            _logger = GameLogger.Instance;
            
            if (weaponInventory == null)
            {
                weaponInventory = GetComponent<MonoBehaviour>();
            }

            _isInitialized = true;
            
            _logger.Log(LogEventType.TestStart, "Inventory", $"Inventory logger initialized: {ownerName}");
        }

        /// <summary>
        /// Вызвать при подборе оружия
        /// </summary>
        public void LogWeaponPickup(string weaponName, int slotIndex)
        {
            if (!_isInitialized) Initialize();
            _logger.LogInventoryChange(weaponName, "Picked up", slotIndex);
            _logger.LogGameplayAction($"Picked up weapon: {weaponName}", 
                $"Slot: {slotIndex}, Owner: {ownerName}");
        }

        /// <summary>
        /// Вызвать при выбрасывании оружия
        /// </summary>
        public void LogWeaponDrop(string weaponName, int slotIndex)
        {
            if (!_isInitialized) Initialize();
            _logger.LogInventoryChange(weaponName, "Dropped", slotIndex);
            _logger.LogGameplayAction($"Dropped weapon: {weaponName}", 
                $"Slot: {slotIndex}, Owner: {ownerName}, Position: {transform.position}");
        }

        /// <summary>
        /// Вызвать при смене слота
        /// </summary>
        public void LogSlotSwitch(int fromSlot, int toSlot, string weaponName)
        {
            if (!_isInitialized) Initialize();
            _logger.LogInventoryChange(weaponName, "Switched slot", toSlot);
            _logger.LogGameplayAction($"Switched weapon slot: {fromSlot} -> {toSlot}", 
                $"Weapon: {weaponName}, Owner: {ownerName}");
        }

        /// <summary>
        /// Вызвать при добавлении гранаты
        /// </summary>
        public void LogGrenadeAdd(string grenadeType, int count)
        {
            if (!_isInitialized) Initialize();
            _logger.LogInventoryChange($"{grenadeType} x{count}", "Added grenade");
            _logger.LogGameplayAction($"Added grenade: {grenadeType} x{count}", 
                $"Owner: {ownerName}");
        }

        /// <summary>
        /// Вызвать при использовании гранаты
        /// </summary>
        public void LogGrenadeUse(string grenadeType)
        {
            if (!_isInitialized) Initialize();
            _logger.LogInventoryChange(grenadeType, "Used grenade");
            _logger.LogGameplayAction($"Used grenade: {grenadeType}", 
                $"Owner: {ownerName}, Position: {transform.position}");
        }

        /// <summary>
        /// Вызвать при изменении патронов
        /// </summary>
        public void LogAmmoChange(string weaponName, int oldAmmo, int newAmmo, string reason = "")
        {
            if (!_isInitialized) Initialize();
            string details = $"Before: {oldAmmo}, After: {newAmmo}";
            if (!string.IsNullOrEmpty(reason))
            {
                details += $", Reason: {reason}";
            }
            _logger.Log(LogEventType.Custom, "Inventory", 
                $"Ammo changed for {weaponName}", details);
        }

        /// <summary>
        /// Вызвать при полной очистке инвентаря
        /// </summary>
        public void LogInventoryClear()
        {
            if (!_isInitialized) Initialize();
            _logger.Log(LogEventType.InventoryChange, "Inventory", 
                $"Inventory cleared", $"Owner: {ownerName}");
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ownerName))
            {
                ownerName = gameObject.name;
            }
        }

        private void OnDestroy()
        {
            _logger?.Log(LogEventType.TestEnd, "Inventory", $"Inventory logger destroyed: {ownerName}");
        }
    }
}
