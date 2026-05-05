using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    [Header("References")]
    public GrenadeInventory grenadeInventory;
    public Transform throwPoint;
    public PlayerInput playerInput;

    [Header("Throw Settings")]
    public float throwForce = 10f;
    public float throwUpward = 2f;

    void Update()
    {
        if (grenadeInventory == null || playerInput == null) return;

        if (playerInput.cycleGrenadePressed)
            grenadeInventory.CycleGrenadeType();
        
        if (playerInput.throwGrenadePressed)
            ThrowGrenade();
    }

    void ThrowGrenade()
    {
        GrenadeType type = grenadeInventory.GetCurrentType();
        if (!grenadeInventory.HasGrenade(type)) return;

        if (grenadeInventory.UseGrenade(type))
        {
            GameObject prefab = grenadeInventory.GetPrefab(type);

            GameObject grenadeObj = Instantiate(prefab, throwPoint.position, throwPoint.rotation);
            Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();
            if (rb == null) return;

            Vector3 direction = throwPoint.forward + Vector3.up * 0.3f;
            rb.AddForce(direction * throwForce + Vector3.up * throwUpward, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        }
        grenadeInventory.CleanEmptySlots();
    }
}