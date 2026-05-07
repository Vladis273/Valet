using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Менеджер магазинов - управляет пулом магазинов для каждого типа оружия
/// Каждое оружие имеет свой уникальный магазин с собственным состоянием
/// </summary>
public class MagazineManager : MonoBehaviour
{
    [System.Serializable]
    public class WeaponMagazinePool
    {
        public string weaponId;
        public List<MagazineState> magazines = new List<MagazineState>();
        public int activeMagazineIndex = 0;
    }
    
    [System.Serializable]
    public class MagazineState
    {
        public int currentAmmo;
        public int capacity;
        public bool isEmpty => currentAmmo <= 0;
        public bool isFull => currentAmmo >= capacity;
        
        public MagazineState(int cap)
        {
            capacity = cap;
            currentAmmo = cap;
        }
        
        public void Reload()
        {
            currentAmmo = capacity;
        }
        
        public int ConsumeAmmo(int amount)
        {
            int toConsume = Mathf.Min(amount, currentAmmo);
            currentAmmo -= toConsume;
            return toConsume;
        }
    }
    
    public static MagazineManager Instance { get; private set; }
    
    [Header("Magazine Pools")]
    public List<WeaponMagazinePool> magazinePools = new List<WeaponMagazinePool>();
    
    private Dictionary<string, WeaponMagazinePool> _poolDictionary;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializePools();
    }
    
    /// <summary>
    /// Инициализирует словарь для быстрого доступа
    /// </summary>
    private void InitializePools()
    {
        _poolDictionary = new Dictionary<string, WeaponMagazinePool>();
        foreach (var pool in magazinePools)
        {
            if (!string.IsNullOrEmpty(pool.weaponId))
            {
                _poolDictionary[pool.weaponId] = pool;
            }
        }
    }
    
    /// <summary>
    /// Регистрирует новый пул магазинов для оружия
    /// </summary>
    public void RegisterWeaponPool(string weaponId, MagazineData magazineData, int initialMagazinesCount = 3)
    {
        if (_poolDictionary.ContainsKey(weaponId))
        {
            Debug.LogWarning($"[MagazineManager] Pool for weapon {weaponId} already exists");
            return;
        }
        
        var newPool = new WeaponMagazinePool
        {
            weaponId = weaponId,
            magazines = new List<MagazineState>()
        };
        
        // Создаем начальные магазины
        for (int i = 0; i < initialMagazinesCount; i++)
        {
            newPool.magazines.Add(new MagazineState(magazineData.capacity));
        }
        
        magazinePools.Add(newPool);
        _poolDictionary[weaponId] = newPool;
    }
    
    /// <summary>
    /// Получает текущий магазин для оружия
    /// </summary>
    public MagazineState GetCurrentMagazine(string weaponId)
    {
        if (!_poolDictionary.TryGetValue(weaponId, out var pool))
        {
            Debug.LogError($"[MagazineManager] No pool found for weapon {weaponId}");
            return null;
        }
        
        if (pool.magazines.Count == 0)
        {
            Debug.LogError($"[MagazineManager] No magazines in pool for weapon {weaponId}");
            return null;
        }
        
        return pool.magazines[pool.activeMagazineIndex];
    }
    
    /// <summary>
    /// Переключается на следующий магазин (при перезарядке со сменой магазина)
    /// </summary>
    public MagazineState SwitchToNextMagazine(string weaponId)
    {
        if (!_poolDictionary.TryGetValue(weaponId, out var pool))
        {
            Debug.LogError($"[MagazineManager] No pool found for weapon {weaponId}");
            return null;
        }
        
        // Находим первый не пустой магазин
        for (int i = 0; i < pool.magazines.Count; i++)
        {
            if (!pool.magazines[i].isEmpty)
            {
                pool.activeMagazineIndex = i;
                return pool.magazines[i];
            }
        }
        
        // Если все пустые, остаемся на текущем
        return pool.magazines[pool.activeMagazineIndex];
    }
    
    /// <summary>
    /// Добавляет патроны в резерв (подбор патронов)
    /// </summary>
    public void AddReserveAmmo(string weaponId, int amount)
    {
        // В данной реализации резерв не используется, но можно расширить
        Debug.Log($"[MagazineManager] Added {amount} reserve ammo for {weaponId}");
    }
    
    /// <summary>
    /// Проверяет, есть ли патроны в магазинах оружия
    /// </summary>
    public bool HasAmmo(string weaponId)
    {
        if (!_poolDictionary.TryGetValue(weaponId, out var pool))
        {
            return false;
        }
        
        foreach (var mag in pool.magazines)
        {
            if (!mag.isEmpty)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Перезаряжает текущий магазин (если есть время/возможность)
    /// Возвращает true если перезарядка началась
    /// </summary>
    public bool ReloadMagazine(string weaponId, MagazineData magazineData)
    {
        var currentMag = GetCurrentMagazine(weaponId);
        if (currentMag == null || currentMag.isFull)
        {
            return false;
        }
        
        // В упрощенной версии просто заполняем магазин
        // Можно добавить логику с потреблением резерва патронов
        currentMag.Reload();
        return true;
    }
    
    /// <summary>
    /// Сбрасывает все магазины для тестирования
    /// </summary>
    public void ResetAllMagazines()
    {
        foreach (var pool in magazinePools)
        {
            foreach (var mag in pool.magazines)
            {
                mag.Reload();
            }
            pool.activeMagazineIndex = 0;
        }
    }
}
