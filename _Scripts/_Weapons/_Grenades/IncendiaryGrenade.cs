using UnityEngine;

public class IncendiaryGrenade : BaseGrenade
{
    public float damagePerSecond = 10f;
    public float burnDuration = 5f;
    public LayerMask damageLayers;

    protected override void OnExplode()
    {
        StartCoroutine(BurnArea());
        Debug.Log("[FIRE] Ignited area!");
    }

    System.Collections.IEnumerator BurnArea()
    {
        float elapsed = 0f;
        while (elapsed < burnDuration)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, damageLayers);
            foreach (Collider col in colliders)
            {
                /*
                if (col.TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(damagePerSecond * Time.deltaTime, transform.position);
                }
                */
            }
            yield return null;
            elapsed += Time.deltaTime;
        }
    }
}