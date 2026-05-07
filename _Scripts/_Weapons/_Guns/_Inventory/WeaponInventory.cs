using UnityEngine;
using System.Collections.Generic;

public class WeaponInventory : MonoBehaviour
{
    public Transform gunContainer;
    public int maxWeapons = 2;
    public int activeSlotIndex = -1;

    [Header("Weapon Slots")]
    public List<WeaponSlot> weaponSlots = new List<WeaponSlot>();

    [System.Serializable]
    public class WeaponSlot
    {
        public GameObject weaponInstance;
        public bool isEmpty => weaponInstance == null;
    }

    private void Awake()
    {
        while (weaponSlots.Count < maxWeapons) weaponSlots.Add(new WeaponSlot());
    }

    public void PickUpWeapon(GameObject weaponPrefab, int ammoInMagazine, Vector3 dropPos, Quaternion dropRot)
    {
        int emptySlot = GetEmptySlotIndex();
        if (emptySlot == -1 && weaponSlots.Count >= maxWeapons)
        {
            emptySlot = activeSlotIndex;
            if (weaponSlots[emptySlot].weaponInstance != null)
            {
                var armsController = weaponSlots[activeSlotIndex].weaponInstance.GetComponent<ArmsController>();
                if (armsController != null && armsController.dropPrefab != null)
                {
                    var weaponComponentsTransform = armsController.GetComponentInChildren<WeaponComponentsTransform>();
                    if (weaponComponentsTransform != null && weaponComponentsTransform.controller != null)
                    {
                        // Сохраняем текущее состояние магазина перед дропом
                        SaveMagazineState(weaponComponentsTransform.controller);
                        
                        GameObject droppedGun = Instantiate(armsController.dropPrefab, dropPos, dropRot);
                        droppedGun.GetComponent<WeaponPickup>().ammoCount = weaponComponentsTransform.controller.currentAmmo;
                    }
                }
                Destroy(weaponSlots[emptySlot].weaponInstance);
            }
        }

        GameObject weaponObj = Instantiate(weaponPrefab, gunContainer);
        weaponObj.GetComponent<ArmsController>().playerData = gameObject.GetComponent<PlayerData>();
        weaponObj.GetComponentInChildren<WeaponController>().currentAmmo = ammoInMagazine;

        weaponSlots[emptySlot].weaponInstance = weaponObj;
        weaponObj.SetActive(false);

        SwitchToSlot(emptySlot);
    }

    /// <summary>
    /// Сохраняет состояние магазина оружия перед его удалением/дропом
    /// </summary>
    private void SaveMagazineState(WeaponController controller)
    {
        if (controller == null) return;
        
        // MagazineManager автоматически сохраняет состояние в пуле
        // При создании нового оружия того же типа оно получит тот же пул
        Debug.Log($"[WeaponInventory] Saved magazine state: {controller.currentAmmo}/{controller.MaxAmmo}");
    }

    public void SwitchToSlot(int index)
    {
        if (index >= weaponSlots.Count || 
            weaponSlots[index].isEmpty || 
            index == activeSlotIndex ||
            index < 0) return;

        if (activeSlotIndex != -1 && !weaponSlots[activeSlotIndex].isEmpty)
            weaponSlots[activeSlotIndex].weaponInstance.SetActive(false);

        activeSlotIndex = index;
        weaponSlots[activeSlotIndex].weaponInstance.SetActive(true);
        Debug.Log($"[WeaponInventory] Activated slot {index}");
    }

    int GetEmptySlotIndex()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i].isEmpty)
                return i;
        }
        return -1;
    }

    public bool CanPickUpWeapon() => GetEmptySlotIndex() != -1 || weaponSlots.Count < maxWeapons;
    
    /// <summary>
    /// Добавляет магазин в пул оружия (для подбора с тел врагов)
    /// </summary>
    public void AddMagazineToWeaponPool(string weaponPoolId, MagazineData magazineData, int ammoInMagazine)
    {
        if (MagazineManager.Instance == null) return;
        
        var pool = MagazineManager.Instance.GetMagazinePool(weaponPoolId);
        if (pool == null)
        {
            // Если пула нет, создаем новый
            MagazineManager.Instance.RegisterWeaponPool(weaponPoolId, magazineData, 1);
            pool = MagazineManager.Instance.GetMagazinePool(weaponPoolId);
        }
        
        // Добавляем новый магазин в пул
        var newMagazine = new MagazineManager.MagazineState(magazineData.capacity);
        newMagazine.currentAmmo = Mathf.Clamp(ammoInMagazine, 0, magazineData.capacity);
        pool.magazines.Add(newMagazine);
        
        Debug.Log($"[WeaponInventory] Added magazine to pool {weaponPoolId}: {newMagazine.currentAmmo}/{newMagazine.capacity}");
    }
}