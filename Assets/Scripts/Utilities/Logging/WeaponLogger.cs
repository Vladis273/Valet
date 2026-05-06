using UnityEngine;
using GameUtilities.Logging;

namespace GameUtilities.Logging
{
    /// <summary>
    /// Автоматический логгер для WeaponController
    /// Подключается к оружию и логирует все действия
    /// </summary>
    public class WeaponLogger : MonoBehaviour
    {
        [Header("Настройки")]
        [Tooltip("Ссылка на WeaponController (если не назначена, попробует найти автоматически)")]
        [SerializeField] private MonoBehaviour weaponController;
        
        [Tooltip("Имя оружия для логов")]
        [SerializeField] private string weaponName = "Unknown Weapon";

        private GameLogger _logger;
        private int _lastAmmoCount = -1;
        private bool _isInitialized;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            _logger = GameLogger.Instance;
            
            if (weaponController == null)
            {
                weaponController = GetComponent<MonoBehaviour>();
            }

            if (string.IsNullOrEmpty(weaponName))
            {
                weaponName = gameObject.name;
            }

            _isInitialized = true;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            // Примечание: Здесь предполагается, что в WeaponController есть события
            // Если их нет, можно добавить их или использовать рефлексию/интерфейсы
            
            // Пример подписки (раскомментировать и адаптировать под реальные события):
            /*
            var controller = weaponController as WeaponController;
            if (controller != null)
            {
                controller.OnWeaponFire += HandleWeaponFire;
                controller.OnWeaponReload += HandleWeaponReload;
                controller.OnAmmoChanged += HandleAmmoChanged;
            }
            */
            
            // Альтернатива: периодическая проверка состояния
            InvokeRepeating(nameof(CheckState), 0f, 0.5f);
        }

        private void CheckState()
        {
            // Здесь можно проверять состояние оружия через рефлексию или публичные свойства
            // Это временное решение до добавления событий в WeaponController
        }

        #region Event Handlers
        
        public void HandleWeaponFire(Vector3 hitPoint)
        {
            int currentAmmo = GetCurrentAmmo();
            _logger.LogWeaponFire(weaponName, currentAmmo, hitPoint);
        }

        public void HandleWeaponReload(int ammoBefore, int ammoAfter, bool tactical)
        {
            _logger.LogWeaponReload(weaponName, ammoBefore, ammoAfter, tactical);
        }

        public void HandleAmmoChanged(int newAmmo)
        {
            if (_lastAmmoCount != -1 && newAmmo != _lastAmmoCount)
            {
                _logger.Log(LogEventType.Custom, "Weapon", 
                    $"Ammo changed: {_lastAmmoCount} -> {newAmmo}", 
                    $"Weapon: {weaponName}");
            }
            _lastAmmoCount = newAmmo;
        }

        #endregion

        private int GetCurrentAmmo()
        {
            // Заглушка - нужно адаптировать под реальный WeaponController
            // Можно использовать рефлексию или интерфейс
            return _lastAmmoCount >= 0 ? _lastAmmoCount : 0;
        }

        /// <summary>
        /// Публичный метод для вызова из других скриптов
        /// </summary>
        public void LogFire(Vector3 hitPoint = default)
        {
            if (!_isInitialized) Initialize();
            HandleWeaponFire(hitPoint);
        }

        /// <summary>
        /// Публичный метод для вызова из других скриптов
        /// </summary>
        public void LogReload(int ammoBefore, int ammoAfter, bool tactical)
        {
            if (!_isInitialized) Initialize();
            HandleWeaponReload(ammoBefore, ammoAfter, tactical);
        }

        private void OnDestroy()
        {
            CancelInvoke(nameof(CheckState));
            
            // Отписка от событий
            /*
            var controller = weaponController as WeaponController;
            if (controller != null)
            {
                controller.OnWeaponFire -= HandleWeaponFire;
                controller.OnWeaponReload -= HandleWeaponReload;
                controller.OnAmmoChanged -= HandleAmmoChanged;
            }
            */
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(weaponName))
            {
                weaponName = gameObject.name;
            }
        }
    }
}
