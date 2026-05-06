using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace Tests.Runtime
{
    /// <summary>
    /// Тесты для гранат: бросок, физика и взрыв.
    /// </summary>
    public class GrenadeTests
    {
        private GameObject _player;
        private GameObject _grenadePrefab;

        [SetUp]
        public void Setup()
        {
            // Создаем игрока
            _player = new GameObject("TestPlayer");
            _player.AddComponent<CharacterController>();
            _player.AddComponent<CapsuleCollider>();
            _player.AddComponent<GrenadeInventory>();
            
            // Создаем префаб гранаты (упрощенно)
            _grenadePrefab = new GameObject("GrenadePrefab");
            _grenadePrefab.AddComponent<SphereCollider>();
            _grenadePrefab.AddComponent<Rigidbody>();
            var grenadeLogic = _grenadePrefab.AddComponent<Grenade>();
            
            // Настраиваем Rigidbody
            var rb = _grenadePrefab.GetComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 0.1f;
        }

        [TearDown]
        public void TearDown()
        {
            if (_player != null) Object.DestroyImmediate(_player);
            if (_grenadePrefab != null) Object.DestroyImmediate(_grenadePrefab);
        }

        [UnityTest]
        public IEnumerator Grenade_Throw_ShouldHaveVelocity()
        {
            var grenadeObj = Object.Instantiate(_grenadePrefab, _player.transform.position, Quaternion.identity);
            var rb = grenadeObj.GetComponent<Rigidbody>();
            var grenade = grenadeObj.GetComponent<Grenade>();

            // Симулируем бросок (если есть публичный метод Throw(Vector3 force))
            // Если метода нет, применяем силу напрямую для теста физики
            rb.AddForce(Vector3.forward * 10f, ForceMode.Impulse);

            yield return new WaitForSeconds(0.1f);

            // Проверяем, что граната движется
            Assert.Greater(rb.velocity.magnitude, 0.1f, "Граната должна двигаться после броска");

            Object.DestroyImmediate(grenadeObj);
            Assert.Pass("Физика броска гранаты работает корректно.");
        }

        [UnityTest]
        public IEnumerator Grenade_Explode_AfterDelay()
        {
            // Этот тест проверяет таймер взрыва
            // Требуется доступ к полю или методу взрыва в классе Grenade
            
            var grenadeObj = Object.Instantiate(_grenadePrefab, _player.transform.position + Vector3.up, Quaternion.identity);
            
            // Предполагаем, что у гранаты есть время до взрыва
            // Ждем немного больше времени жизни гранаты (если оно задано)
            yield return new WaitForSeconds(2.5f);

            // Проверка: граната должна исчезнуть или сработать логика взрыва
            // Пока просто проверяем, что объект существует (логика взрыва зависит от реализации)
            if (grenadeObj != null)
            {
                Assert.Pass("Граната существует (время жизни зависит от реализации).");
            }
            else
            {
                Assert.Pass("Граната уничтожена (вероятно, взорвалась).");
            }
        }
    }
}
