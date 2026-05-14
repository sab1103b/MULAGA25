// ─────────────────────────────────────────────────────────────
// PATRONES_Rango.cs
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo de distancia.
///
/// ZONAS:
///   - 0m → 8m     = ráfaga con mucha caída.
///   - 8m+         = sniper rápido con poca caída.
///
/// MOVIMIENTO:
///   - Menos de 4m → huye.
///   - Huye hasta recuperar 10m.
///   - Si hay pared durante huida → quieto.
///   - Más de 12m → se acerca.
///   - Entre 10m y 12m → órbita.
/// </summary>
public class PATRONES_Rango : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // REFERENCIAS
    // ─────────────────────────────────────────────

    [Header("Player Reference")]
    public Transform player;

    private Transform playerHead;

    // ─────────────────────────────────────────────
    // SUELO & PAREDES
    // ─────────────────────────────────────────────

    [Header("Ground Detection")]
    public float groundCheckDistance = 20f;
    public float groundOffset = 0.1f;
    public float heightSmooth = 8f;
    public LayerMask groundLayer;

    [Header("Obstacle / Wall Detection")]
    public float obstacleCheckDistance = 1.5f;
    public float obstacleRadius = 0.5f;
    public LayerMask obstacleLayer;

    // ─────────────────────────────────────────────
    // MOVIMIENTO
    // ─────────────────────────────────────────────

    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Distancias IA")]

    // Menos de esto → huida
    public float retreatDistance = 4f;

    // Recupera esta distancia
    public float safeDistance = 10f;

    // Más de esto → acercarse
    public float approachDistance = 12f;

    // Velocidad órbita
    public float orbitSpeed = 40f;

    // Delay antes de huir
    public float retreatDelay = 1f;

    // ─────────────────────────────────────────────
    // ATAQUE
    // ─────────────────────────────────────────────

    [Header("Attack - General")]
    public float attackAngle = 35f;
    public float attackRange = 25f;
    public float attackCooldown = 3f;

    public LayerMask attackLayerMask;

    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Cambio de ataque")]
    public float mediumRangeThreshold = 8f;

    [Header("Ráfaga")]
    public int burstCount = 3;
    public float burstProjectileSpeed = 15f;
    public float burstInterval = 0.18f;

    [Header("Sniper")]
    public float sniperProjectileSpeed = 30f;

    // ─────────────────────────────────────────────
    // AUDIO
    // ─────────────────────────────────────────────

    [Header("Audio")]

    public AudioClip spawnSFX;
    public AudioClip attackSFX;
    public AudioClip deathSFX;

    private AudioSource audioSource;

    // ─────────────────────────────────────────────
    // ESTADOS
    // ─────────────────────────────────────────────

    private enum RetreatState
    {
        Normal,
        WaitingToFlee,
        Fleeing,
        BlockedByWall
    }

    private RetreatState retreatState = RetreatState.Normal;

    private float retreatDelayTimer = 0f;

    private Vector3 fleeDirection = Vector3.zero;

    private float attackTimer = 0f;

    private bool isFiringBurst = false;

    // ─────────────────────────────────────────────
    // UNITY
    // ─────────────────────────────────────────────

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    void Start()
    {
        ResolvePlayerHead();
    }

    void OnEnable()
    {
        retreatState = RetreatState.Normal;

        retreatDelayTimer = 0f;

        fleeDirection = Vector3.zero;

        attackTimer = attackCooldown * 0.5f;

        isFiringBurst = false;

        ResolvePlayerHead();

        PlaySpawnSFX();
    }

    void Update()
    {
        if (playerHead == null)
        {
            ResolvePlayerHead();
            return;
        }

        attackTimer += Time.deltaTime;

        float dist =
            Vector3.Distance(
                transform.position,
                playerHead.position
            );

        // Máquina de estados
        UpdateRetreatState(dist);

        // Movimiento
        Vector3 targetPos = CalculateMovement(dist);

        targetPos = AvoidObstacles(targetPos);

        targetPos = AdjustToGround(targetPos);

        transform.position = targetPos;

        // Rotación
        RotateTowardPlayer();

        // Ataque
        if (
            retreatState == RetreatState.Normal &&
            !isFiringBurst &&
            CanAttack()
        )
        {
            PerformAttack();
        }
    }

    // ─────────────────────────────────────────────
    // IA DE HUIDA
    // ─────────────────────────────────────────────

    void UpdateRetreatState(float dist)
    {
        switch (retreatState)
        {
            case RetreatState.Normal:

                if (dist < retreatDistance)
                {
                    retreatState = RetreatState.WaitingToFlee;

                    retreatDelayTimer = 0f;
                }

                break;

            case RetreatState.WaitingToFlee:

                retreatDelayTimer += Time.deltaTime;

                if (dist >= retreatDistance)
                {
                    retreatState = RetreatState.Normal;
                    break;
                }

                if (retreatDelayTimer >= retreatDelay)
                {
                    fleeDirection =
                        (transform.position - playerHead.position);

                    fleeDirection.y = 0f;

                    fleeDirection.Normalize();

                    retreatState = RetreatState.Fleeing;
                }

                break;

            case RetreatState.Fleeing:

                if (WallAhead(fleeDirection))
                {
                    retreatState = RetreatState.BlockedByWall;
                    break;
                }

                if (dist >= safeDistance)
                {
                    retreatState = RetreatState.Normal;
                    break;
                }

                fleeDirection =
                    (transform.position - playerHead.position);

                fleeDirection.y = 0f;

                fleeDirection.Normalize();

                break;

            case RetreatState.BlockedByWall:

                if (dist >= safeDistance)
                {
                    retreatState = RetreatState.Normal;
                }

                break;
        }
    }

    bool WallAhead(Vector3 dir)
    {
        if (dir == Vector3.zero)
            return false;

        return Physics.SphereCast(
            transform.position,
            obstacleRadius,
            dir,
            out _,
            obstacleCheckDistance * 1.5f,
            obstacleLayer
        );
    }

    // ─────────────────────────────────────────────
    // MOVIMIENTO
    // ─────────────────────────────────────────────

    Vector3 CalculateMovement(float dist)
    {
        // Quieto si hay pared durante huida
        if (retreatState == RetreatState.BlockedByWall)
            return transform.position;

        // Huyendo
        if (retreatState == RetreatState.Fleeing)
        {
            return transform.position +
                   fleeDirection *
                   moveSpeed *
                   Time.deltaTime;
        }

        // Esperando huida
        if (retreatState == RetreatState.WaitingToFlee)
            return transform.position;

        // ─────────────────────────────────────
        // NORMAL
        // ─────────────────────────────────────

        Vector3 toPlayer =
            playerHead.position - transform.position;

        toPlayer.y = 0f;

        Vector3 move = Vector3.zero;

        // Muy lejos → acercarse
        if (dist > approachDistance)
        {
            move = toPlayer.normalized * moveSpeed;
        }

        // Zona media → órbita
        else if (dist >= safeDistance && dist <= approachDistance)
        {
            Vector3 orbitOffset =
                transform.position - playerHead.position;

            orbitOffset.y = 0f;

            orbitOffset =
                orbitOffset.normalized * safeDistance;

            orbitOffset =
                Quaternion.Euler(
                    0,
                    orbitSpeed * Time.deltaTime,
                    0
                ) * orbitOffset;

            Vector3 orbitTarget =
                playerHead.position + orbitOffset;

            move =
                (orbitTarget - transform.position).normalized *
                moveSpeed;
        }

        return transform.position + move * Time.deltaTime;
    }

    void RotateTowardPlayer()
    {
        Vector3 dir =
            playerHead.position - transform.position;

        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion target =
            Quaternion.LookRotation(dir) *
            Quaternion.Euler(0, 180f, 0);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                target,
                6f * Time.deltaTime
            );
    }

    // ─────────────────────────────────────────────
    // ATAQUE
    // ─────────────────────────────────────────────

    bool CanAttack()
    {
        if (attackTimer < attackCooldown)
            return false;

        float dist =
            Vector3.Distance(
                transform.position,
                playerHead.position
            );

        if (dist > attackRange)
            return false;

        Vector3 dirToPlayer =
            (playerHead.position - transform.position).normalized;

        if (
            Vector3.Angle(-transform.forward, dirToPlayer)
            > attackAngle
        )
            return false;

        if (
            Physics.Linecast(
                transform.position,
                playerHead.position,
                obstacleLayer
            )
        )
            return false;

        return true;
    }

    void PerformAttack()
    {
        attackTimer = 0f;

        float dist =
            Vector3.Distance(
                transform.position,
                playerHead.position
            );

        // Cerca → ráfaga
        if (dist <= mediumRangeThreshold)
        {
            StartCoroutine(FireBurst());
        }
        // Lejos → sniper
        else
        {
            FireSniper();
        }
    }

    IEnumerator FireBurst()
    {
        isFiringBurst = true;

        for (int i = 0; i < burstCount; i++)
        {
            if (!gameObject.activeInHierarchy)
                break;

            // Mucha caída
            FireProjectile(
                burstProjectileSpeed,
                3.5f
            );

            PlayAttackSFX();

            yield return new WaitForSeconds(
                burstInterval
            );
        }

        isFiringBurst = false;
    }

    void FireSniper()
    {
        // Muy poca caída
        FireProjectile(
            sniperProjectileSpeed,
            0.3f
        );

        PlayAttackSFX();
    }

    void FireProjectile(
        float speed,
        float gravityMultiplier
    )
    {
        if (
            projectilePrefab == null ||
            playerHead == null
        )
            return;

        Transform origin =
            firePoint != null
            ? firePoint
            : transform;

        GameObject proj =
            Instantiate(
                projectilePrefab,
                origin.position,
                Quaternion.identity
            );

        Projectile p =
            proj.GetComponent<Projectile>();

        if (p != null)
        {
            Vector3 dir =
                (
                    playerHead.position -
                    origin.position
                ).normalized;

            p.Initialize(
                dir,
                speed,
                gravityMultiplier
            );
        }
    }

    // ─────────────────────────────────────────────
    // SUELO & OBSTÁCULOS
    // ─────────────────────────────────────────────

    Vector3 AdjustToGround(Vector3 targetPosition)
    {
        Vector3 rayOrigin =
            targetPosition + Vector3.up * 5f;

        RaycastHit hit;

        if (
            Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out hit,
                groundCheckDistance,
                groundLayer
            )
        )
        {
            float desiredY =
                hit.point.y + groundOffset;

            targetPosition.y =
                Mathf.Lerp(
                    transform.position.y,
                    desiredY,
                    heightSmooth * Time.deltaTime
                );
        }

        return targetPosition;
    }

    Vector3 AvoidObstacles(Vector3 targetPosition)
    {
        Vector3 currentPos = transform.position;

        Vector3 moveDir =
            targetPosition - currentPos;

        float moveDist = moveDir.magnitude;

        if (moveDist < 0.001f)
            return targetPosition;

        moveDir.Normalize();

        RaycastHit hit;

        if (
            Physics.SphereCast(
                currentPos,
                obstacleRadius,
                moveDir,
                out hit,
                obstacleCheckDistance,
                obstacleLayer
            )
        )
        {
            Vector3 slideDir =
                Vector3.ProjectOnPlane(
                    moveDir,
                    hit.normal
                ).normalized;

            if (slideDir.sqrMagnitude < 0.01f)
            {
                slideDir =
                    Vector3.Cross(
                        hit.normal,
                        Vector3.up
                    ).normalized;
            }

            return currentPos + slideDir * moveDist;
        }

        return targetPosition;
    }

    // ─────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────

    void ResolvePlayerHead()
    {
        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            playerHead = mainCam.transform;
        }
    }

    void PlaySpawnSFX()
    {
        if (
            spawnSFX != null
        )
        {
            AudioSource.PlayClipAtPoint(
                spawnSFX,
                transform.position,
                1f
            );
        }
    }

    void PlayAttackSFX()
    {
        if (
            audioSource != null &&
            attackSFX != null
        )
        {
            audioSource.PlayOneShot(
                attackSFX,
                0.06f // volumen
            );
        }
    }

    public void PlayDeathSFX()
    {
        if (
            deathSFX != null
        )
        {
            AudioSource.PlayClipAtPoint(
                deathSFX,
                transform.position,
                1f
            );
        }
    }
}