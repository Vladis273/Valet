using UnityEngine;
using LightSide.Core;

public class IncendiaryGrenade : BaseGrenade
{
    public float damagePerSecond = 10f;
    public float burnDuration = 5f;
    public LayerMask damageLayers;

    protected override void OnExplode()
    {
        StartCoroutine(BurnArea());
        Debug.Log("[FIRE] Ignited area!");

        // Вызываем событие взрыва гранаты
        EventBus.InvokeGrenadeExploded(GrenadeType.Incendiary, transform.position);
    }

    System.Collections.IEnumerator BurnArea()
    {
        float elapsed = 0f;
        while (elapsed < burnDuration)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, damageLayers);
            foreach (Collider col in colliders)
            {
                if (col.TryGetComponent(out IDamageable target))
                {
                    Vector3 direction = (target as MonoBehaviour).transform.position - transform.position;
                    target.TakeDamage(damagePerSecond * Time.deltaTime, transform.position, direction.normalized);
                }
            }
            yield return null;
            elapsed += Time.deltaTime;
        }
    }
}