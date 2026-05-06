# Система логирования тестов и геймплея

## Обзор

Эта система предоставляет комплексное решение для логирования событий тестов и игрового процесса в Unity-проекте. Она автоматически собирает данные о действиях игрока, результатах тестов, использовании оружия, гранат и других игровых механиках.

## Компоненты

### 1. GameLogger (Основной менеджер)
**Путь:** `Assets/Scripts/Utilities/Logging/GameLogger.cs`

Центральный компонент системы, который:
- Создается автоматически как Singleton
- Сохраняет события в CSV-файлы
- Поддерживает различные типы событий (тесты, оружие, гранаты, урон и т.д.)
- Автоматически сохраняет лог при остановке сцены
- Имеет защиту от переполнения файлов (создает новые при достижении лимита)

**Использование:**
```csharp
// Базовое логирование
GameLogger.Instance.Log(LogEventType.TestPass, "Test", "Test completed", "Details here");

// Специализированные методы (через расширения)
GameLogger.Instance.LogWeaponFire("AK-47", 30, hitPoint);
GameLogger.Instance.LogWeaponReload("AK-47", 5, 30, false);
GameLogger.Instance.LogInventoryChange("Pistol", "Picked up", 1);
GameLogger.Instance.LogDamage("Enemy", 25f, "Player");
```

### 2. WeaponLogger
**Путь:** `Assets/Scripts/Utilities/Logging/WeaponLogger.cs`

Автоматический логгер для оружия. Добавляется на префаб оружия или объект с WeaponController.

**Настройки:**
- `weaponController` - ссылка на контроллер оружия
- `weaponName` - имя для логов

**Методы:**
```csharp
weaponLogger.LogFire(hitPoint);
weaponLogger.LogReload(ammoBefore, ammoAfter, isTactical);
```

### 3. PlayerLogger
**Путь:** `Assets/Scripts/Utilities/Logging/PlayerLogger.cs`

Логгирует действия игрока: движение, прыжки, взаимодействие, урон.

**Настройки:**
- `playerMovement` - ссылка на контроллер движения
- `playerName` - имя игрока
- `logEveryMove` - логировать каждое движение (может создать много данных)
- `moveLogInterval` - интервал логирования движений

**Методы:**
```csharp
playerLogger.LogJump();
playerLogger.LogLand();
playerLogger.LogInteraction("Door", "Open");
playerLogger.LogDamageReceived(10f, "Enemy");
playerLogger.LogWeaponSwitch("Pistol", "Rifle");
playerLogger.LogGrenadeThrow("Fragmentation");
```

### 4. InventoryLogger
**Путь:** `Assets/Scripts/Utilities/Logging/InventoryLogger.cs`

Логгирует все операции с инвентарем.

**Методы:**
```csharp
inventoryLogger.LogWeaponPickup("AK-47", 0);
inventoryLogger.LogWeaponDrop("Pistol", 1);
inventoryLogger.LogSlotSwitch(0, 1, "Rifle");
inventoryLogger.LogGrenadeAdd("Fragmentation", 2);
inventoryLogger.LogGrenadeUse("Fragmentation");
inventoryLogger.LogAmmoChange("AK-47", 10, 30, "Reload");
```

### 5. GrenadeLogger
**Путь:** `Assets/Scripts/Utilities/Logging/GrenadeLogger.cs`

Логгирует бросок, полет и взрыв гранат.

**Настройки:**
- `grenadeType` - тип гранаты
- `throwerName` - кто бросил
- `logFlight` - логировать полет
- `flightLogInterval` - интервал логирования полета

**Методы:**
```csharp
grenadeLogger.LogThrow(position, velocity);
grenadeLogger.LogExplosion(position, affectedTargets, damage);
grenadeLogger.LogImpact(position, targetName);
grenadeLogger.LogBounce(position, velocity);
```

### 6. TestStatsCollector
**Путь:** `Assets/Scripts/Utilities/Logging/TestStatsCollector.cs`

Собирает агрегированную статистику по всей сессии и выводит её при остановке.

**Возможности:**
- Автоматический подсчет тестов (успешные/проваленные)
- Подсчет игровых действий (выстрелы, перезарядки, гранаты и т.д.)
- Вывод статистики в консоль
- Сохранение статистики в отдельный файл
- Расчет процента успешных тестов

## Настройка

### 1. Автоматическая инициализация
Система создает необходимые объекты автоматически при первом обращении к `GameLogger.Instance`. Ничего дополнительно настраивать не нужно.

### 2. Ручная настройка (опционально)
Создайте пустой GameObject в сцене и добавьте компонент `GameLogger`. Настройте параметры в инспекторе:
- `Enable In Editor` - логирование в редакторе
- `Enable In Game` - логирование в билде
- `Log Folder Path` - путь для сохранения логов
- `Max Log File Size MB` - максимальный размер файла
- `Auto Save On Stop` - автосохранение при остановке

### 3. Добавление логгеров на объекты
Для автоматического логирования конкретных действий добавьте соответствующие компоненты на объекты:
- `WeaponLogger` - на объекты с оружием
- `PlayerLogger` - на объект игрока
- `InventoryLogger` - на объект игрока или менеджера инвентаря
- `GrenadeLogger` - на префаб гранаты

## Интеграция с существующими скриптами

### Вариант 1: Прямой вызов методов
В существующие скрипты добавьте вызовы методов логгера в нужных местах:

```csharp
// В WeaponController.cs
public void Fire()
{
    // ... существующий код стрельбы ...
    
    // Логирование
    GameLogger.Instance.LogWeaponFire(weaponName, currentAmmo, hitPoint);
}
```

### Вариант 2: Использование событий
Добавьте события в основные классы и подпишите логгеры:

```csharp
// В WeaponController.cs
public event Action<Vector3> OnWeaponFire;

public void Fire()
{
    // ... код ...
    OnWeaponFire?.Invoke(hitPoint);
}

// В WeaponLogger.cs (раскомментировать и адаптировать)
controller.OnWeaponFire += HandleWeaponFire;
```

### Вариант 3: Интерфейсы
Создайте интерфейсы для логируемых действий:

```csharp
public interface ILoggableWeapon
{
    string GetWeaponName();
    int GetCurrentAmmo();
    event Action<Vector3> OnFire;
}
```

## Формат логов

Логи сохраняются в CSV-файлы со следующей структурой:

```csv
Timestamp,DateTime,Type,Category,Message,Details,GameTime,FrameCount
1234567890123,2024-01-15 14:30:45.123,TestPass,Test,"Test passed: ReloadTest","Details here",15.234,1234
1234567890456,2024-01-15 14:30:45.456,WeaponFire,Weapon,"Fired: AK-47","Ammo: 29, Hit: (10.5, 2.3, 5.1)",15.567,1240
```

Файлы сохраняются в:
- **Editor:** `<Project>/Logs/` или `Application.persistentDataPath/GameLogs/`
- **Build:** `Application.persistentDataPath/GameLogs/`

## Типы событий

| Тип | Описание |
|-----|----------|
| `TestStart` | Начало теста |
| `TestEnd` | Конец теста |
| `TestPass` | Тест успешен |
| `TestFail` | Тест провален |
| `GameplayAction` | Игровое действие |
| `WeaponFire` | Выстрел |
| `WeaponReload` | Перезарядка |
| `PlayerMove` | Движение игрока |
| `PlayerJump` | Прыжок |
| `InventoryChange` | Изменение инвентаря |
| `GrenadeThrow` | Бросок гранаты |
| `DamageDealt` | Нанесен урон |
| `DamageReceived` | Получен урон |
| `Error` | Ошибка |
| `Warning` | Предупреждение |
| `Custom` | Пользовательское событие |

## Пример использования в тестах

```csharp
using NUnit.Framework;
using GameUtilities.Logging;

[TestFixture]
public class WeaponTests
{
    private GameLogger _logger;
    
    [SetUp]
    public void SetUp()
    {
        _logger = GameLogger.Instance;
    }
    
    [Test]
    public void TestReload()
    {
        _logger.LogTestStart("ReloadTest", "Testing tactical reload");
        
        // ... код теста ...
        
        if (success)
        {
            _logger.LogTestPass("ReloadTest", "Ammo correctly restored");
            Assert.Pass();
        }
        else
        {
            _logger.LogTestFail("ReloadTest", "Ammo mismatch", "Expected 30, got 29");
            Assert.Fail();
        }
    }
}
```

## Статистика

При остановке сцены автоматически выводится статистика:

```
========================================
       TEST & GAMEPLAY STATISTICS      
========================================
Session Duration: 125.45s
----------------------------------------
Total Tests: 15
Passed: 14
Failed: 1
Pass Rate: 93.3%
----------------------------------------
Weapon Fires: 245
Reloads: 32
Grenade Throws: 8
Damage Events (Dealt): 67
Damage Events (Received): 12
Inventory Changes: 45
========================================
```

## Производительность

- Логирование асинхронное и не влияет на производительность игры
- CSV-формат легко читается и анализируется
- Автоматическое создание новых файлов предотвращает переполнение
- Можно отключить логирование в релизных сборках

## Рекомендации

1. **Не логируйте каждое движение** - используйте интервалы или только значимые события
2. **Очищайте старые логи** - файлы могут занимать много места
3. **Используйте разные категории** - это упростит фильтрацию
4. **Добавляйте контекст в Details** - это поможет при анализе проблем
5. **Отключайте логирование в релизе** - используйте `#if UNITY_EDITOR`

## Поиск и анализ логов

Для анализа CSV-файлов можно использовать:
- Excel / Google Sheets
- Python (pandas)
- Специализированные инструменты для логов
- Текстовые редакторы с поддержкой CSV

Пример Python-скрипта для анализа:
```python
import pandas as pd

df = pd.read_csv('GameLog_20240115_143045.csv')
print(df.groupby('Type').size())
print(df[df['Type'] == 'TestFail'])
```
