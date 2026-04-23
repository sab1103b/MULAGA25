using System.Collections.Generic;
using UnityEngine;

public class GrenadeItem : ThrowableItem
{
    [Header("Explosión")]
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private int explosionDamage = 40;
    [SerializeField] private float explosionForce = 12f;
    [SerializeField] private LayerMask damageMask = ~0;
    [SerializeField] private GameObject explosionEffectPrefab;

    protected override float GetFuseTime() => fuseTime;

    protected override void OnActivate()
    {
        Vector3 center = transform.position;

        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, center, Quaternion.identity);
            Destroy(fx, 3f);
        }

        Collider[] hits = Physics.OverlapSphere(
            center, explosionRadius, damageMask, QueryTriggerInteraction.Collide);

        HashSet<Transform> alreadyDamaged = new HashSet<Transform>();
        HashSet<Rigidbody> pushedBodies = new HashSet<Rigidbody>();

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;
            Transform root = hit.transform.root;

            if (!alreadyDamaged.Contains(root))
            {
                foreach (MonoBehaviour mb in hit.GetComponentsInParent<MonoBehaviour>(true))
                {
                    if (mb is IDamageable damageable)
                    {
                        damageable.TakeDamage(explosionDamage);
                        alreadyDamaged.Add(root);
                        break;
                    }
                }
            }

            Rigidbody hitRb = hit.attachedRigidbody;
            if (hitRb != null && hitRb != rb && !pushedBodies.Contains(hitRb))
            {
                hitRb.AddExplosionForce(
                    explosionForce, center, explosionRadius, 0.2f, ForceMode.Impulse);
                pushedBodies.Add(hitRb);
            }
        }

        Destroy(gameObject);
    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}