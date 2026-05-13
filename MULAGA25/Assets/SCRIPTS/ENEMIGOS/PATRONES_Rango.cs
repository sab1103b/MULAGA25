using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo de distancia.
/// 
/// MOVIMIENTO:
///   - Mantiene siempre preferredDistance con el jugador (se aleja/acerca suavemente).
///   - Si el jugador invade retreatDistance, espera retreatDelay segundos y luego huye.
///   - Si una pared bloquea la huida, se detiene hasta que el jugador vuelva a alejarse.
///   - Al volver a la distancia preferida, retoma el comportamiento normal (órbita + ataque).
///
/// ATAQUE (según distancia):
///   - Mediana (retreatDistance … mediumRangeThreshold): ráfaga de 5 proyectiles lentos.
///   - Larga   (mediumRangeThreshold … attackRange):     un proyectil rápido único.
///   - Ambos hacen 1 hit de daño completo (model.TakeDamage(1)).
/// </summary>
public class PATRONES_Rango : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  REFERENCIAS
    // ─────────────────────────────────────────────
    [Header("Player Reference")]
    public Transform player;
    private Transform playerHead;

    // ─────────────────────────────────────────────
    //  SUELO & PAREDES
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
    //  MOVIMIENTO
    // ─────────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed = 4f;
    /// <summary>Distancia de combate ideal. El enemigo intenta mantenerse aquí siempre.</summary>
    public float preferredDistance = 12f;
    /// <summary>Si el jugador se acerca menos que esto, arranca el contador de huida.</summary>
    public float retreatDistance = 7f;
    /// <summary>Segundos que el jugador debe estar dentro de retreatDistance antes de que el enemigo huya.</summary>
    public float retreatDelay = 1.2f;
    /// <summary>Velocidad angular de órbita lateral (°/s).</summary>
    public float orbitSpeed = 40f;
    /// <summary>Suavizado general de posición.</summary>
    public float smoothFactor = 5f;

    // ─────────────────────────────────────────────
    //  ATAQUE
    // ─────────────────────────────────────────────
    [Header("Attack - General")]
    public float attackAngle = 35f;
    public float attackRange = 25f;
    public float attackCooldown = 3f;
    public LayerMask attackLayerMask;
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Attack - Mediana distancia (ráfaga lenta)")]
    public float mediumRangeThreshold = 14f;
    public int burstCount = 3;
    public float burstProjectileSpeed = 15f;
    public float burstInterval = 0.18f;

    [Header("Attack - Larga distancia (disparo rápido)")]
    public float sniperProjectileSpeed = 30f;

    // ─────────────────────────────────────────────
    //  AUDIO
    // ─────────────────────────────────────────────
    [Header("Audio")]
    public AudioClip attackSFX;
    private AudioSource audioSource;

    // ─────────────────────────────────────────────
    //  ESTADO INTERNO
    // ─────────────────────────────────────────────

    private enum RetreatState { Normal, WaitingToFlee, Fleeing, BlockedByWall }
    private RetreatState retreatState = RetreatState.Normal;
    private float retreatDelayTimer = 0f;
    private Vector3 fleeDirection = Vector3.zero;

    private float attackTimer = 0f;
    private bool isFiringBurst = false;
    private float globalTimer = 0f;

    // ─────────────────────────────────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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
        globalTimer = 0f;

        ResolvePlayerHead();

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Stop();
            audioSource.Play();
        }
    }

    void Update()
    {
        if (playerHead == null) { ResolvePlayerHead(); return; }

        globalTimer += Time.deltaTime;
        attackTimer += Time.deltaTime;

        float dist = Vector3.Distance(transform.position, playerHead.position);

        // 1. Actualizar máquina de estados de huida
        UpdateRetreatState(dist);

        // 2. Calcular posición objetivo
        Vector3 targetPos = CalculateMovement(dist);
        targetPos = AvoidObstacles(targetPos);
        targetPos = AdjustToGround(targetPos);
        transform.position = targetPos;

        // 3. Rotación: siempre mirando al jugador
        RotateTowardPlayer();

        // 4. Ataque (solo en estado Normal)
        if (retreatState == RetreatState.Normal && !isFiringBurst && CanAttack())
            PerformAttack();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  MÁQUINA DE ESTADOS DE HUIDA
    // ─────────────────────────────────────────────────────────────────────────

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

                // Jugador se alejó antes del delay → cancelar
                if (dist >= retreatDistance)
                {
                    retreatState = RetreatState.Normal;
                    break;
                }

                if (retreatDelayTimer >= retreatDelay)
                {
                    fleeDirection = (transform.position - playerHead.position);
                    fleeDirection.y = 0f;
                    fleeDirection.Normalize();
                    retreatState = RetreatState.Fleeing;
                }
                break;

            case RetreatState.Fleeing:
                // ¿Pared delante?
                if (WallAhead(fleeDirection))
                {
                    retreatState = RetreatState.BlockedByWall;
                    break;
                }

                // ¿Recuperó la distancia preferida?
                if (dist >= preferredDistance)
                {
                    retreatState = RetreatState.Normal;
                    break;
                }

                // Actualizar dirección de huida en tiempo real para movimiento fluido
                fleeDirection = (transform.position - playerHead.position);
                fleeDirection.y = 0f;
                fleeDirection.Normalize();
                break;

            case RetreatState.BlockedByWall:
                // Espera quieto hasta que el jugador se aleje
                if (dist >= preferredDistance)
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

    // ─────────────────────────────────────────────────────────────────────────
    //  MOVIMIENTO
    // ─────────────────────────────────────────────────────────────────────────

    Vector3 CalculateMovement(float dist)
    {
        // Quieto si está bloqueado por pared
        if (retreatState == RetreatState.BlockedByWall)
            return transform.position;

        // Huyendo: moverse en dirección de huida
        if (retreatState == RetreatState.Fleeing)
            return transform.position + fleeDirection * moveSpeed * Time.deltaTime;

        // Esperando el delay: sin movimiento
        if (retreatState == RetreatState.WaitingToFlee)
            return transform.position;

        // ── NORMAL: órbita + corrección de distancia ──────────────────────────

        // Corrección radial: empuja hacia/desde el jugador según el error de distancia
        Vector3 toPlayer = playerHead.position - transform.position;
        toPlayer.y = 0f;
        float distError = toPlayer.magnitude - preferredDistance;  // + = muy lejos, - = muy cerca
        Vector3 radialMove = toPlayer.normalized * Mathf.Clamp(distError, -moveSpeed, moveSpeed);

        // Órbita lateral
        float oscillationY = Mathf.Sin(globalTimer * 0.8f) * 0.3f;
        Vector3 orbitOffset = transform.position - playerHead.position;
        orbitOffset.y = 0f;
        orbitOffset = orbitOffset.normalized * preferredDistance;
        orbitOffset = Quaternion.Euler(0, orbitSpeed * Time.deltaTime, 0) * orbitOffset;

        Vector3 orbitTarget = playerHead.position + orbitOffset;
        orbitTarget.y += oscillationY * Time.deltaTime * smoothFactor;

        // Combinar suavizado de órbita + corrección radial
        return Vector3.Lerp(transform.position, orbitTarget, smoothFactor * Time.deltaTime)
               + radialMove * Time.deltaTime;
    }

    void RotateTowardPlayer()
    {
        Vector3 dir = playerHead.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        // +180° porque el modelo mira hacia atrás (igual que PATRONES.cs)
        Quaternion target = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180f, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, 6f * Time.deltaTime);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ATAQUE
    // ─────────────────────────────────────────────────────────────────────────

    bool CanAttack()
    {
        if (attackTimer < attackCooldown) return false;

        float dist = Vector3.Distance(transform.position, playerHead.position);
        if (dist > attackRange) return false;

        Vector3 dirToPlayer = (playerHead.position - transform.position).normalized;
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
            FireProjectile(burstProjectileSpeed);
            PlayAttackSFX();
            yield return new WaitForSeconds(burstInterval);
        }

        isFiringBurst = false;
    }

    void FireSniper()
    {
        FireProjectile(sniperProjectileSpeed);
        PlayAttackSFX();
    }

    /// <summary>
    /// Instancia un proyectil. El daño (1 vida completa) lo gestiona Projectile.cs
    /// directamente con model.TakeDamage(1) al colisionar con el jugador.
    /// </summary>
    void FireProjectile(float speed)
    {
        if (projectilePrefab == null || playerHead == null) return;

        Transform origin = firePoint != null ? firePoint : transform;
        GameObject proj = Instantiate(projectilePrefab, origin.position, Quaternion.identity);
        Projectile p = proj.GetComponent<Projectile>();

        if (p != null)
            p.Initialize((playerHead.position - origin.position).normalized, speed);
    }

    void PlayAttackSFX()
    {
        if (audioSource != null && attackSFX != null)
            audioSource.PlayOneShot(attackSFX);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  SUELO & OBSTÁCULOS
    // ─────────────────────────────────────────────────────────────────────────

    Vector3 AdjustToGround(Vector3 targetPosition)
    {
        Vector3 rayOrigin = targetPosition + Vector3.up * 5f;
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance, groundLayer))
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

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    void ResolvePlayerHead()
    {
        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            playerHead = mainCam.transform;
        }
    }
}