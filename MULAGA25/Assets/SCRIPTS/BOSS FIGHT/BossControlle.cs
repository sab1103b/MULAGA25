using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour, IDamageable
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  FASE DEL BOSS
    // ═══════════════════════════════════════════════════════════════════════════
    public enum BossPhase { Phase1, Phase2, Phase3 }

    private BossPhase currentPhase = BossPhase.Phase1;
    private bool isTransitioning = false;

    // ── Stats ─────────────────────────────────────────────────────────────────
    [Header("Stats")]
    public float maxHealth = 500f;
    public float currentHealth;

    // ── Locomoción ────────────────────────────────────────────────────────────
    [Header("Locomotion")]
    public float orbitDistance = 6f;
    public float minDistance = 3f;
    public float maxDistance = 12f;
    public float moveSpeed = 2.5f;
    public float rotateSpeed = 60f;
    public float strafeSpeed = 1.5f;

    private float orbitAngle = 0f;

    // ── Ataque base ───────────────────────────────────────────────────────────
    [Header("Attack — Shared")]
    public GameObject dangerZonePrefab;
    public GameObject projectilePrefab;
    public Transform attackSpawnPoint;
    public float warningDuration = 2.5f;

    private bool isAttacking = false;
    private float attackTimer = 5f;

    // ── Fase 1 ────────────────────────────────────────────────────────────────
    [Header("Phase 1  (HP > 66 %)")]
    public float p1AttackCooldown = 8f;
    public float p1SpawnCooldown = 15f;
    public int p1MinionsPerWave = 3;

    // ── Fase 2 ────────────────────────────────────────────────────────────────
    [Header("Phase 2  (HP 33 – 66 %)")]
    public float p2AttackCooldown = 5.5f;
    public float p2SpawnCooldown = 10f;
    public int p2MinionsPerWave = 4;
    public int p2ZoneCount = 2;
    public float p2ZoneSpread = 2.8f;

    // ── Fase 3 ────────────────────────────────────────────────────────────────
    [Header("Phase 3  (HP < 33 %)")]
    public float p3AttackCooldown = 3.5f;
    public float p3SpawnCooldown = 8f;
    public int p3MinionsPerWave = 5;

    [Tooltip("Cantidad de proyectiles del disparo en abanico. Para triple disparo usa 3.")]
    public int p3FanCount = 3;

    [Tooltip("Qué tan separados nacen los proyectiles alrededor del boss.")]
    public float p3SpawnRingRadius = 3.2f;

    [Tooltip("Qué tan separados caen los proyectiles alrededor del jugador.")]
    public float p3TargetSpread = 2.2f;

    public int p3ZoneCount = 3;

    // ── Minions ───────────────────────────────────────────────────────────────
    [Header("Minions")]
    public MinionSpawner minionSpawner;

    private float spawnCooldown = 15f;
    private int minionsPerWave = 3;
    private float attackCooldown = 8f;
    private float spawnTimer = 10f;

    // ── Agresividad ───────────────────────────────────────────────────────────
    [Header("Aggression")]
    [Range(1f, 2f)]
    public float maxAggressionMult = 1.8f;

    // ── Escudo de minions ─────────────────────────────────────────────────────
    [Header("Minion Shield")]
    [Tooltip("El boss es inmune mientras haya minions vivos.")]
    public bool minionShieldEnabled = true;
    public Color immuneFlashColor = new Color(0.2f, 0.8f, 1f);

    // ── Referencias ───────────────────────────────────────────────────────────
    [Header("References")]
    public BossArena arena;

    private Transform player;
    private bool isActive = false;
    private Rigidbody rb;

    private Color originalColor = Color.white;
    private bool originalColorCached = false;

    // ═══════════════════════════════════════════════════════════════════════════
    //  INICIO
    // ═══════════════════════════════════════════════════════════════════════════
    void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation |
                             RigidbodyConstraints.FreezePositionY;
        }

        ApplyPhaseSettings(BossPhase.Phase1);
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("BossController: no encontró ningún objeto con Tag 'Player'.");
        }
    }

    public void ActivateBoss()
    {
        if (player == null)
            FindPlayer();

        isActive = true;

        attackTimer = 3f;
        spawnTimer = 8f;

        Debug.Log("Boss activado — Fase 1.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  UPDATE
    // ═══════════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (!isActive || player == null || isTransitioning)
            return;

        float aggr = GetAggressionMultiplier();

        HandleLocomotion(aggr);
        HandleAreaAttack(aggr);
        HandleMinionSpawn();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  LOCOMOCIÓN
    // ═══════════════════════════════════════════════════════════════════════════
    void HandleLocomotion(float aggrMult)
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;

        if (dist <= 0.01f)
            return;

        Vector3 dirToPlayer = toPlayer.normalized;
        Vector3 desiredMove;

        if (dist > maxDistance)
        {
            desiredMove = dirToPlayer * moveSpeed * 1.5f;
        }
        else if (dist < minDistance)
        {
            desiredMove = -dirToPlayer * moveSpeed * 0.5f;
        }
        else
        {
            orbitAngle += rotateSpeed * aggrMult * Time.deltaTime;

            Vector3 right = Vector3.Cross(Vector3.up, dirToPlayer).normalized;
            float side = Mathf.Sin(orbitAngle * Mathf.Deg2Rad);

            Vector3 strafe = right * side * strafeSpeed;
            Vector3 radial = dirToPlayer * (dist - orbitDistance) * 0.8f;

            desiredMove = strafe + radial;
        }

        Vector3 nextPos = transform.position + desiredMove * Time.deltaTime;
        nextPos.y = transform.position.y;

        if (arena == null || arena.IsInsideArena(nextPos))
            transform.position = nextPos;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dirToPlayer),
            Time.deltaTime * 6f
        );
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  ATAQUES
    // ═══════════════════════════════════════════════════════════════════════════
    void HandleAreaAttack(float aggr)
    {
        if (isAttacking)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        switch (currentPhase)
        {
            case BossPhase.Phase1:
                StartCoroutine(SingleProjectileAttack());
                break;

            case BossPhase.Phase2:
                StartCoroutine(MultiZoneAttack(p2ZoneCount, p2ZoneSpread));
                break;

            case BossPhase.Phase3:
                if (Random.value > 0.5f)
                    StartCoroutine(FanAttack(p3FanCount));
                else
                    StartCoroutine(MultiZoneAttack(p3ZoneCount, p2ZoneSpread * 1.2f));
                break;
        }

        attackTimer = attackCooldown / aggr;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  FASE 1: un proyectil directo al jugador
    // ═══════════════════════════════════════════════════════════════════════════
    IEnumerator SingleProjectileAttack()
    {
        isAttacking = true;

        Vector3 target = player.position;
        target.y = 0.05f;

        GameObject warning = SpawnWarning(target, GetProjectileRadius());

        if (warning != null)
            warning.GetComponent<DangerZoneIndicator>()?.ShowWarning(warningDuration);

        FireProjectileFromTo(attackSpawnPoint.position, target, warningDuration, warning);

        yield return new WaitForSeconds(warningDuration);

        isAttacking = false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  FASE 2 / 3: zonas alrededor del jugador
    // ═══════════════════════════════════════════════════════════════════════════
    IEnumerator MultiZoneAttack(int zoneCount, float spread)
    {
        isAttacking = true;

        Vector3 center = player.position;
        center.y = 0.05f;

        float radius = GetProjectileRadius();

        for (int i = 0; i < zoneCount; i++)
        {
            Vector3 target;

            if (i == 0)
            {
                target = center;
            }
            else
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

            if (warning != null)
                warning.GetComponent<DangerZoneIndicator>()?.ShowWarning(warningDuration);

            Vector3 spawnPos = GetProjectileSpawnAroundBoss(i, zoneCount, p3SpawnRingRadius);

            FireProjectileFromTo(spawnPos, target, warningDuration, warning);
        }

        yield return new WaitForSeconds(warningDuration);

        isAttacking = false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  FASE 3: triple disparo
    //  Las bolas NACEN alrededor del boss y VAN hacia el jugador.
    // ═══════════════════════════════════════════════════════════════════════════
    IEnumerator FanAttack(int count)
    {
        isAttacking = true;

        if (attackSpawnPoint == null)
        {
            Debug.LogError("BossController: AttackSpawnPoint no asignado.");
            isAttacking = false;
            yield break;
        }

        if (count <= 0)
        {
            isAttacking = false;
            yield break;
        }

        Vector3 playerCenter = player.position;
        playerCenter.y = 0.05f;

        Vector3 bossToPlayer = player.position - transform.position;
        bossToPlayer.y = 0f;

        if (bossToPlayer.sqrMagnitude < 0.001f)
            bossToPlayer = transform.forward;

        bossToPlayer.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, bossToPlayer).normalized;

        float radius = GetProjectileRadius();

        for (int i = 0; i < count; i++)
        {
            // Posición de nacimiento: alrededor del boss
            Vector3 spawnPos = GetProjectileSpawnAroundBoss(i, count, p3SpawnRingRadius);

            // Posición de impacto: cerca del jugador
            float offsetIndex = i - (count - 1) * 0.5f;

            Vector3 target = playerCenter + right * offsetIndex * p3TargetSpread;
            target.y = 0.05f;

            GameObject warning = SpawnWarning(target, radius);

            if (warning != null)
                warning.GetComponent<DangerZoneIndicator>()?.ShowWarning(warningDuration);

            FireProjectileFromTo(spawnPos, target, warningDuration, warning);
        }

        yield return new WaitForSeconds(warningDuration);

        isAttacking = false;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  HELPERS DE ATAQUE
    // ═══════════════════════════════════════════════════════════════════════════

    Vector3 GetProjectileSpawnAroundBoss(int index, int count, float ringRadius)
    {
        if (count <= 1)
        {
            if (attackSpawnPoint != null)
                return attackSpawnPoint.position;

            return transform.position + transform.forward * ringRadius + Vector3.up * 2f;
        }

        Vector3 bossToPlayer = player.position - transform.position;
        bossToPlayer.y = 0f;

        if (bossToPlayer.sqrMagnitude < 0.001f)
            bossToPlayer = transform.forward;

        bossToPlayer.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, bossToPlayer).normalized;

        float offsetIndex = index - (count - 1) * 0.5f;

        Vector3 spawnPos =
            transform.position +
            bossToPlayer * ringRadius +
            right * offsetIndex * 1.8f +
            Vector3.up * 2.2f;

        return spawnPos;
    }

    GameObject SpawnWarning(Vector3 position, float radius)
    {
        if (dangerZonePrefab == null)
        {
            Debug.LogError("BossController: DangerZonePrefab no asignado.");
            return null;
        }

        GameObject warning = Instantiate(dangerZonePrefab, position, Quaternion.identity);

        DangerZoneIndicator indicator = warning.GetComponent<DangerZoneIndicator>();

        if (indicator != null)
            indicator.SetRadius(radius);

        return warning;
    }

    void FireProjectileFromTo(Vector3 spawnPos, Vector3 target, float travelTime, GameObject warning)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("BossController: ProjectilePrefab no asignado.");
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        BossProjectile bp = proj.GetComponent<BossProjectile>();

        if (bp != null)
        {
            bp.Initialize(target, travelTime, warning);
        }
        else
        {
            Debug.LogError("BossController: el projectilePrefab no tiene BossProjectile.");
        }
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
    //  SPAWN DE MINIONS
    // ═══════════════════════════════════════════════════════════════════════════
    void HandleMinionSpawn()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        if (minionSpawner != null)
        {
            bool aggressive = currentPhase != BossPhase.Phase1;

            minionSpawner.SpawnWave(minionsPerWave, aggressive);

            Debug.Log($"Boss spawneó {minionsPerWave} minions. Agresivos: {aggressive}");
        }
        else
        {
            Debug.LogError("BossController: MinionSpawner no asignado.");
        }

        spawnTimer = spawnCooldown;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  RECIBIR DAÑO
    // ═══════════════════════════════════════════════════════════════════════════
    public void TakeDamage(int damage)
    {
        if (!isActive)
            return;

        if (minionShieldEnabled && MinionAI.ActiveMinions > 0)
        {
            Debug.Log($"Boss inmune — minions vivos: {MinionAI.ActiveMinions}");
            StartCoroutine(ImmuneFlash());
            return;
        }

        currentHealth -= damage;

        if (currentHealth < 0f)
            currentHealth = 0f;

        Debug.Log($"Boss recibió {damage} daño. HP: {currentHealth}/{maxHealth}");

        StartCoroutine(HitFlash());

        CheckPhaseTransition();

        if (currentHealth <= 0f)
            Die();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  TRANSICIÓN DE FASES
    // ═══════════════════════════════════════════════════════════════════════════
    void CheckPhaseTransition()
    {
        float ratio = currentHealth / maxHealth;

        if (ratio <= 0.33f && currentPhase != BossPhase.Phase3)
        {
            StartCoroutine(PhaseTransition(BossPhase.Phase3));
        }
        else if (ratio <= 0.66f && currentPhase == BossPhase.Phase1)
        {
            StartCoroutine(PhaseTransition(BossPhase.Phase2));
        }
    }

    IEnumerator PhaseTransition(BossPhase newPhase)
    {
        isTransitioning = true;

        Debug.Log($"Boss entra en {newPhase}.");

        Renderer rend = GetComponentInChildren<Renderer>();

        CacheOriginalColor(rend);

        if (rend != null)
        {
            for (int i = 0; i < 4; i++)
            {
                rend.material.color = Color.white;
                yield return new WaitForSeconds(0.12f);

                rend.material.color = originalColor;
                yield return new WaitForSeconds(0.12f);
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        currentPhase = newPhase;

        ApplyPhaseSettings(newPhase);

        if (minionSpawner != null)
            minionSpawner.SpawnWave(minionsPerWave, newPhase != BossPhase.Phase1);

        isTransitioning = false;

        Debug.Log($"Fase {newPhase} activa. Cooldown: {attackCooldown}s. Minions: {minionsPerWave}");
    }

    void ApplyPhaseSettings(BossPhase phase)
    {
        switch (phase)
        {
            case BossPhase.Phase1:
                attackCooldown = p1AttackCooldown;
                spawnCooldown = p1SpawnCooldown;
                minionsPerWave = p1MinionsPerWave;
                break;

            case BossPhase.Phase2:
                attackCooldown = p2AttackCooldown;
                spawnCooldown = p2SpawnCooldown;
                minionsPerWave = p2MinionsPerWave;
                moveSpeed *= 1.2f;
                break;

            case BossPhase.Phase3:
                attackCooldown = p3AttackCooldown;
                spawnCooldown = p3SpawnCooldown;
                minionsPerWave = p3MinionsPerWave;
                moveSpeed *= 1.15f;
                strafeSpeed *= 1.3f;
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  FEEDBACK VISUAL
    // ═══════════════════════════════════════════════════════════════════════════
    void CacheOriginalColor(Renderer rend)
    {
        if (!originalColorCached && rend != null)
        {
            originalColor = rend.material.color;
            originalColorCached = true;
        }
    }

    IEnumerator HitFlash()
    {
        Renderer rend = GetComponentInChildren<Renderer>();

        CacheOriginalColor(rend);

        if (rend == null)
            yield break;

        rend.material.color = Color.red;

        yield return new WaitForSeconds(0.12f);

        if (rend != null)
            rend.material.color = originalColor;
    }

    IEnumerator ImmuneFlash()
    {
        Renderer rend = GetComponentInChildren<Renderer>();

        CacheOriginalColor(rend);

        if (rend == null)
            yield break;

        rend.material.color = immuneFlashColor;

        yield return new WaitForSeconds(0.18f);

        if (rend != null)
            rend.material.color = originalColor;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  AGRESIVIDAD
    // ═══════════════════════════════════════════════════════════════════════════
    float GetAggressionMultiplier()
    {
        if (maxHealth <= 0f)
            return 1f;

        return Mathf.Lerp(maxAggressionMult, 1f, currentHealth / maxHealth);
    }

    void Die()
    {
        isActive = false;

        Debug.Log("Boss derrotado.");

        if (arena != null)
            arena.EndBossFight();

        Destroy(gameObject, 2.5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, orbitDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}