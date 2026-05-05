using UnityEngine;

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
            /*
            if (col.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(damage, transform.position);
            }
            */
            Debug.Log($"{col} exploded!");
        }
    }
}