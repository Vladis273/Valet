using UnityEngine;

public class FlashbangGrenade : BaseGrenade
{
    public GameObject flashLight;
    public float flashTime = 0.25f;

    protected override void OnExplode()
    {
        GameObject flash = Instantiate(flashLight, transform.position, Quaternion.identity);
        Destroy(flash, flashTime);
        Debug.Log("[FLASHBANG] Blinded enemies!");
    }
}