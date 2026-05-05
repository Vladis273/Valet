using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GrenadeInventory : MonoBehaviour
{
    private GrenadeType currentType;
    public int MaxSlotsForGrenades;

    [Header("Grenade Slots")]
    public List<GrenadeSlot> slots = new List<GrenadeSlot>();

    [System.Serializable]
    public class GrenadeSlot
    {
        public GrenadeType type;
        public int count;
        public int maxCount;
        public GameObject prefab;
    }

    void Start()
    {
        UpdateCurrentType();
    }

    public void AddGrenade(GrenadeType type, int amount = 1, GameObject activePrefab = null, int limit = 0)
    {
        var existingSlot = slots.Find(s => s.type == type);
        if (existingSlot != null)
        {
            existingSlot.count = Mathf.Min(existingSlot.count + amount, existingSlot.maxCount);
        }
        else if (CanAddGrenadeSlot(type))
        {
            slots.Add(new GrenadeSlot
            {
                type = type,
                count = 1,
                prefab = activePrefab,
                maxCount = limit
            });
            Debug.Log($"Added new grenade type: {type}");
        }

        UpdateCurrentType();
    }

    public bool UseGrenade(GrenadeType type)
    {
        var slot = slots.Find(s => s.type == type);
        if (slot != null && slot.count > 0)
        {
            slot.count--;
            UpdateCurrentType();
            return true;
        }
        return false;
    }

    public void CycleGrenadeType()
    {
        var available = slots.Where(s => s.count > 0).Select(s => s.type).ToList();
        if (available.Count == 0) return;

        int currentIndex = available.IndexOf(currentType);
        int nextIndex = (currentIndex + 1) % available.Count;
        currentType = available[nextIndex];

        Debug.Log($"[GrenadeInventory] Switched to: {currentType} ({GetCount(currentType)} left)");
    }

    public bool CanAddGrenadeSlot(GrenadeType type)
    {
        if (slots.Exists(s => s.type == type))
            return true;

        return slots.Count < MaxSlotsForGrenades;
    }

    void UpdateCurrentType()
    {
        if (!HasGrenade(currentType))
        {
            var first = slots.FirstOrDefault(s => s.count > 0);
            currentType = first != null ? first.type : GrenadeType.None;
        }
    }

    public bool CanAddGrenade(GrenadeType type)
    {
        var existingSlot = slots.Find(s => s.type == type);

        if (existingSlot != null) 
            return GetMaxCount(type) > GetCount(type);
        else 
            return slots.Count < MaxSlotsForGrenades;
    }

    public bool HasGrenade(GrenadeType type)
    {
        return slots.Exists(s => s.type == type && s.count > 0);
    }

    public GameObject GetPrefab(GrenadeType type)
    {
        return slots.Find(s => s.type == type)?.prefab;
    }

    public void CleanEmptySlots()
    {
        slots.RemoveAll(s => s.count <= 0);
    }

    public GrenadeType GetCurrentType() => currentType;
    public int GetCount(GrenadeType type) => slots.Find(s => s.type == type)?.count ?? 0;
    public int GetMaxCount(GrenadeType type) => slots.Find(s => s.type == type)?.maxCount ?? 0;
}