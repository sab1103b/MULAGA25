using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Arc Settings")]
    public float arcHeight = 6f;
    public float travelTime = 1.2f;

    [Header("Explosion")]
    public float explosionRadius = 1.8f;
    public int damage = 40;
    public GameObject explosionVFX;

    private Vector3 startPos;
    private Vector3 targetPos;

    private float timer = 0f;
    private bool initialized = false;
    private bool exploded = false;

    private GameObject linkedWarning;

    // Método normal
    public void Initialize(Vector3 lockedTarget)
    {
        startPos = transform.position;

        targetPos = lockedTarget;
        targetPos.y = 0.05f;

        timer = 0f;
        initialized = true;

        Destroy(gameObject, travelTime + 1f);
    }

    // Método sincronizado con alerta
    public void Initialize(Vector3 lockedTarget, float customTravelTime, GameObject warning)
    {
        startPos = transform.position;

        targetPos = lockedTarget;
        targetPos.y = 0.05f;

        travelTime = customTravelTime;
        linkedWarning = warning;

        timer = 0f;
        initialized = true;

        Destroy(gameObject, travelTime + 1f);
    }

    void Update()
    {
        if (!initialized || exploded) return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / travelTime);

        // Movimiento horizontal hacia el target
        Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

        // Arco vertical
        pos.y += arcHeight * Mathf.Sin(Mathf.PI * t);

        transform.position = pos;

        // Rotar hacia la dirección del movimiento
        if (t < 0.99f)
        {
            float nextT = Mathf.Clamp01(t + 0.02f);

            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, nextT);
            nextPos.y += arcHeight * Mathf.Sin(Mathf.PI * nextT);

            Vector3 dir = nextPos - pos;

            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        if (t >= 1f)
            Explode();
    }

    void Explode()
    {
        if (exploded) return;

        exploded = true;

        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                IDamageable dmg = hit.GetComponentInParent<IDamageable>();

                if (dmg != null)
                    dmg.TakeDamage(damage);
            }
        }

        if (linkedWarning != null)
            Destroy(linkedWarning);

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}