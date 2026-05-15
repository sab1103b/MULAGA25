// ─────────────────────────────────────────────────────────────
// PATRONES_Rango.cs — corregido para Animator con parámetro Shoot
// ─────────────────────────────────────────────────────────────

using UnityEngine;
using System.Collections;

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

    [Header("Animator")]
    public Animator animator;

    [Tooltip("Parámetro Trigger que tienes en el Animator. En tu caso es Shoot.")]
    public string shootTriggerName = "Shoot";

    [Tooltip("Opcional. Si agregas Speed como Float, controla caminar/idle.")]
    public string speedParamName = "Speed";

    [Tooltip("Opcional. Si agregas Die como Trigger, reproduce muerte.")]
    public string dieTriggerName = "Die";

    [Tooltip("Si tu Animator NO tiene Speed, deja esto en false.")]
    public bool useSpeedParameter = false;

    [Tooltip("Si tu Animator NO tiene Die, deja esto en false.")]
    public bool useDieParameter = false;

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
    public float retreatDistance = 4f;
    public float safeDistance = 10f;
    public float approachDistance = 12f;
    public float orbitSpeed = 40f;
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
    // ESTADOS INTERNOS
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
    private bool isDead = false;

    // ─────────────────────────────────────────────
    // UNITY
    // ─────────────────────────────────────────────

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
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
        isDead = false;
        retreatState = RetreatState.Normal;
        retreatDelayTimer = 0f;
        fleeDirection = Vector3.zero;
        attackTimer = attackCooldown * 0.5f;
        isFiringBurst = false;

        ResolvePlayerHead();
        PlaySpawnSFX();

        if (useSpeedParameter)
            SafeSetFloat(speedParamName, 0f);
    }

    void Update()
    {
        if (isDead) return;

        if (playerHead == null)
        {
            ResolvePlayerHead();
            return;
        }

        attackTimer += Time.deltaTime;

        float dist = Vector3.Distance(transform.position, playerHead.position);

        UpdateRetreatState(dist);

        Vector3 previousPosition = transform.position;
        Vector3 targetPos = CalculateMovement(dist);

        targetPos = AvoidObstacles(targetPos);
        targetPos = AdjustToGround(targetPos);

        transform.position = targetPos;

        RotateTowardPlayer();

        UpdateMovementAnimation(previousPosition);

        if (retreatState == RetreatState.Normal && !isFiringBurst && CanAttack())
            PerformAttack();
    }

    // ─────────────────────────────────────────────
    // ANIMATOR SEGURO
    // ─────────────────────────────────────────────

    bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType type)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == type)
                return true;
        }

        return false;
    }

    void SafeSetTrigger(string paramName)
    {
        if (animator == null) return;

        if (HasAnimatorParameter(paramName, AnimatorControllerParameterType.Trigger))
            animator.SetTrigger(paramName);
        else
            Debug.LogWarning("[PATRONES_Rango] El Animator no tiene el Trigger: " + paramName, this);
    }

    void SafeSetFloat(string paramName, float value)
    {
        if (animator == null) return;

        if (HasAnimatorParameter(paramName, AnimatorControllerParameterType.Float))
            animator.SetFloat(paramName, value);
    }

    void UpdateMovementAnimation(Vector3 previousPosition)
    {
        if (!useSpeedParameter) return;
        if (animator == null) return;

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        float speed = (transform.position - previousPosition).magnitude / deltaTime;

        SafeSetFloat(speedParamName, speed);
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
                    fleeDirection = transform.position - playerHead.position;
                    fleeDirection.y = 0f;

                    if (fleeDirection.sqrMagnitude > 0.001f)
                        fleeDirection.Normalize();
                    else
                        fleeDirection = -transform.forward;

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

                fleeDirection = transform.position - playerHead.position;
                fleeDirection.y = 0f;

                if (fleeDirection.sqrMagnitude > 0.001f)
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
        if (retreatState == RetreatState.BlockedByWall)
            return transform.position;

        if (retreatState == RetreatState.Fleeing)
            return transform.position + fleeDirection * moveSpeed * Time.deltaTime;

        if (retreatState == RetreatState.WaitingToFlee)
            return transform.position;

        Vector3 toPlayer = playerHead.position - transform.position;
        toPlayer.y = 0f;

        Vector3 move = Vector3.zero;

        if (dist > approachDistance)
        {
            move = toPlayer.normalized * moveSpeed;
        }
        else if (dist >= safeDistance && dist <= approachDistance)
        {
            Vector3 orbitOffset = transform.position - playerHead.position;
            orbitOffset.y = 0f;

            if (orbitOffset.sqrMagnitude > 0.001f)
                orbitOffset = orbitOffset.normalized * safeDistance;
            else
                orbitOffset = -transform.forward * safeDistance;

            orbitOffset = Quaternion.Euler(0f, orbitSpeed * Time.deltaTime, 0f) * orbitOffset;

            Vector3 orbitTarget = playerHead.position + orbitOffset;
            Vector3 dir = orbitTarget - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
                move = dir.normalized * moveSpeed;
        }

        return transform.position + move * Time.deltaTime;
    }

    void RotateTowardPlayer()
    {
        if (playerHead == null) return;

        Vector3 dir = playerHead.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        /*
         * IMPORTANTE:
         * En tu código anterior se usaba:
         * Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180f, 0)
         *
         * Eso es porque tu modelo probablemente mira al revés.
         * Si el enemigo queda de espaldas, deja el 180.
         * Si queda mirando bien, cambia rotationOffsetY a 0.
         */

        float rotationOffsetY = 180f;

        Quaternion target = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, rotationOffsetY, 0f);
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

        Vector3 dirToPlayer = playerHead.position - transform.position;
        dirToPlayer.y = 0f;

        if (dirToPlayer.sqrMagnitude < 0.001f) return false;

        dirToPlayer.Normalize();

        /*
         * Como el modelo está girado 180°, usamos -transform.forward.
         * Si luego corriges el modelo y mira bien hacia adelante,
         * cambia -transform.forward por transform.forward.
         */

        Vector3 enemyForward = -transform.forward;

        if (Vector3.Angle(enemyForward, dirToPlayer) > attackAngle)
            return false;

        if (Physics.Linecast(transform.position, playerHead.position, obstacleLayer))
            return false;

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
            if (isDead) break;

            SafeSetTrigger(shootTriggerName);

            FireProjectile(burstProjectileSpeed, 3.5f);
            PlayAttackSFX();

            yield return new WaitForSeconds(burstInterval);
        }

        isFiringBurst = false;
    }

    void FireSniper()
    {
        SafeSetTrigger(shootTriggerName);

        FireProjectile(sniperProjectileSpeed, 0.3f);
        PlayAttackSFX();
    }

    void FireProjectile(float speed, float gravityMultiplier)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[PATRONES_Rango] No hay ProjectilePrefab asignado.", this);
            return;
        }

        if (playerHead == null) return;

        Transform origin = firePoint != null ? firePoint : transform;

        GameObject proj = Instantiate(projectilePrefab, origin.position, Quaternion.identity);

        Projectile p = proj.GetComponent<Projectile>();

        if (p != null)
        {
            Vector3 dir = playerHead.position - origin.position;

            if (dir.sqrMagnitude > 0.001f)
            {
                dir.Normalize();
                p.Initialize(dir, speed, gravityMultiplier);
            }
        }
        else
        {
            Debug.LogWarning("[PATRONES_Rango] El prefab del proyectil no tiene script Projectile.", proj);
        }
    }

    // ─────────────────────────────────────────────
    // MUERTE — llamado desde EnemyLife.Die()
    // ─────────────────────────────────────────────

    public void TriggerDeath()
    {
        if (isDead) return;

        isDead = true;
        isFiringBurst = false;

        if (useSpeedParameter)
            SafeSetFloat(speedParamName, 0f);

        if (useDieParameter)
            SafeSetTrigger(dieTriggerName);

        PlayDeathSFX();
    }

    // ─────────────────────────────────────────────
    // SUELO & OBSTÁCULOS
    // ─────────────────────────────────────────────

    Vector3 AdjustToGround(Vector3 targetPosition)
    {
        Vector3 rayOrigin = targetPosition + Vector3.up * 5f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            float desiredY = hit.point.y + groundOffset;
            targetPosition.y = Mathf.Lerp(transform.position.y, desiredY, heightSmooth * Time.deltaTime);
        }

        return targetPosition;
    }

    Vector3 AvoidObstacles(Vector3 targetPosition)
    {
        Vector3 currentPos = transform.position;
        Vector3 moveDir = targetPosition - currentPos;
        float moveDist = moveDir.magnitude;

        if (moveDist < 0.001f)
            return targetPosition;

        moveDir.Normalize();

        if (Physics.SphereCast(currentPos, obstacleRadius, moveDir, out RaycastHit hit, obstacleCheckDistance, obstacleLayer))
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
        if (player != null)
        {
            playerHead = player;
            return;
        }

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

    // ─────────────────────────────────────────────
    // GIZMOS
    // ─────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);

        if (firePoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(firePoint.position, 0.12f);
        }
    }
}