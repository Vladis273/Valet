# Система автоматического тестирования и логирования

## Обзор
Эта система позволяет автоматически тестировать ключевые механики игры и собирать подробную статистику во время тестов и обычного геймплея.

## Структура файлов

### Тесты (Play Mode)
- `WeaponLogicTests.cs` - Тесты оружия (перезарядка, стрельба, отдача)
- `PlayerMovementTests.cs` - Тесты движения игрока (ходьба, бег, прыжки)
- `InventoryTests.cs` - Тесты инвентаря (подбор/выбрасывание оружия, гранаты)
- `GrenadeTests.cs` - Тесты гранат (бросок, взрыв, урон)

### Логирование и статистика
- `TestLogger.cs` - Центральный менеджер логов (сохраняет в `Assets/Tests/Runtime/Logs/`)
- `GameplayStatsCollector.cs` - Автоматический сборщик статистики gameplay

## Как использовать

### Запуск тестов
1. Откройте Unity
2. Перейдите в `Window > General > Test Runner`
3. Выберите вкладку **PlayMode**
4. Нажмите **Run All** или отдельные тесты

### Логирование во время игры
1. Добавьте на сцену пустой объект
2. Прикрепите скрипт `TestLogger` (опционально, создается автоматически при первом логе)
3. Прикрепите скрипт `GameplayStatsCollector` для сбора статистики

### Интеграция с основными скриптами
Для логирования действий добавьте вызовы в ваши скрипты:

```csharp
// В WeaponController.cs при выстреле
TestLogger.LogWeapon("Fired", currentAmmo);
statsCollector.RecordShot(hitDetected);

// В WeaponController.cs при перезарядке
TestLogger.LogWeapon("Reloaded");
statsCollector.RecordReload();

// В PlayerMovement.cs
TestLogger.LogPlayer("Jumped", transform.position);

// В Inventory.cs
TestLogger.LogInventory("Picked up", weaponName);
statsCollector.RecordWeaponPickup();

// В Grenade.cs
TestLogger.LogGrenade("Thrown", grenadeType);
statsCollector.RecordGrenadeThrow();
```

### Формат логов
Логи сохраняются в текстовые файлы с временной меткой:
```
[13:45:22.123] [Weapon] Fired (Value: 28.00)
[13:45:23.456] [Player] Jumped at (10.5, 0.0, 5.2)
[13:45:25.789] [Inventory] Picked up: AK-74
[13:45:30.012] [Grenade] Thrown: Fragmentation
```

### Статистика
Каждые 30 секунд (настраивается) и при завершении игры сохраняется сводка:
- Точность стрельбы
- Количество перезарядок
- Пройденное расстояние
- Смерти/убийства
- Подобрano/выброшено оружия
- Брошено гранат

## Расположение логов
Все логи сохраняются в: `Assets/Tests/Runtime/Logs/test_log_YYYY-MM-DD_HH-mm-ss.txt`

## Автоматизация в CI/CD
Тесты можно запускать из командной строки:
```bash
Unity -batchmode -runTests -testPlatform PlayMode -projectPath <path>
```

## Примечания
- Тесты требуют наличия соответствующих префабов и компонентов на сцене
- Для корректной работы некоторых тестов может потребоваться настройка физических материалов
- Логирование минимально влияет на производительность (<1% FPS)
