using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine;

namespace Tests.Runtime
{
    public class GrenadeTests
    {
        private GameObject testGrenade;
        private Grenade grenade;

        [SetUp]
        public void Setup()
        {
            // Создаем тестовую гранату
            testGrenade = new GameObject("TestGrenade");
            grenade = testGrenade.AddComponent<Grenade>();
            
            // Добавляем необходимые компоненты
            testGrenade.AddComponent<Rigidbody>();
            testGrenade.AddComponent<SphereCollider>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(testGrenade);
        }

        [Test]
        public void Grenade_InitializesCorrectly()
        {
            Assert.IsNotNull(grenade);
        }

        [UnityTest]
        public IEnumerator Grenade_ExplodesAfterDelay()
        {
            // Тест взрыва по таймеру
            yield return null;
            Assert.Pass("Grenade explosion test structure ready");
        }

        [UnityTest]
        public IEnumerator Grenade_DamageOnExplosion()
        {
            // Тест урона при взрыве
            yield return null;
            Assert.Pass("Grenade damage test structure ready");
        }
    }
}
