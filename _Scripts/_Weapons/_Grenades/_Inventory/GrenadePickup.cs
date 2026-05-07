using UnityEngine;

public class GrenadePickup : MonoBehaviour, IInteractable
{
    [Header("Grenade Type")]
    public GrenadeType grenadeType = GrenadeType.Fragmentation;
    public int maxCount = 1;
    public GameObject activePrefab;

    public void OnInteract(PlayerInteractor interactor)
    {
        GrenadeInventory inventory = interactor.GetComponentInParent<GrenadeInventory>();
        if (!inventory.CanAddGrenade(grenadeType)) return;
        inventory.AddGrenade(grenadeType, 1, activePrefab, maxCount);
        Pickup();
    }

    public string GetInteractionPrompt() => $"Подобрать {grenadeType} гранату";
    public void Pickup() => Destroy(gameObject);
}