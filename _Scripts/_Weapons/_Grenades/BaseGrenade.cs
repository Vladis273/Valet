using UnityEngine;

public enum GrenadeType
{
    None,
    Fragmentation,
    Smoke,
    Flashbang,
    Incendiary
}

public abstract class BaseGrenade : MonoBehaviour
{
    public float fuseTime = 3f;
    public float explosionRadius = 5f;

    protected bool hasExploded = false;

    void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    protected virtual void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        OnExplode();

        Destroy(gameObject);
    }

    protected abstract void OnExplode();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}