using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrenadePickupItem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Collider mainCollider;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private FloatingVisual floatingVisual;

    [Header("Explosión")]
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private int explosionDamage = 40;
    [SerializeField] private float explosionForce = 12f;
    [SerializeField] private LayerMask damageMask = ~0;
    [SerializeField] private bool explodeOnImpact = false;
    [SerializeField] private float armDelay = 0.15f;

    private Rigidbody rb;
    private Collider[] allColliders;
    private Renderer[] allRenderers;

    private bool hasBeenThrown = false;
    private bool hasExploded = false;
    private float armedTime = 0f;
    private Coroutine fuseRoutine;

    private Vector3 originalLocalScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        allColliders = GetComponentsInChildren<Collider>(true);
        allRenderers = GetComponentsInChildren<Renderer>(true);

        if (mainCollider == null)
            mainCollider = GetComponent<Collider>();

        originalLocalScale = transform.localScale;
    }

    private void Start()
    {
        bool startsAttached = transform.parent != null;

        if (floatingVisual != null)
        {
            if (startsAttached)
                floatingVisual.NotifyPickedUp();
            else
                floatingVisual.NotifyDropped();
        }
    }

    public void StoreTo(Transform storageSocket)
    {
        ResetGrenadeState();

        if (floatingVisual != null)
            floatingVisual.NotifyPickedUp();

        SetPhysicsHeldState();
        SetAllCollidersEnabled(false);

        if (storageSocket != null)
        {
            transform.SetParent(storageSocket, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            transform.SetParent(null, true);
        }

        transform.localScale = originalLocalScale;
        SetVisible(false);
    }

    public void EquipTo(Transform socket)
    {
        if (socket == null) return;

        ResetGrenadeState();

        if (floatingVisual != null)
            floatingVisual.NotifyPickedUp();

        SetVisible(true);
        SetPhysicsHeldState();
        SetAllCollidersEnabled(false);

        transform.SetParent(socket, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalLocalScale;
    }

    public void ThrowFrom(Transform hand, float forwardOffset, float throwForce, float throwUpForce, float spinForce)
    {
        if (hand == null) return;

        SetVisible(true);

        transform.SetParent(null, true);
        transform.position = hand.position + hand.forward * forwardOffset;
        transform.rotation = hand.rotation;

        SetAllCollidersEnabled(true);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.detectCollisions = true;
        rb.linearVelocity = (hand.forward * throwForce) + (hand.up * throwUpForce);
        rb.angularVelocity = Random.onUnitSphere * spinForce;

        IgnoreOwnerCollisions(hand.root);

        hasBeenThrown = true;
        armedTime = Time.time + armDelay;

        if (fuseRoutine != null)
            StopCoroutine(fuseRoutine);

        fuseRoutine = StartCoroutine(FuseRoutine());

        if (floatingVisual != null)
            floatingVisual.NotifyDropped();
    }

    private void ResetGrenadeState()
    {
        hasBeenThrown = false;
        hasExploded = false;

        if (fuseRoutine != null)
        {
            StopCoroutine(fuseRoutine);
            fuseRoutine = null;
        }
    }

    private void SetPhysicsHeldState()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.detectCollisions = false;
    }

    private void SetAllCollidersEnabled(bool enabled)
    {
        foreach (Collider c in allColliders)
        {
            if (c != null)
                c.enabled = enabled;
        }
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in allRenderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    private void IgnoreOwnerCollisions(Transform ownerRoot)
    {
        if (ownerRoot == null || mainCollider == null) return;

        Collider[] ownerColliders = ownerRoot.GetComponentsInChildren<Collider>(true);

        foreach (Collider c in ownerColliders)
        {
            if (c != null && c != mainCollider)
                Physics.IgnoreCollision(mainCollider, c, true);
        }
    }

    private IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown) return;
        if (!explodeOnImpact) return;
        if (hasExploded) return;
        if (Time.time < armedTime) return;

        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Vector3 center = transform.position;

        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, center, Quaternion.identity);
            Destroy(fx, 3f);
        }

        Collider[] hits = Physics.OverlapSphere(
            center,
            explosionRadius,
            damageMask,
            QueryTriggerInteraction.Collide
        );

        HashSet<Transform> alreadyDamaged = new HashSet<Transform>();
        HashSet<Rigidbody> pushedBodies = new HashSet<Rigidbody>();

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            Transform root = hit.transform.root;

            if (!alreadyDamaged.Contains(root))
            {
                MonoBehaviour[] behaviours = hit.GetComponentsInParent<MonoBehaviour>(true);

                foreach (MonoBehaviour mb in behaviours)
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
                    explosionForce,
                    center,
                    explosionRadius,
                    0.2f,
                    ForceMode.Impulse
                );

                pushedBodies.Add(hitRb);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}