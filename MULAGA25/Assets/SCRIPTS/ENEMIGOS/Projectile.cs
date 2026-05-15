// ─────────────────────────────────────────────────────────────
// Projectile.cs
// ─────────────────────────────────────────────────────────────

using UnityEngine;

/// <summary>
/// Proyectil del enemigo de distancia.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Lifetime")]
    public float maxLifetime = 5f;

    [Header("Impact FX")]
    public ParticleSystem impactFX;

    public AudioClip impactSFX;

    // ─────────────────────────────────────────────

    private Rigidbody rb;

    private float lifeTimer;

    // Multiplicador gravedad
    private float gravityMultiplier = 1f;

    // ─────────────────────────────────────────────

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Gravedad manual
        rb.useGravity = false;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;
    }

    /// <summary>
    /// Inicializa velocidad y gravedad.
    /// </summary>
    public void Initialize(
        Vector3 direction,
        float speed,
        float gravity = 1f
    )
    {
        gravityMultiplier = gravity;

        lifeTimer = 0f;

        rb.linearVelocity =
            direction.normalized * speed;

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer >= maxLifetime)
        {
            DestroyProjectile();
        }
    }

    void FixedUpdate()
    {
        // Gravedad personalizada
        rb.AddForce(
            Physics.gravity * gravityMultiplier,
            ForceMode.Acceleration
        );
    }

    void OnCollisionEnter(Collision collision)
    {
        // ─────────────────────────────────────
        // Daño jugador
        // ─────────────────────────────────────

        PlayerController pc =
            collision.collider.GetComponentInParent<PlayerController>();

        if (pc != null && pc.model != null)
        {
            pc.model.TakeDamage(1);
        }

        // ─────────────────────────────────────
        // FX
        // ─────────────────────────────────────

        if (impactFX != null)
        {
            ParticleSystem fx =
                Instantiate(
                    impactFX,
                    transform.position,
                    Quaternion.identity
                );

            Destroy(
                fx.gameObject,
                fx.main.duration + 0.5f
            );
        }

        if (impactSFX != null)
        {
            AudioSource.PlayClipAtPoint(
                impactSFX,
                transform.position
            );
        }

        DestroyProjectile();
    }

    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}