using UnityEngine;
using LightSide.Core;

public class SmokeGrenade : BaseGrenade
{
    public GameObject smokeEffectPrefab;

    protected override void OnExplode()
    {
        GameObject smoke = Instantiate(smokeEffectPrefab, transform.position, Quaternion.identity);
        Destroy(smoke, 10f);
        Debug.Log("[SMOKE] Deployed!");

        // Вызываем событие взрыва гранаты
        EventBus.InvokeGrenadeExploded(GrenadeType.Smoke, transform.position);
    }
}