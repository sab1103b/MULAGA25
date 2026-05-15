// ─────────────────────────────────────────────────────────────
// PATRONES_Rango.cs — con animaciones conectadas al Animator
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo de distancia con animaciones conectadas al Animator.
///
/// PARÁMETROS REQUERIDOS EN EL ANIMATOR CONTROLLER:
///   - Speed        (Float)   → controla transición Idle ↔ Walk
///   - Attack1      (Trigger) → dispara Ataque 1  (sniper)
///   - Attack2      (Trigger) → dispara Ataque 2  (primer disparo de ráfaga)
///   - AttackFlash  (Trigger) → dispara Ataque Flash (disparos 2-N de ráfaga)
///   - Die          (Trigger) → dispara animación de muerte
///
/// TRANSICIONES RECOMENDADAS EN EL ANIMATOR:
///   Any State → Attack1      (Trigger: Attack1,     Has Exit Time: OFF)
///   Any State → Attack2      (Trigger: Attack2,     Has Exit Time: OFF)
///   Any State → AttackFlash  (Trigger: AttackFlash, Has Exit Time: OFF)
///   Any State → Die          (Trigger: Die,         Has Exit Time: OFF)
///   Idle ↔ Walk              (Float:  Speed, umbral ~0.1)
///   Attack1/2/Flash → Walk   (Has Exit Time: ON — que terminen antes de volver)
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
    // ANIMATOR
    // ─────────────────────────────────────────────

    private Animator animator;

    // Cambia estos strings si tus parámetros tienen otros nombres.
    // Es preferible cambiarlos aquí que en el Animator.
    private const string PARAM_SPEED        = "Speed";
    private const string PARAM_ATTACK1      = "Attack1";
    private const string PARAM_ATTACK2      = "Attack2";
    private const string PARAM_ATTACK_FLASH = "AttackFlash";
    private const string PARAM_DIE          = "Die";

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
    public float retreatDistance  = 4f;
    public float safeDistance     = 10f;
    public float approachDistance = 12f;
    public float orbitSpeed       = 40f;
    public float retreatDelay     = 1f;

    // ─────────────────────────────────────────────
    // ATAQUE
    // ─────────────────────────────────────────────

    [Header("Attack - General")]
    public float attackAngle    = 35f;
    public float attackRange    = 25f;
    public float attackCooldown = 3f;
    public LayerMask attackLayerMask;
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Cambio de ataque")]
    public float mediumRangeThreshold = 8f;

    [Header("Ráfaga")]
    public int   burstCount          = 3;
    public float burstProjectileSpeed = 15f;
    public float burstInterval       = 0.18f;

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
    // ESTADOS INTERNOS
    // ─────────────────────────────────────────────

    private enum RetreatState
    {
        Normal,
        WaitingToFlee,
        Fleeing,
        BlockedByWall
    }

    private RetreatState retreatState     = RetreatState.Normal;
    private float        retreatDelayTimer = 0f;
    private Vector3      fleeDirection    = Vector3.zero;
    private float        attackTimer      = 0f;
    private bool         isFiringBurst    = false;

    // ─────────────────────────────────────────────
    // UNITY
    // ─────────────────────────────────────────────

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator    = GetComponent<Animator>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop        = false;
        }

        if (animator == null)
            Debug.LogWarning("[PATRONES_Rango] No se encontró Animator en " + gameObject.name, this);
    }

    void Start()
    {
        ResolvePlayerHead();
    }

    void OnEnable()
    {
        retreatState      = RetreatState.Normal;
        retreatDelayTimer = 0f;
        fleeDirection     = Vector3.zero;
        attackTimer       = attackCooldown * 0.5f;
        isFiringBurst     = false;

        ResolvePlayerHead();
        PlaySpawnSFX();

        if (animator != null)
            animator.SetFloat(PARAM_SPEED, 0f);
    }

    void Update()
    {
        if (playerHead == null)
        {
            ResolvePlayerHead();
            return;
        }

        attackTimer += Time.deltaTime;

        float dist = Vector3.Distance(transform.position, playerHead.position);

        UpdateRetreatState(dist);

        Vector3 previousPosition = transform.position;
        Vector3 targetPos        = CalculateMovement(dist);

        targetPos = AvoidObstacles(targetPos);
        targetPos = AdjustToGround(targetPos);

        transform.position = targetPos;

        RotateTowardPlayer();

        // ── Animación de movimiento ───────────────────────────────
        UpdateMovementAnimation(previousPosition);

        // ── Ataque ───────────────────────────────────────────────
        if (retreatState == RetreatState.Normal && !isFiringBurst && CanAttack())
            PerformAttack();
    }

    // ─────────────────────────────────────────────
    // ANIMATOR
    // ─────────────────────────────────────────────

    void UpdateMovementAnimation(Vector3 previousPosition)
    {
        if (animator == null) return;

        float speed = (transform.position - previousPosition).magnitude / Time.deltaTime;
        animator.SetFloat(PARAM_SPEED, speed);
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
                    retreatState      = RetreatState.WaitingToFlee;
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
                    fleeDirection   = (transform.position - playerHead.position);
                    fleeDirection.y = 0f;
                    fleeDirection.Normalize();
                    retreatState    = RetreatState.Fleeing;
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

                fleeDirection   = (transform.position - playerHead.position);
                fleeDirection.y = 0f;
                fleeDirection.Normalize();
                break;

            case RetreatState.BlockedByWall:
                if (dist >= safeDistance)
                    retreatState = RetreatState.Normal;
                break;
        }
    }

    bool WallAhead(Vector3 dir)
    {
        if (dir == Vector3.zero) return false;

        return Physics.SphereCast(
            transform.position, obstacleRadius, dir,
            out _, obstacleCheckDistance * 1.5f, obstacleLayer
        );
    }

    // ─────────────────────────────────────────────
    // MOVIMIENTO
    // ─────────────────────────────────────────────

    Vector3 CalculateMovement(float dist)
    {
        if (retreatState == RetreatState.BlockedByWall)
            return transform.position;

        if (retreatState == RetreatState.Fleeing)
            return transform.position + fleeDirection * moveSpeed * Time.deltaTime;

        if (retreatState == RetreatState.WaitingToFlee)
            return transform.position;

        Vector3 toPlayer = playerHead.position - transform.position;
        toPlayer.y       = 0f;
        Vector3 move     = Vector3.zero;

        if (dist > approachDistance)
        {
            move = toPlayer.normalized * moveSpeed;
        }
        else if (dist >= safeDistance && dist <= approachDistance)
        {
            Vector3 orbitOffset = transform.position - playerHead.position;
            orbitOffset.y       = 0f;
            orbitOffset         = orbitOffset.normalized * safeDistance;
            orbitOffset         = Quaternion.Euler(0, orbitSpeed * Time.deltaTime, 0) * orbitOffset;
            Vector3 orbitTarget = playerHead.position + orbitOffset;
            move                = (orbitTarget - transform.position).normalized * moveSpeed;
        }

        return transform.position + move * Time.deltaTime;
    }

    void RotateTowardPlayer()
    {
        Vector3 dir = playerHead.position - transform.position;
        dir.y       = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion target  = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180f, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, 6f * Time.deltaTime);
    }

    // ─────────────────────────────────────────────
    // ATAQUE
    // ─────────────────────────────────────────────

    bool CanAttack()
    {
        if (attackTimer < attackCooldown) return false;

        float dist = Vector3.Distance(transform.position, playerHead.position);
        if (dist > attackRange) return false;

        Vector3 dirToPlayer = (playerHead.position - transform.position).normalized;

        // NOTA: Se usa -transform.forward porque el modelo está orientado al revés.
        // Si tu modelo apunta hacia adelante, cambia a transform.forward.
        if (Vector3.Angle(-transform.forward, dirToPlayer) > attackAngle) return false;

        if (Physics.Linecast(transform.position, playerHead.position, obstacleLayer)) return false;

        return true;
    }

    void PerformAttack()
    {
        attackTimer = 0f;

        float dist = Vector3.Distance(transform.position, playerHead.position);

        if (dist <= mediumRangeThreshold)
            StartCoroutine(FireBurst());
        else
            FireSniper();
    }

    IEnumerator FireBurst()
    {
        isFiringBurst = true;

        for (int i = 0; i < burstCount; i++)
        {
            if (!gameObject.activeInHierarchy) break;

            // Primer disparo → Ataque 2; los siguientes → Ataque Flash
            if (animator != null)
            {
                if (i == 0)
                    animator.SetTrigger(PARAM_ATTACK2);
                else
                    animator.SetTrigger(PARAM_ATTACK_FLASH);
            }

            FireProjectile(burstProjectileSpeed, 3.5f);
            PlayAttackSFX();

            yield return new WaitForSeconds(burstInterval);
        }

        isFiringBurst = false;
    }

    void FireSniper()
    {
        if (animator != null)
            animator.SetTrigger(PARAM_ATTACK1);

        FireProjectile(sniperProjectileSpeed, 0.3f);
        PlayAttackSFX();
    }

    void FireProjectile(float speed, float gravityMultiplier)
    {
        if (projectilePrefab == null || playerHead == null) return;

        Transform origin   = firePoint != null ? firePoint : transform;
        GameObject proj    = Instantiate(projectilePrefab, origin.position, Quaternion.identity);
        Projectile p       = proj.GetComponent<Projectile>();

        if (p != null)
        {
            Vector3 dir = (playerHead.position - origin.position).normalized;
            p.Initialize(dir, speed, gravityMultiplier);
        }
    }

    // ─────────────────────────────────────────────
    // MUERTE — llamado desde EnemyLife.Die()
    // ─────────────────────────────────────────────

    /// <summary>
    /// Llama esto desde EnemyLife.Die() ANTES de SetActive(false)
    /// para que la animación de muerte se reproduzca.
    /// </summary>
    public void TriggerDeath()
    {
        if (animator != null)
            animator.SetTrigger(PARAM_DIE);
    }

    // ─────────────────────────────────────────────
    // SUELO & OBSTÁCULOS
    // ─────────────────────────────────────────────

    Vector3 AdjustToGround(Vector3 targetPosition)
    {
        Vector3 rayOrigin = targetPosition + Vector3.up * 5f;
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            float desiredY    = hit.point.y + groundOffset;
            targetPosition.y  = Mathf.Lerp(transform.position.y, desiredY, heightSmooth * Time.deltaTime);
        }

        return targetPosition;
    }

    Vector3 AvoidObstacles(Vector3 targetPosition)
    {
        Vector3 currentPos = transform.position;
        Vector3 moveDir    = targetPosition - currentPos;
        float moveDist     = moveDir.magnitude;

        if (moveDist < 0.001f) return targetPosition;

        moveDir.Normalize();
        RaycastHit hit;

        if (Physics.SphereCast(currentPos, obstacleRadius, moveDir, out hit, obstacleCheckDistance, obstacleLayer))
        {
            Vector3 slideDir = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;

            if (slideDir.sqrMagnitude < 0.01f)
                slideDir = Vector3.Cross(hit.normal, Vector3.up).normalized;

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
            playerHead = mainCam.transform;
    }

    void PlaySpawnSFX()
    {
        if (spawnSFX != null)
            AudioSource.PlayClipAtPoint(spawnSFX, transform.position, 1f);
    }

    void PlayAttackSFX()
    {
        if (audioSource != null && attackSFX != null)
            audioSource.PlayOneShot(attackSFX, 0.06f);
    }

    public void PlayDeathSFX()
    {
        if (deathSFX != null)
            AudioSource.PlayClipAtPoint(deathSFX, transform.position, 1f);
    }
}