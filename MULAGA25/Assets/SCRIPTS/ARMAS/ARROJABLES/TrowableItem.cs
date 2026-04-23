using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public abstract class ThrowableItem : MonoBehaviour
{
    [Header("Referencias Base")]
    [SerializeField] protected Collider mainCollider;
    [SerializeField] protected FloatingVisual floatingVisual;

    [Header("Lanzamiento")]
    [SerializeField] protected float armDelay = 0.15f;
    [SerializeField] protected bool explodeOnImpact = false;

    protected Rigidbody rb;
    protected Collider[] allColliders;
    protected Renderer[] allRenderers;

    protected bool hasBeenThrown = false;
    protected bool hasTriggered = false;
    protected float armedTime = 0f;
    protected Coroutine fuseRoutine;
    protected Vector3 originalLocalScale;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        allColliders = GetComponentsInChildren<Collider>(true);
        allRenderers = GetComponentsInChildren<Renderer>(true);

        if (mainCollider == null)
            mainCollider = GetComponent<Collider>();

        originalLocalScale = transform.localScale;
    }

    protected virtual void Start()
    {
        bool startsAttached = transform.parent != null;
        if (floatingVisual != null)
        {
            if (startsAttached) floatingVisual.NotifyPickedUp();
            else floatingVisual.NotifyDropped();
        }
    }

    // ─── API Pública ───────────────────────────────────────────

    public void StoreTo(Transform storageSocket)
    {
        ResetState();
        if (floatingVisual != null) floatingVisual.NotifyPickedUp();

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

        ResetState();
        if (floatingVisual != null) floatingVisual.NotifyPickedUp();

        SetVisible(true);
        SetPhysicsHeldState();
        SetAllCollidersEnabled(false);

        transform.SetParent(socket, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalLocalScale;
    }

    public void ThrowFrom(Transform hand, float forwardOffset,
                          float throwForce, float throwUpForce, float spinForce)
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

        if (fuseRoutine != null) StopCoroutine(fuseRoutine);
        fuseRoutine = StartCoroutine(FuseRoutine());

        if (floatingVisual != null) floatingVisual.NotifyDropped();
    }

    // ─── Métodos que las subclases implementan ─────────────────

    // Duración antes de activarse (granada=fuseTime, escudo=0 para onImpact)
    protected abstract float GetFuseTime();

    // Qué hacer al activarse
    protected abstract void OnActivate();

    // ─── Internos ──────────────────────────────────────────────

    protected virtual void ResetState()
    {
        hasBeenThrown = false;
        hasTriggered = false;
        if (fuseRoutine != null)
        {
            StopCoroutine(fuseRoutine);
            fuseRoutine = null;
        }
    }

    protected virtual IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(GetFuseTime());
        TriggerActivation();
    }

    protected void TriggerActivation()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        OnActivate();
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown) return;
        if (!explodeOnImpact) return;
        if (hasTriggered) return;
        if (Time.time < armedTime) return;
        TriggerActivation();
    }

    protected void SetPhysicsHeldState()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.detectCollisions = false;
    }

    protected void SetAllCollidersEnabled(bool enabled)
    {
        foreach (Collider c in allColliders)
            if (c != null) c.enabled = enabled;
    }

    protected void SetVisible(bool visible)
    {
        foreach (Renderer r in allRenderers)
            if (r != null) r.enabled = visible;
    }

    protected void IgnoreOwnerCollisions(Transform ownerRoot)
    {
        if (ownerRoot == null || mainCollider == null) return;
        Collider[] ownerColliders = ownerRoot.GetComponentsInChildren<Collider>(true);
        foreach (Collider c in ownerColliders)
            if (c != null && c != mainCollider)
                Physics.IgnoreCollision(mainCollider, c, true);
    }

    protected virtual void OnDrawGizmosSelected() { }
}