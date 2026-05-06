# Автоматические тесты для Unity-проекта

Эта папка содержит набор автоматических тестов для быстрой проверки функционала игры без ручного запуска сцены.

## Структура

- **Tests/Editor** - Юнит-тесты (Edit Mode)
  - Тестируют чистую логику без зависимости от сцены и физики
  - Запускаются мгновенно
  - Файл: `WeaponLogicTests.cs`

- **Tests/Runtime** - Интеграционные тесты (Play Mode)
  - Тестируют поведение объектов в сцене, физику, ввод
  - Требуют запуска сцены (автоматически создается тестовая сцена)
  - Файлы:
    - `PlayerMovementTests.cs` - движение, прыжки игрока
    - `InventoryTests.cs` - подбор оружия, инвентарь
    - `GrenadeTests.cs` - бросок и взрыв гранат

## Как запускать тесты

### Через Unity Editor
1. Откройте окно **Test Runner**: `Window > General > Test Runner`
2. Выберите вкладку **EditMode** или **PlayMode**
3. Нажмите **Run All** или выберите конкретный тест

### Через командную строку (CI/CD)
```bash
# Запуск всех тестов
Unity -batchmode -quit -projectPath /path/to/project -runTests -testPlatform editmode
Unity -batchmode -quit -projectPath /path/to/project -runTests -testPlatform playmode

# Запуск с фильтрацией по имени
Unity -batchmode -quit -projectPath /path/to/project -runTests -testPlatform editmode -editorTestsFilter "WeaponLogicTests.*"
```

## Добавление новых тестов

### Шаблон юнит-теста (Editor)
```csharp
using NUnit.Framework;

namespace Tests.Editor
{
    public class MyNewTests
    {
        [Test]
        public void MyFeature_ShouldWorkCorrectly()
        {
            // Arrange - подготовка данных
            int expected = 5;
            
            // Act - выполнение действия
            int result = 2 + 3;
            
            // Assert - проверка результата
            Assert.AreEqual(expected, result);
        }
    }
}
```

### Шаблон интеграционного теста (Runtime)
```csharp
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;

namespace Tests.Runtime
{
    public class MyNewTests
    {
        [UnityTest]
        public IEnumerator MyFeature_ShouldWorkInScene()
        {
            // Подготовка сцены
            var obj = new GameObject("Test");
            
            yield return null; // Пропуск кадра
            
            // Проверка
            Assert.IsNotNull(obj);
            
            // Очистка (автоматическая через TearDown)
        }
    }
}
```

## Примечания

- Тесты используют фреймворк **NUnit** (встроен в Unity)
- Для Play Mode тестов используется **UnityEngine.TestTools**
- Моки и заглушки создаются внутри тестов для изоляции зависимостей
- Если ваши скрипты имеют сильные зависимости от сцены, возможно, потребуется рефакторинг для улучшения тестируемости
