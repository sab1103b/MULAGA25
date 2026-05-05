using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class MinionAI : MonoBehaviour, IDamageable
{
    // ── Contador global de minions vivos ─────────────────────────────────────
    // BossController lo lee para saber si el escudo está activo.
    public static int ActiveMinions = 0;

    // ── Stats base ────────────────────────────────────────────────────────────
    [Header("Stats")]
    public float maxHealth    = 60f;
    public float currentHealth;
    public int   damage       = 15;
    public float attackRange  = 1.6f;
    public float attackCooldown = 1.4f;

    [Header("Behaviour")]
    public float detectionRange = 25f;
    public float stopDistance   = 1.2f;

    [Header("Speed")]
    public float walkSpeed   = 2.5f;
    public float chargeSpeed = 5f;

    [Header("Arena")]
    public BossArena arena;

    // ── Modo agresivo (activado por BossController en fases 2 y 3) ───────────
    private bool isAggressive = false;

    private NavMeshAgent agent;
    private Transform    player;
    private float        attackTimer = 0f;
    private bool         isDead      = false;

    enum MinionState { Idle, Chase, Attack, Dead }
    private MinionState state = MinionState.Idle;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake() => agent = GetComponent<NavMeshAgent>();

    void OnEnable()
    {
        // Incrementar contador cuando el minion se activa (compatible con pooling)
        ActiveMinions++;

        isDead      = false;
        currentHealth = maxHealth;
        state       = MinionState.Idle;
        attackTimer = 0f;
        isAggressive = false;

        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.enabled = true;

            // ⚠️ Forzar al NavMesh antes de usar el agente
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);

                agent.stoppingDistance = stopDistance;
                agent.speed = walkSpeed;
                agent.angularSpeed = 360f;
                agent.acceleration = 12f;

                agent.isStopped = false; // ← ahora sí es seguro
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: no encontró NavMesh al activarse.");
            }
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void OnDisable()
    {
        // Decrementar contador cuando el objeto se desactiva o destruye (pooling-safe)
        ActiveMinions = Mathf.Max(0, ActiveMinions - 1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void SetPlayer(Transform newPlayer) => player = newPlayer;

    /// <summary>
    /// Llamado por MinionSpawner según la fase del boss.
    /// Boost: +velocidad, +daño, -cooldown de ataque.
    /// </summary>
    public void SetAggressiveMode(bool aggressive)
    {
        isAggressive = aggressive;

        if (agent == null) return;

        if (aggressive)
        {
            agent.speed         = walkSpeed  * 1.6f;
            agent.acceleration  = 20f;
        }
        else
        {
            agent.speed         = walkSpeed;
            agent.acceleration  = 12f;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (isDead || player == null || agent == null || !agent.enabled) return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name}: no está sobre el NavMesh.");
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case MinionState.Idle:
                if (distToPlayer <= detectionRange) state = MinionState.Chase;
                break;
            case MinionState.Chase:
                ChasePlayer(distToPlayer);
                break;
            case MinionState.Attack:
                AttackPlayer(distToPlayer);
                break;
        }

        attackTimer -= Time.deltaTime;
    }

    void ChasePlayer(float dist)
    {
        agent.isStopped = false;
        agent.speed     = dist < 4f || isAggressive ? chargeSpeed * (isAggressive ? 1.5f : 1f) : walkSpeed;
        agent.SetDestination(player.position);

        if (dist <= attackRange) state = MinionState.Attack;
    }

    void AttackPlayer(float dist)
    {
        agent.isStopped = false;
        agent.speed     = chargeSpeed * (isAggressive ? 1.5f : 1f);
        agent.SetDestination(player.position);

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 15f);
        }

        if (attackTimer <= 0f)
        {
            PerformAttack();
            // Modo agresivo: cooldown reducido 40%
            attackTimer = attackCooldown * (isAggressive ? 0.6f : 1f);
        }

        if (dist > attackRange * 1.5f) state = MinionState.Chase;
    }

    void PerformAttack()
    {
        int finalDamage = isAggressive ? Mathf.RoundToInt(damage * 1.5f) : damage;

        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * 1f, attackRange * 0.8f);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(finalDamage);
                Debug.Log($"Minion golpeó jugador ({finalDamage} dmg, agresivo:{isAggressive}).");
                break;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        StartCoroutine(HitReact());
        if (currentHealth <= 0f) Die();
    }

    IEnumerator HitReact()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
        yield return new WaitForSeconds(0.15f);
        if (!isDead && agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = false;
    }

    void Die()
    {
        if (isDead) return;   // Guardia extra para evitar doble decremento
        isDead = true;
        state  = MinionState.Dead;

        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
        if (agent != null) agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 2.5f);
        // OnDisable se llamará al destruir → decrementa ActiveMinions
    }

    // ─────────────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}