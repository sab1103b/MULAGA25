using UnityEngine;

/// <summary>
/// Proyectil del enemigo de distancia.
/// Al impactar contra el jugador llama a model.TakeDamage(1),
/// que es exactamente el mismo sistema que usa PlayerController.
/// Un impacto = una vida completa de daño.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Lifetime")]
    public float maxLifetime = 5f;

    [Header("Impact FX")]
    public ParticleSystem impactFX;
    public AudioClip impactSFX;

    // ── Estado interno ────────────────────────────────────────────────────────
    private Rigidbody rb;
    private float lifeTimer;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    /// <summary>
    /// Llamar justo después de Instantiate para darle dirección y velocidad.
    /// El daño está fijo a 1 (= vida completa del jugador).
    /// </summary>
    public void Initialize(Vector3 direction, float speed)
    {
        lifeTimer = 0f;
        rb.linearVelocity = direction.normalized * speed;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
            DestroyProjectile();
    }

    void OnCollisionEnter(Collision collision)
    {
        // ── ¿Golpeó al jugador? ──────────────────────────────────────────────
        // Busca PlayerController en el objeto golpeado o en su raíz
        PlayerController pc = collision.collider.GetComponentInParent<PlayerController>();
        if (pc != null && pc.model != null)
        {
            pc.model.TakeDamage(1);  // 1 = vida completa, igual que el melee
        }

        // ── Efectos de impacto ───────────────────────────────────────────────
        if (impactFX != null)
        {
            ParticleSystem fx = Instantiate(impactFX, transform.position, Quaternion.identity);
            Destroy(fx.gameObject, fx.main.duration + 0.5f);
        }

        if (impactSFX != null)
            AudioSource.PlayClipAtPoint(impactSFX, transform.position);

        DestroyProjectile();
    }

    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}