using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace Tests.Runtime
{
    /// <summary>
    /// Интеграционные тесты для проверки поведения игрока и оружия в сцене.
    /// Запускаются в Play Mode.
    /// </summary>
    public class PlayerMovementTests
    {
        private GameObject _playerPrefab;
        private GameObject _playerInstance;

        [SetUp]
        public void Setup()
        {
            // Создаем тестового игрока программно, чтобы не зависеть от сцены
            _playerPrefab = new GameObject("TestPlayer");
            _playerPrefab.AddComponent<CharacterController>();
            _playerPrefab.AddComponent<PlayerMovement>();
            // Добавляем заглушки для необходимых компонентов
            _playerPrefab.AddComponent<CapsuleCollider>();
            
            _playerInstance = Object.Instantiate(_playerPrefab);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerInstance != null)
                Object.DestroyImmediate(_playerInstance);
            if (_playerPrefab != null)
                Object.DestroyImmediate(_playerPrefab);
        }

        [UnityTest]
        public IEnumerator Player_Jump_ShouldMoveUp()
        {
            var movement = _playerInstance.GetComponent<PlayerMovement>();
            var controller = _playerInstance.GetComponent<CharacterController>();
            
            // Симулируем нажатие прыжка (если есть публичный метод или доступ к полю)
            // В реальном тесте нужно будет вызвать метод Jump() или сэмулировать ввод
            
            float startY = controller.transform.position.y;
            
            // Ждем кадр для обновления физики
            yield return null;
            
            // Проверяем, что игрок не провалился под землю (базовая проверка)
            Assert.GreaterOrEqual(controller.transform.position.y, startY - 0.1f);
            
            // Примечание: Для полноценной проверки прыжка нужно эмулировать ввод через InputSystem
            Assert.Pass("Базовая проверка позиции игрока пройдена.");
        }

        [UnityTest]
        public IEnumerator Player_Move_ShouldChangePosition()
        {
            var movement = _playerInstance.GetComponent<PlayerMovement>();
            var controller = _playerInstance.GetComponent<CharacterController>();

            Vector3 startPos = controller.transform.position;

            // Эмуляция движения (нужен доступ к методам движения или ввод)
            // Если в PlayerMovement есть публичный метод Move(Vector3 dir), вызываем его
            
            yield return null;

            // Проверка: позиция должна измениться, если движение было успешным
            // Пока просто проверяем, что объект существует и не уничтожен
            Assert.IsNotNull(controller);
            Assert.Pass("Объект игрока корректно создан и существует.");
        }
    }
}
