# ScriptableObject пресеты для оружия

## Как создать .asset файлы

### Способ 1: Автоматически через меню (Рекомендуется)

1. Откройте Unity Editor
2. В верхнем меню выберите: **Tools → Create Weapon Presets → Create All Magazines**
   - Будет создано 8 файлов магазинов в `_ScriptableObjects/Weapons/Magazines/`
3. Затем выберите: **Tools → Create Weapon Presets → Create All Weapons**
   - Будет создано 8 файлов конфигурации оружия в `_ScriptableObjects/Weapons/Configs/`

### Способ 2: Вручную через контекстное меню

#### Для MagazineData:
1. В Project окне нажмите ПКМ
2. Выберите: **Create → ScriptableObjects → Weapons → Magazine Data**
3. Назовите файл (например, `HK65_Magazine`)
4. Заполните поля в Inspector:
   - Magazine Name: "HK65 9x19mm Magazine"
   - Ammo Type: Pistol
   - Capacity: 15
   - Reload Time: 2.0
   - Tactical Reload Time: 1.5
   - Weight: 0.3

#### Для WeaponData:
1. В Project окне нажмите ПКМ
2. Выберите: **Create → ScriptableObjects → Weapons → Weapon Data**
3. Назовите файл (например, `HK65_Pistol`)
4. Заполните поля в Inspector
5. **Важно:** Перетащите соответствующий MagazineData в поле "Magazine Data"

---

## Список создаваемых файлов

### Магазины (MagazineData)
| Файл | Тип | Ёмкость | Время перезарядки |
|------|-----|---------|-------------------|
| `HK65_Magazine.asset` | Pistol | 15 | 2.0s / 1.5s |
| `UMP_Magazine.asset` | SMG | 25 | 2.2s / 1.6s |
| `AKM_Magazine.asset` | Rifle | 30 | 2.5s / 1.8s |
| `AUG_Magazine.asset` | Rifle | 30 | 2.3s / 1.7s |
| `CZ907_Magazine.asset` | Rifle | 30 | 2.4s / 1.75s |
| `M16_Magazine.asset` | Rifle | 30 | 2.3s / 1.7s |
| `PumpShotgun_Shells.asset` | Shotgun | 6 | 3.5s / 2.8s |
| `BenelliM4_Magazine.asset` | Shotgun | 7 | 3.0s / 2.5s |

### Оружие (WeaponData)
| Файл | Урон | Скорострельность | Режим | Магазин |
|------|------|------------------|-------|---------|
| `HK65_Pistol.asset` | 35 | 400 rpm | Single | HK65 |
| `UMP_SMG.asset` | 28 | 650 rpm | Auto | UMP |
| `AKM_Assault.asset` | 32 | 600 rpm | Auto | AKM |
| `AUG_Assault.asset` | 30 | 680 rpm | Auto | AUG |
| `CZ907_Assault.asset` | 31 | 620 rpm | Auto | CZ-907 |
| `M16_Assault.asset` | 30 | 700 rpm | Burst(3)/Single | M16 |
| `PumpShotgun.asset` | 25×8 | 60 rpm | Single | Pump |
| `BenelliM4.asset` | 24×8 | 120 rpm | Auto | Benelli M4 |

---

## Структура папок

```
Assets/
└── _ScriptableObjects/
    └── Weapons/
        ├── Magazines/          # Пресеты MagazineData
        │   ├── HK65_Magazine.asset
        │   ├── UMP_Magazine.asset
        │   ├── AKM_Magazine.asset
        │   ├── AUG_Magazine.asset
        │   ├── CZ907_Magazine.asset
        │   ├── M16_Magazine.asset
        │   ├── PumpShotgun_Shells.asset
        │   └── BenelliM4_Magazine.asset
        ├── Configs/            # Пресеты WeaponData
        │   ├── HK65_Pistol.asset
        │   ├── UMP_SMG.asset
        │   ├── AKM_Assault.asset
        │   ├── AUG_Assault.asset
        │   ├── CZ907_Assault.asset
        │   ├── M16_Assault.asset
        │   ├── PumpShotgun.asset
        │   └── BenelliM4.asset
        └── README.md           # Этот файл
```

---

## Настройка баланса

Все значения можно изменить в Inspector после создания файлов:

### MagazineData параметры:
- **Capacity**: Количество патронов в магазине
- **Reload Time**: Время полной перезарядки
- **Tactical Reload Time**: Время быстрой перезарядки (когда магазин не пуст)
- **Weight**: Вес магазина (влияет на скорость движения)

### WeaponData параметры:
- **Damage**: Базовый урон (для дробовиков - урон одной дробины)
- **Fire Rate**: Скорострельность (выстрелов в минуту)
- **Range**: Дальность эффективного огня
- **Recoil Impulse**: Сила отдачи (X, Y)
- **Max Reserve Ammo**: Максимальный боезапас
- **Fire Mode**: Режим огня (Single/Burst/Auto)
- **Burst Length**: Длина очереди (для burst режима)
- **Pellets Per Shot**: Количество дробин (для дробовиков)
- **Shotgun Spread**: Разброс дробин (для дробовиков)

---

## Интеграция с префабами

После создания .asset файлов:

1. Откройте префаб оружия
2. Найдите компонент `WeaponController` (или наследника)
3. В поле **Weapon Data** перетащите соответствующий `.asset` файл из `Configs/`
4. Сохраните префаб

Пример:
- Префаб `HK65_Prefab` → Weapon Data: `HK65_Pistol.asset`
- Префаб `AKM_Prefab` → Weapon Data: `AKM_Assault.asset`
