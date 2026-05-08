using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Arc")]
    public float arcHeight = 6f;

    [Header("Speed")]
    [Tooltip("2 = doble de rápido, 3 = triple de rápido")]
    public float speedMultiplier = 2f;

    [Header("Explosion")]
    public float explosionRadius = 2.5f;
    public int damage = 1;
    public GameObject explosionVFX;

    [Header("Debug")]
    public bool debugExplosion = true;

    private Vector3 startPos;
    private Vector3 targetPos;

    private float travelTime = 1.2f;
    private float timer = 0f;

    private bool initialized = false;
    private bool exploded = false;

    private GameObject linkedWarning;

    public void Initialize(Vector3 target, float duration, GameObject warning = null)
    {
        startPos = transform.position;

        targetPos = target;
        targetPos.y = 0.05f;

        // DOBLE DE VELOCIDAD:
        // Si antes llegaba en 1.2 segundos, ahora llega en 0.6 segundos.
        travelTime = Mathf.Max(0.05f, duration / speedMultiplier);

        timer = 0f;

        initialized = true;
        exploded = false;

        linkedWarning = warning;

        Destroy(gameObject, travelTime + 1f);
    }

    void Update()
    {
        if (!initialized || exploded) return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / travelTime);

        Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
        pos.y += arcHeight * Mathf.Sin(Mathf.PI * t);
        transform.position = pos;

        if (t < 0.98f)
        {
            float tNext = Mathf.Clamp01(t + 0.02f);

            Vector3 posNext = Vector3.Lerp(startPos, targetPos, tNext);
            posNext.y += arcHeight * Mathf.Sin(Mathf.PI * tNext);

            Vector3 dir = posNext - pos;

            if (dir.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        if (t >= 1f)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        Vector3 explosionPos = transform.position;

        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, explosionPos, Quaternion.identity);
        }

        Collider[] hits = Physics.OverlapSphere(
            explosionPos,
            explosionRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        if (debugExplosion)
        {
            Debug.Log(
                "[BossProjectile] Explosión en: " + explosionPos +
                " | Radio: " + explosionRadius +
                " | Objetos detectados: " + hits.Length
            );
        }

        bool playerDamaged = false;

        foreach (Collider hit in hits)
        {
            if (debugExplosion)
            {
                Debug.Log(
                    "[BossProjectile] Detectó: " + hit.name +
                    " | Tag: " + hit.tag +
                    " | Layer: " + LayerMask.LayerToName(hit.gameObject.layer) +
                    " | Root: " + hit.transform.root.name
                );
            }

            PlayerModel playerModel = FindPlayerModelFromHit(hit);

            if (playerModel != null)
            {
                if (!playerModel.isDead)
                {
                    playerModel.TakeDamage(damage);
                    playerDamaged = true;

                    Debug.Log("[BossProjectile] Daño aplicado al jugador: " + damage);
                }

                break;
            }
        }

        if (!playerDamaged && debugExplosion)
        {
            Debug.LogWarning(
                "[BossProjectile] No hizo daño. Puede que el radio no alcance al jugador, " +
                "o que el PlayerModel / PlayerController no esté en el objeto detectado."
            );
        }

        if (linkedWarning != null)
        {
            Destroy(linkedWarning);
        }

        Destroy(gameObject);
    }

    PlayerModel FindPlayerModelFromHit(Collider hit)
    {
        PlayerModel model = hit.GetComponent<PlayerModel>();

        if (model != null)
            return model;

        model = hit.GetComponentInParent<PlayerModel>();

        if (model != null)
            return model;

        PlayerController controller = hit.GetComponent<PlayerController>();

        if (controller != null && controller.model != null)
            return controller.model;

        controller = hit.GetComponentInParent<PlayerController>();

        if (controller != null && controller.model != null)
            return controller.model;

        model = hit.transform.root.GetComponentInChildren<PlayerModel>();

        if (model != null)
            return model;

        controller = hit.transform.root.GetComponentInChildren<PlayerController>();

        if (controller != null && controller.model != null)
            return controller.model;

        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}