using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float arcHeight = 6f;
    public float travelTime = 1.2f;

    public float explosionRadius = 1.8f;
    public int damage = 40;
    public GameObject explosionVFX;

    private Vector3 startPos;
    private Vector3 targetPos;

    private float timer = 0f;
    private bool initialized = false;
    private bool exploded = false;

    public void Initialize(Vector3 lockedTarget)
    {
        startPos = transform.position;

        targetPos = lockedTarget;
        targetPos.y = 0.1f;

        timer = 0f;
        initialized = true;

        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (!initialized || exploded) return;

        timer += Time.deltaTime;
        float t = timer / travelTime;

        Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

        float height = arcHeight * Mathf.Sin(Mathf.PI * t);
        pos.y += height;

        transform.position = pos;

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
                dmg?.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}