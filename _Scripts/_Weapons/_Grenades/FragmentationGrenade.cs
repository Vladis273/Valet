using UnityEngine;
using LightSide.Core;

public class FragmentationGrenade : BaseGrenade
{
    public float damage = 100f;
    public LayerMask damageLayers;

    protected override void OnExplode()
    {
        Debug.Log("[FRAG] Explosion!");

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, damageLayers);
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out IDamageable target))
            {
                Vector3 direction = (target as MonoBehaviour).transform.position - transform.position;
                target.TakeDamage(damage, transform.position, direction.normalized);
            }
            else
            {
                Debug.Log($"{col.name} hit by explosion!");
            }
        }

        // Вызываем событие взрыва гранаты
        EventBus.InvokeGrenadeExploded(GrenadeType.Fragmentation, transform.position);
    }
}