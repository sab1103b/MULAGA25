using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class MinionAI : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 60f;
    public float currentHealth;
    public int damage = 15;
    public float attackRange = 1.6f;
    public float attackCooldown = 1.4f;

    [Header("Behaviour")]
    public float detectionRange = 25f;
    public float stopDistance = 1.2f;

    [Header("Speed")]
    public float walkSpeed = 2.5f;
    public float chargeSpeed = 5f;

    [Header("Arena")]
    public BossArena arena;

    private NavMeshAgent agent;
    private Transform player;
    private float attackTimer = 0f;
    private bool isDead = false;

    enum MinionState { Idle, Chase, Attack, Dead }
    private MinionState state = MinionState.Idle;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        isDead = false;
        currentHealth = maxHealth;
        state = MinionState.Idle;
        attackTimer = 0f;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.stoppingDistance = stopDistance;
            agent.speed = walkSpeed;
            agent.angularSpeed = 360f;
            agent.acceleration = 12f;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    void Update()
    {
        if (isDead || player == null || agent == null || !agent.enabled)
            return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name}: no está sobre el NavMesh.");
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case MinionState.Idle:
                if (distToPlayer <= detectionRange)
                    state = MinionState.Chase;
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
        agent.speed = dist < 4f ? chargeSpeed : walkSpeed;
        agent.SetDestination(player.position);

        if (dist <= attackRange)
            state = MinionState.Attack;
    }

    void AttackPlayer(float dist)
    {
        agent.isStopped = false;
        agent.speed = chargeSpeed;
        agent.SetDestination(player.position);

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 15f
            );
        }

        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }

        if (dist > attackRange * 1.5f)
            state = MinionState.Chase;
    }

    void PerformAttack()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * 1f,
            attackRange * 0.8f
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(damage);
                Debug.Log("Minion golpeó al jugador.");
                break;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        StartCoroutine(HitReact());

        if (currentHealth <= 0f)
            Die();
    }

    IEnumerator HitReact()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = true;

        yield return new WaitForSeconds(0.15f);

        if (!isDead && agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    void Die()
    {
        isDead = true;
        state = MinionState.Dead;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = true;

        if (agent != null)
            agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Destroy(gameObject, 2.5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}