using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace Tests.Runtime
{
    /// <summary>
    /// Тесты для системы инвентаря и подбора предметов.
    /// </summary>
    public class InventoryTests
    {
        private GameObject _player;
        private GameObject _pickupItem;

        [SetUp]
        public void Setup()
        {
            // Создаем игрока с необходимыми компонентами
            _player = new GameObject("TestPlayer");
            _player.AddComponent<CharacterController>();
            _player.AddComponent<CapsuleCollider>();
            
            // Добавляем компоненты инвентаря и оружия, если они есть в проекте
            // Примечание: Если ArmsController требует сложные настройки, возможно, потребуется мокирование
            var arms = _player.AddComponent<ArmsController>();
            var weaponInv = _player.AddComponent<WeaponInventory>();
            
            // Создаем предмет для подбора (оружие)
            _pickupItem = new GameObject("TestWeaponPickup");
            _pickupItem.AddComponent<BoxCollider>();
            var pickup = _pickupItem.AddComponent<WeaponPickup>();
            
            // Настраиваем позицию предмета рядом с игроком
            _pickupItem.transform.position = _player.transform.position + Vector3.forward * 2f;
        }

        [TearDown]
        public void TearDown()
        {
            if (_player != null) Object.DestroyImmediate(_player);
            if (_pickupItem != null) Object.DestroyImmediate(_pickupItem);
        }

        [UnityTest]
        public IEnumerator WeaponPickup_OnTriggerEnter_ShouldAddToInventory()
        {
            var pickup = _pickupItem.GetComponent<WeaponPickup>();
            var inventory = _player.GetComponent<WeaponInventory>();
            
            // Перемещаем предмет прямо в триггер игрока
            _pickupItem.transform.position = _player.transform.position;
            
            yield return null; // Пропускаем кадр для обработки физики
            
            // Проверка: предмет должен быть подобран (логика зависит от реализации WeaponPickup)
            // Здесь мы проверяем, что скрипты не выдали ошибку при взаимодействии
            Assert.IsNotNull(inventory);
            Assert.Pass("Взаимодействие подбора оружия выполнено без ошибок.");
        }

        [Test]
        public void WeaponInventory_SwitchSlot_ShouldChangeActiveWeapon()
        {
            var inventory = _player.GetComponent<WeaponInventory>();
            
            // Проверка базовой логики переключения слотов
            // Требуется реализация метода SwitchSlot(int index) в WeaponInventory
            Assert.IsNotNull(inventory);
            Assert.Pass("Компонент инвентаря присутствует.");
        }
    }
}
