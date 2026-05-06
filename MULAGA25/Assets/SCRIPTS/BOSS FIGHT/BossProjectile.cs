using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Arc")]
    public float arcHeight = 6f;

    [Header("Explosion")]
    public float explosionRadius = 1.8f;
    public int   damage          = 40;
    public GameObject explosionVFX;

    // ── Estado interno ────────────────────────────────────────────────────────
    private Vector3    startPos;
    private Vector3    targetPos;
    private float      travelTime  = 1.2f;
    private float      timer       = 0f;
    private bool       initialized = false;
    private bool       exploded    = false;
    private GameObject linkedWarning;   // El DangerZone que se destruye junto al impacto

    // ═══════════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Llamado por BossController.FireProjectileFromTo().
    /// El proyectil viaja en 'duration' segundos (igual que el warningDuration)
    /// para que llegue exactamente cuando desaparece el indicador.
    /// </summary>
    public void Initialize(Vector3 target, float duration, GameObject warning = null)
    {
        startPos      = transform.position;
        targetPos     = target;
        targetPos.y   = 0.05f;      // ras del suelo

        travelTime    = duration;
        timer         = 0f;
        initialized   = true;
        linkedWarning = warning;

        // Seguridad: auto-destruir si algo falla
        Destroy(gameObject, duration + 1f);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (!initialized || exploded) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / travelTime);

        // Parábola: interpolación horizontal + arco sinusoidal en Y
        Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
        pos.y += arcHeight * Mathf.Sin(Mathf.PI * t);
        transform.position = pos;

        // Orientar el proyectil hacia adelante (feedback visual en VR)
        if (t < 0.98f)
        {
            float   tNext   = Mathf.Clamp01(t + 0.02f);
            Vector3 posNext = Vector3.Lerp(startPos, targetPos, tNext);
            posNext.y += arcHeight * Mathf.Sin(Mathf.PI * tNext);
            Vector3 dir = posNext - pos;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        if (t >= 1f) Explode();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        // Daño al jugador si está dentro del radio
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                IDamageable dmg = hit.GetComponentInParent<IDamageable>();
                dmg?.TakeDamage(damage);
                break;
            }
        }

        // Destruir el DangerZone asociado si aún existe
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