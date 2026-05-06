using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour, IDamageable
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  FASES
    // ═══════════════════════════════════════════════════════════════════════════
    public enum BossPhase { Phase1, Phase2, Phase3 }

    private BossPhase currentPhase = BossPhase.Phase1;
    private bool isTransitioning = false;

    // ═══════════════════════════════════════════════════════════════════════════
    //  STATS DE BOSS
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("Boss Stats")]
    public float maxHealth = 1500f;
    public float currentHealth;

    [Tooltip("Reduce el daño recibido. 0.45 significa que recibe el 45% del daño.")]
    [Range(0.05f, 1f)]
    public float incomingDamageMultiplier = 0.45f;

    [Tooltip("Evita que muchas balas le bajen vida en el mismo instante.")]
    public float damageReceiveCooldown = 0.18f;

    private float lastDamageTime = -999f;

    [Header("Boss Phase Gates")]
    public bool phaseGateEnabled = true;

    [Tooltip("El boss no puede bajar de este porcentaje antes de pasar a Fase 2.")]
    [Range(0.5f, 0.9f)]
    public float phase2HealthGate = 0.66f;

    [Tooltip("El boss no puede bajar de este porcentaje antes de pasar a Fase 3.")]
    [Range(0.15f, 0.5f)]
    public float phase3HealthGate = 0.33f;

    // ═══════════════════════════════════════════════════════════════════════════
    //  MOVIMIENTO FELINO VR FRIENDLY
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("Feline Locomotion - VR Friendly")]
    public float orbitDistance = 8f;
    public float minDistance = 5.5f;
    public float maxDistance = 14f;

    [Tooltip("Velocidad cuando se acerca al jugador.")]
    public float approachSpeed = 1.45f;

    [Tooltip("Velocidad cuando retrocede si el jugador está muy cerca.")]
    public float retreatSpeed = 1.85f;

    [Tooltip("Velocidad lateral de acecho.")]
    public float strafeSpeed = 1.15f;

    [Tooltip("Velocidad de rotación suave. Para VR no subir mucho.")]
    public float rotateSpeed = 2.2f;

    [Header("Cat-like Slow Side Movement")]
    public float sideDashInterval = 4.2f;
    public float sideDashDuration = 0.22f;
    public float sideDashSpeed = 1.75f;

    private float sideDashTimer;
    private float sideDashActiveTimer;
    private int strafeDirection = 1;
    private Vector3 sideDashDirection;

    [Header("VR Comfort Rotation")]
    [Tooltip("Cada cuánto el boss actualiza su dirección de mirada. Más alto = menos rotación constante.")]
    public float rotationUpdateInterval = 0.35f;

    [Tooltip("Ángulo mínimo para que el boss decida corregir la mirada.")]
    public float rotationDeadAngle = 8f;

    private float rotationUpdateTimer = 0f;
    private Quaternion targetLookRotation;

    [Header("Damage Reaction")]
    [Tooltip("Distancia que el boss retrocede cuando recibe daño real.")]
    public float damageStepBackDistance = 1.2f;

    [Tooltip("Duración del retroceso por daño.")]
    public float damageStepBackDuration = 0.22f;

    [Tooltip("Tiempo mínimo entre retrocesos para que no tiemble con muchas balas.")]
    public float damageStepBackCooldown = 0.45f;

    private bool isSteppingBackFromDamage = false;
    private float lastStepBackTime = -999f;
    private Coroutine stepBackRoutine;

    // ═══════════════════════════════════════════════════════════════════════════
    //  ATAQUE COMPARTIDO
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("Attack — Shared")]
    public GameObject dangerZonePrefab;
    public GameObject projectilePrefab;
    public Transform attackSpawnPoint;
    public float warningDuration = 2.5f;

    private bool isAttacking = false;
    private float attackTimer = 5f;

    // ═══════════════════════════════════════════════════════════════════════════
    //  FASE 1
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("Phase 1  (HP > 66%)")]
    public float p1AttackCooldown = 7f;
    public float p1SpawnCooldown = 14f;
    public int p1MinionsPerWave = 3;

    // ═══════════════════════════════════════════════════════════════════════════
    //  FASE 2
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("Phase 2  (HP 33–66%)")]
    public float p2AttackCooldown = 4.5f;
    public float p2SpawnCooldown = 9f;
    public int p2MinionsPerWave = 4;
    public int p2ZoneCount = 2;
    public float p2ZoneSpread = 2.8f;

    // ═══════════════════════════════════════════════════════════════════════════
    //  FASE 3
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("Phase 3  (HP < 33%)")]
    public float p3AttackCooldown = 2.5f;
    public float p3SpawnCooldown = 7f;
    public int p3MinionsPerWave = 5;
    public int p3ZoneCount = 3;

    [Tooltip("Separación lateral entre los 3 proyectiles del ataque triple.")]
    public float p3TripleSpawnOffset = 0.45f;

    // ═══════════════════════════════════════════════════════════════════════════
    //  MINIONS / ESCUDO
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("Minions")]
    public MinionSpawner minionSpawner;

    private float spawnCooldown = 15f;
    private int minionsPerWave = 3;
    private float attackCooldown = 7f;
    private float spawnTimer = 10f;

    [Header("Minion Shield")]
    public bool minionShieldEnabled = true;

    [Tooltip("Spawnea una oleada apenas empieza el boss para que no quede vulnerable al inicio.")]
    public bool spawnMinionsOnStart = true;

    [Tooltip("Tiempo mínimo de escudo después de cada oleada.")]
    public float minimumShieldTimeAfterWave = 3f;

    private float shieldUntilTime = 0f;

    // ═══════════════════════════════════════════════════════════════════════════
    //  AGRESIVIDAD
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("Aggression")]
    [Range(1f, 2.5f)]
    public float maxAggressionMult = 1.8f;

    // ═══════════════════════════════════════════════════════════════════════════
    //  TRANSICIÓN POR TIEMPO
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("Phase Timers")]
    public float phase2ForceTime = 45f;
    public float phase3ForceTime = 90f;

    private float fightTimer = 0f;

    // ═══════════════════════════════════════════════════════════════════════════
    //  VISUAL ROBUSTO
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("Visual Feedback")]
    public Color normalFallbackColor = Color.white;

    [Tooltip("Color permanente mientras está invulnerable.")]
    public Color invulnerableColor = new Color(0.1f, 0.45f, 1f, 1f);

    [Tooltip("Flash cuando una bala pega pero el boss bloquea el daño.")]
    public Color blockedHitColor = new Color(0.25f, 0.9f, 1f, 1f);

    [Tooltip("Flash cuando el boss recibe daño real.")]
    public Color damageColor = new Color(1f, 0.05f, 0.02f, 1f);

    public Color phaseTransitionColor = Color.white;

    [Tooltip("Intensidad de emisión para que el flash sea visible incluso con materiales oscuros.")]
    public float emissionIntensity = 2.5f;

    public float realDamageFlashDuration = 0.18f;
    public float blockedDamageFlashDuration = 0.14f;

    private Renderer[] bossRenderers;
    private MaterialPropertyBlock propertyBlock;
    private Color[] originalColors;
    private Coroutine flashRoutine;
    private bool lastInvulnerableState = false;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    // ═══════════════════════════════════════════════════════════════════════════
    //  REFERENCIAS
    // ═══════════════════════════════════════════════════════════════════════════
    [Header("References")]
    public BossArena arena;

    private Transform player;
    private Rigidbody rb;
    private bool isActive = false;

    // ═══════════════════════════════════════════════════════════════════════════
    //  START
    // ═══════════════════════════════════════════════════════════════════════════
    void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            // CLAVE:
            // Kinematic evita que las balas empujen al boss.
            // El boss se mueve por código, no por fuerza física externa.
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            rb.constraints = RigidbodyConstraints.FreezeRotation |
                             RigidbodyConstraints.FreezePositionY;
        }

        CacheBossRenderers();
        FindPlayer();

        ApplyPhaseSettings(BossPhase.Phase1);
        ApplyShieldVisualState(true);

        sideDashTimer = sideDashInterval;
        targetLookRotation = transform.rotation;
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;
        else
            Debug.LogError("[BOSS] No encontró un objeto con tag Player.");
    }

    public void ActivateBoss()
    {
        if (player == null)
            FindPlayer();

        // Evita basura de sesiones anteriores.
        MinionAI.ActiveMinions = 0;

        isActive = true;
        isTransitioning = false;
        isAttacking = false;
        isSteppingBackFromDamage = false;

        currentPhase = BossPhase.Phase1;
        currentHealth = maxHealth;

        attackTimer = 3f;
        spawnTimer = p1SpawnCooldown;
        fightTimer = 0f;

        sideDashTimer = sideDashInterval;
        sideDashActiveTimer = 0f;
        strafeDirection = 1;

        rotationUpdateTimer = 0f;
        targetLookRotation = transform.rotation;

        lastDamageTime = -999f;
        lastStepBackTime = -999f;

        ApplyPhaseSettings(BossPhase.Phase1);

        if (spawnMinionsOnStart)
            SpawnMinionWaveWithShield();
        else
            ForceShieldForSeconds(minimumShieldTimeAfterWave);

        ApplyShieldVisualState(true);

        Debug.Log("[BOSS] Activado — Fase 1.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  UPDATE
    // ═══════════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (!isActive || player == null) return;

        ApplyShieldVisualState(false);

        fightTimer += Time.deltaTime;

        if (!isTransitioning)
        {
            CheckPhaseTransition();
            HandleAreaAttack(GetAggressionMultiplier());
            HandleMinionSpawn();
        }

        // Si está retrocediendo por daño, no mezcles patrón normal.
        // Esto evita movimientos bruscos y mareo en VR.
        if (isSteppingBackFromDamage)
        {
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > 0.001f)
                RotateTowardsPlayerVR(toPlayer.normalized);

            return;
        }

        // Se mueve incluso invulnerable, pero lento y VR friendly.
        HandleFelineLocomotion(GetAggressionMultiplier());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  MOVIMIENTO FELINO VR FRIENDLY
    // ═══════════════════════════════════════════════════════════════════════════
    void HandleFelineLocomotion(float aggrMult)
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;
        if (dist <= 0.01f) return;

        Vector3 dirToPlayer = toPlayer.normalized;
        Vector3 awayFromPlayer = -dirToPlayer;
        Vector3 right = Vector3.Cross(Vector3.up, dirToPlayer).normalized;

        Vector3 desiredMove = Vector3.zero;

        // Movimiento lateral suave tipo acecho.
        sideDashTimer -= Time.deltaTime;

        if (sideDashTimer <= 0f)
        {
            strafeDirection *= -1;

            sideDashDirection = right * strafeDirection;
            sideDashActiveTimer = sideDashDuration;

            sideDashTimer = sideDashInterval / Mathf.Max(1f, aggrMult);
        }

        if (sideDashActiveTimer > 0f)
        {
            desiredMove += sideDashDirection * sideDashSpeed;
            sideDashActiveTimer -= Time.deltaTime;
        }

        // Muy cerca: retrocede despacio.
        if (dist < minDistance)
        {
            desiredMove += awayFromPlayer * retreatSpeed;
            desiredMove += right * strafeDirection * (strafeSpeed * 0.45f);
        }
        // Muy lejos: se acerca despacio.
        else if (dist > maxDistance)
        {
            desiredMove += dirToPlayer * approachSpeed;
        }
        // Distancia ideal: orbita lento.
        else
        {
            float distanceError = dist - orbitDistance;

            desiredMove += right * strafeDirection * strafeSpeed;
            desiredMove += dirToPlayer * distanceError * 0.35f;
        }

        // Para VR no dejamos que la agresividad lo vuelva demasiado rápido.
        float vrSafeAggression = Mathf.Clamp(aggrMult, 1f, 1.25f);
        desiredMove *= vrSafeAggression;

        Vector3 nextPos = transform.position + desiredMove * Time.deltaTime;
        nextPos.y = transform.position.y;

        if (arena == null || arena.IsInsideArena(nextPos))
        {
            transform.position = nextPos;
        }

        RotateTowardsPlayerVR(dirToPlayer);
    }

    void RotateTowardsPlayerVR(Vector3 dirToPlayer)
    {
        if (dirToPlayer.sqrMagnitude <= 0.001f) return;

        rotationUpdateTimer -= Time.deltaTime;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        // Solo actualiza la rotación objetivo cada cierto tiempo
        // y solo si el ángulo realmente cambió.
        if (rotationUpdateTimer <= 0f && angleToPlayer > rotationDeadAngle)
        {
            targetLookRotation = Quaternion.LookRotation(dirToPlayer);
            rotationUpdateTimer = rotationUpdateInterval;
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetLookRotation,
            Time.deltaTime * rotateSpeed
        );
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  REACCIÓN AL DAÑO: PASOS HACIA ATRÁS
    // ═══════════════════════════════════════════════════════════════════════════
    void StepBackFromDamage()
    {
        if (Time.time - lastStepBackTime < damageStepBackCooldown)
            return;

        if (player == null)
            return;

        lastStepBackTime = Time.time;

        if (stepBackRoutine != null)
            StopCoroutine(stepBackRoutine);

        stepBackRoutine = StartCoroutine(StepBackFromDamageRoutine());
    }

    IEnumerator StepBackFromDamageRoutine()
    {
        isSteppingBackFromDamage = true;

        Vector3 fromPlayer = transform.position - player.position;
        fromPlayer.y = 0f;

        if (fromPlayer.sqrMagnitude < 0.001f)
            fromPlayer = -transform.forward;

        fromPlayer.Normalize();

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + fromPlayer * damageStepBackDistance;
        endPos.y = startPos.y;

        if (arena != null && !arena.IsInsideArena(endPos))
        {
            endPos = startPos;
        }

        float timer = 0f;

        while (timer < damageStepBackDuration)
        {
            timer += Time.deltaTime;

            float t = timer / damageStepBackDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, endPos, t);

            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > 0.001f)
                RotateTowardsPlayerVR(toPlayer.normalized);

            yield return null;
        }

        transform.position = endPos;

        isSteppingBackFromDamage = false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  ATAQUES
    // ═══════════════════════════════════════════════════════════════════════════
    void HandleAreaAttack(float aggr)
    {
        if (isAttacking) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        switch (currentPhase)
        {
            case BossPhase.Phase1:
                StartCoroutine(SingleProjectileAttack());
                break;

            case BossPhase.Phase2:
                StartCoroutine(MultiZoneAttack(p2ZoneCount, p2ZoneSpread));
                break;

            case BossPhase.Phase3:
                if (Random.value > 0.4f)
                    StartCoroutine(TripleAttack());
                else
                    StartCoroutine(MultiZoneAttack(p3ZoneCount, p2ZoneSpread * 1.3f));
                break;
        }

        attackTimer = attackCooldown / aggr;
    }

    IEnumerator SingleProjectileAttack()
    {
        isAttacking = true;

        if (attackSpawnPoint == null)
        {
            Debug.LogError("[BOSS] AttackSpawnPoint no asignado.");
            isAttacking = false;
            yield break;
        }

        Vector3 target = player.position;
        target.y = 0.05f;

        GameObject warning = SpawnWarning(target, GetProjectileRadius());

        DangerZoneIndicator dz = warning != null ? warning.GetComponent<DangerZoneIndicator>() : null;
        if (dz != null)
            dz.ShowWarning(warningDuration);

        FireProjectileFromTo(attackSpawnPoint.position, target, warningDuration, warning);

        yield return new WaitForSeconds(warningDuration);

        isAttacking = false;
    }

    IEnumerator MultiZoneAttack(int zoneCount, float spread)
    {
        isAttacking = true;

        if (attackSpawnPoint == null)
        {
            Debug.LogError("[BOSS] AttackSpawnPoint no asignado.");
            isAttacking = false;
            yield break;
        }

        Vector3 center = player.position;
        center.y = 0.05f;

        float radius = GetProjectileRadius();

        for (int i = 0; i < zoneCount; i++)
        {
            Vector3 target = center;

            if (i > 0)
            {
                float angle = (360f / zoneCount) * i;

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    0f,
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                ) * spread;

                target = center + offset;
                target.y = 0.05f;
            }

            GameObject warning = SpawnWarning(target, radius);

            DangerZoneIndicator dz = warning != null ? warning.GetComponent<DangerZoneIndicator>() : null;
            if (dz != null)
                dz.ShowWarning(warningDuration);

            FireProjectileFromTo(attackSpawnPoint.position, target, warningDuration, warning);
        }

        yield return new WaitForSeconds(warningDuration);

        isAttacking = false;
    }

    IEnumerator TripleAttack()
    {
        isAttacking = true;

        if (attackSpawnPoint == null)
        {
            Debug.LogError("[BOSS] AttackSpawnPoint no asignado.");
            isAttacking = false;
            yield break;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.001f)
            toPlayer = transform.forward;

        toPlayer.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, toPlayer).normalized;

        Vector3 lockedTarget = player.position;
        lockedTarget.y = 0.05f;

        float radius = GetProjectileRadius() * 1.2f;

        GameObject sharedWarning = SpawnWarning(lockedTarget, radius);

        DangerZoneIndicator dz = sharedWarning != null ? sharedWarning.GetComponent<DangerZoneIndicator>() : null;
        if (dz != null)
            dz.ShowWarning(warningDuration);

        float[] offsets =
        {
            -p3TripleSpawnOffset,
            0f,
            p3TripleSpawnOffset
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3 spawnPos = attackSpawnPoint.position + right * offsets[i];

            GameObject warningOwner = i == 0 ? sharedWarning : null;

            FireProjectileFromTo(spawnPos, lockedTarget, warningDuration, warningOwner);
        }

        yield return new WaitForSeconds(warningDuration);

        isAttacking = false;
    }

    GameObject SpawnWarning(Vector3 position, float radius)
    {
        if (dangerZonePrefab == null)
        {
            Debug.LogError("[BOSS] DangerZonePrefab no asignado.");
            return null;
        }

        GameObject warning = Instantiate(dangerZonePrefab, position, Quaternion.identity);

        DangerZoneIndicator dz = warning.GetComponent<DangerZoneIndicator>();

        if (dz != null)
            dz.SetRadius(radius);

        return warning;
    }

    void FireProjectileFromTo(Vector3 spawnPos, Vector3 target, float travelTime, GameObject warning)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[BOSS] ProjectilePrefab no asignado.");
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        BossProjectile bp = proj.GetComponent<BossProjectile>();

        if (bp != null)
            bp.Initialize(target, travelTime, warning);
        else
            Debug.LogError("[BOSS] El projectilePrefab no tiene BossProjectile.");
    }

    float GetProjectileRadius()
    {
        if (projectilePrefab == null)
            return 1.8f;

        BossProjectile bp = projectilePrefab.GetComponent<BossProjectile>();

        if (bp != null)
            return bp.explosionRadius;

        return 1.8f;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  MINIONS / ESCUDO
    // ═══════════════════════════════════════════════════════════════════════════
    void HandleMinionSpawn()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f) return;

        SpawnMinionWaveWithShield();

        spawnTimer = spawnCooldown;
    }

    void SpawnMinionWaveWithShield()
    {
        if (minionSpawner != null)
        {
            bool aggressive = currentPhase != BossPhase.Phase1;

            minionSpawner.SpawnWave(minionsPerWave, aggressive);

            ForceShieldForSeconds(minimumShieldTimeAfterWave);

            Debug.Log($"[BOSS] Oleada de {minionsPerWave} minions. Escudo activo.");
        }
        else
        {
            Debug.LogError("[BOSS] MinionSpawner no asignado.");
            ForceShieldForSeconds(minimumShieldTimeAfterWave);
        }
    }

    void ForceShieldForSeconds(float seconds)
    {
        shieldUntilTime = Mathf.Max(shieldUntilTime, Time.time + seconds);
        ApplyShieldVisualState(true);
    }

    bool IsInvulnerable()
    {
        if (!minionShieldEnabled) return false;

        bool hasActiveMinions = MinionAI.ActiveMinions > 0;
        bool hasForcedShield = Time.time < shieldUntilTime;

        return hasActiveMinions || hasForcedShield;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  DAÑO + FEEDBACK
    // ═══════════════════════════════════════════════════════════════════════════
    public void TakeDamage(int damage)
    {
        if (!isActive) return;

        // Invulnerable: bloquea daño, flash azul fuerte.
        if (IsInvulnerable())
        {
            Debug.Log($"[BOSS] BLOQUEÓ DAÑO — minions vivos: {MinionAI.ActiveMinions} | Escudo temporal: {Time.time < shieldUntilTime}");
            StartFlash(blockedHitColor, blockedDamageFlashDuration);
            return;
        }

        // Cooldown de daño para que no se derrita con ráfagas.
        if (Time.time - lastDamageTime < damageReceiveCooldown)
        {
            Debug.Log("[BOSS] Daño ignorado por cooldown.");
            return;
        }

        lastDamageTime = Time.time;

        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * incomingDamageMultiplier));
        float nextHealth = currentHealth - finalDamage;

        // Gate Fase 1 → Fase 2
        if (phaseGateEnabled &&
            currentPhase == BossPhase.Phase1 &&
            nextHealth <= maxHealth * phase2HealthGate)
        {
            currentHealth = maxHealth * phase2HealthGate;

            Debug.Log($"[BOSS] RECIBIÓ DAÑO REAL: {finalDamage}. Gate Fase 2 activado.");

            StartFlash(damageColor, realDamageFlashDuration);
            StepBackFromDamage();

            if (!isTransitioning)
                StartCoroutine(PhaseTransition(BossPhase.Phase2));

            return;
        }

        // Gate Fase 2 → Fase 3
        if (phaseGateEnabled &&
            currentPhase == BossPhase.Phase2 &&
            nextHealth <= maxHealth * phase3HealthGate)
        {
            currentHealth = maxHealth * phase3HealthGate;

            Debug.Log($"[BOSS] RECIBIÓ DAÑO REAL: {finalDamage}. Gate Fase 3 activado.");

            StartFlash(damageColor, realDamageFlashDuration);
            StepBackFromDamage();

            if (!isTransitioning)
                StartCoroutine(PhaseTransition(BossPhase.Phase3));

            return;
        }

        currentHealth = Mathf.Clamp(nextHealth, 0f, maxHealth);

        Debug.Log($"[BOSS] RECIBIÓ DAÑO REAL: {finalDamage} | HP: {currentHealth}/{maxHealth}");

        // Feedback rojo + pasos hacia atrás.
        StartFlash(damageColor, realDamageFlashDuration);
        StepBackFromDamage();

        if (currentHealth <= 0f)
        {
            if (currentPhase == BossPhase.Phase3)
            {
                Die();
            }
            else
            {
                currentHealth = 1f;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  FASES
    // ═══════════════════════════════════════════════════════════════════════════
    void CheckPhaseTransition()
    {
        if (isTransitioning) return;

        float ratio = currentHealth / maxHealth;

        if (ratio <= phase3HealthGate && currentPhase != BossPhase.Phase3)
        {
            StartCoroutine(PhaseTransition(BossPhase.Phase3));
            return;
        }

        if (ratio <= phase2HealthGate && currentPhase == BossPhase.Phase1)
        {
            StartCoroutine(PhaseTransition(BossPhase.Phase2));
            return;
        }

        if (fightTimer >= phase3ForceTime && currentPhase != BossPhase.Phase3)
        {
            StartCoroutine(PhaseTransition(BossPhase.Phase3));
            return;
        }

        if (fightTimer >= phase2ForceTime && currentPhase == BossPhase.Phase1)
        {
            StartCoroutine(PhaseTransition(BossPhase.Phase2));
            return;
        }
    }

    IEnumerator PhaseTransition(BossPhase newPhase)
    {
        isTransitioning = true;

        Debug.Log($"[BOSS] TRANSICIÓN → {newPhase}");

        // Durante transición también es invulnerable.
        ForceShieldForSeconds(2.5f);

        for (int i = 0; i < 4; i++)
        {
            SetBossVisualColor(phaseTransitionColor, true);
            yield return new WaitForSeconds(0.12f);

            if (IsInvulnerable())
                SetBossVisualColor(invulnerableColor, true);
            else
                RestoreBaseVisualColor();

            yield return new WaitForSeconds(0.12f);
        }

        currentPhase = newPhase;

        ApplyPhaseSettings(newPhase);

        // Cada cambio de fase invoca minions y vuelve a activar escudo.
        SpawnMinionWaveWithShield();

        isTransitioning = false;

        Debug.Log($"[BOSS] Fase activa: {newPhase}");
    }

    void ApplyPhaseSettings(BossPhase phase)
    {
        switch (phase)
        {
            case BossPhase.Phase1:
                attackCooldown = p1AttackCooldown;
                spawnCooldown = p1SpawnCooldown;
                minionsPerWave = p1MinionsPerWave;

                approachSpeed = 1.45f;
                retreatSpeed = 1.85f;
                strafeSpeed = 1.15f;
                rotateSpeed = 2.2f;
                break;

            case BossPhase.Phase2:
                attackCooldown = p2AttackCooldown;
                spawnCooldown = p2SpawnCooldown;
                minionsPerWave = p2MinionsPerWave;

                approachSpeed = 1.65f;
                retreatSpeed = 2.05f;
                strafeSpeed = 1.25f;
                rotateSpeed = 2.4f;
                break;

            case BossPhase.Phase3:
                attackCooldown = p3AttackCooldown;
                spawnCooldown = p3SpawnCooldown;
                minionsPerWave = p3MinionsPerWave;

                approachSpeed = 1.85f;
                retreatSpeed = 2.25f;
                strafeSpeed = 1.35f;
                rotateSpeed = 2.6f;
                break;
        }
    }

    float GetAggressionMultiplier()
    {
        if (maxHealth <= 0f)
            return 1f;

        float hpRatio = currentHealth / maxHealth;
        float baseAggr = Mathf.Lerp(maxAggressionMult, 1f, hpRatio);

        switch (currentPhase)
        {
            case BossPhase.Phase2:
                return baseAggr * 1.15f;

            case BossPhase.Phase3:
                return baseAggr * 1.25f;

            default:
                return baseAggr;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  VISUAL ROBUSTO
    // ═══════════════════════════════════════════════════════════════════════════
    void CacheBossRenderers()
    {
        bossRenderers = GetComponentsInChildren<Renderer>(true);
        propertyBlock = new MaterialPropertyBlock();

        if (bossRenderers == null || bossRenderers.Length == 0)
        {
            Debug.LogWarning("[BOSS] No se encontraron Renderers.");
            return;
        }

        originalColors = new Color[bossRenderers.Length];

        for (int i = 0; i < bossRenderers.Length; i++)
        {
            Renderer r = bossRenderers[i];

            if (r == null || r.sharedMaterial == null)
            {
                originalColors[i] = normalFallbackColor;
                continue;
            }

            Material mat = r.sharedMaterial;

            if (mat.HasProperty(BaseColorID))
                originalColors[i] = mat.GetColor(BaseColorID);
            else if (mat.HasProperty(ColorID))
                originalColors[i] = mat.GetColor(ColorID);
            else
                originalColors[i] = normalFallbackColor;
        }
    }

    void ApplyShieldVisualState(bool force)
    {
        bool invulnerable = IsInvulnerable();

        if (!force && invulnerable == lastInvulnerableState)
            return;

        lastInvulnerableState = invulnerable;

        if (flashRoutine != null)
            return;

        if (invulnerable)
            SetBossVisualColor(invulnerableColor, true);
        else
            RestoreBaseVisualColor();
    }

    void SetBossVisualColor(Color color, bool useEmission)
    {
        if (bossRenderers == null || bossRenderers.Length == 0)
            CacheBossRenderers();

        if (bossRenderers == null || propertyBlock == null)
            return;

        Color emissionColor = color * emissionIntensity;

        for (int i = 0; i < bossRenderers.Length; i++)
        {
            Renderer r = bossRenderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor(BaseColorID, color);
            propertyBlock.SetColor(ColorID, color);

            if (useEmission)
                propertyBlock.SetColor(EmissionColorID, emissionColor);
            else
                propertyBlock.SetColor(EmissionColorID, Color.black);

            r.SetPropertyBlock(propertyBlock);
        }
    }

    void RestoreBaseVisualColor()
    {
        if (bossRenderers == null || originalColors == null || propertyBlock == null)
            return;

        for (int i = 0; i < bossRenderers.Length; i++)
        {
            Renderer r = bossRenderers[i];
            if (r == null) continue;

            Color baseColor = i < originalColors.Length ? originalColors[i] : normalFallbackColor;

            r.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor(BaseColorID, baseColor);
            propertyBlock.SetColor(ColorID, baseColor);
            propertyBlock.SetColor(EmissionColorID, Color.black);

            r.SetPropertyBlock(propertyBlock);
        }
    }

    void StartFlash(Color color, float duration)
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine(color, duration));
    }

    IEnumerator FlashRoutine(Color color, float duration)
    {
        SetBossVisualColor(color, true);

        yield return new WaitForSeconds(duration);

        flashRoutine = null;

        if (IsInvulnerable())
            SetBossVisualColor(invulnerableColor, true);
        else
            RestoreBaseVisualColor();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  MUERTE
    // ═══════════════════════════════════════════════════════════════════════════
    void Die()
    {
        isActive = false;

        Debug.Log("[BOSS] Derrotado.");

        if (arena != null)
            arena.EndBossFight();

        Destroy(gameObject, 2.5f);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  GIZMOS
    // ═══════════════════════════════════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, orbitDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}