using UnityEngine;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    public GameObject weaponPrefab;
    public int ammoCount;
    [HideInInspector] public WeaponInventory inventory;

    public string GetInteractionPrompt() => "Удерживайте [R] чтобы подобрать";

    public void OnInteract(PlayerInteractor interactor) { }

    public void OnHoldInteract(PlayerInteractor interactor)
    {
        WeaponInventory inventory = interactor.GetComponentInParent<WeaponInventory>();
        if (inventory == null && !inventory.CanPickUpWeapon()) return;

        inventory.PickUpWeapon(weaponPrefab, ammoCount, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}