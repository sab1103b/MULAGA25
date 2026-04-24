using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 500f;
    public float currentHealth;

    [Header("Locomotion")]
    public float orbitDistance = 6f;
    public float minDistance = 3f;
    public float maxDistance = 12f;
    public float moveSpeed = 2.5f;
    public float rotateSpeed = 60f;
    public float strafeSpeed = 1.5f;

    private float orbitAngle = 0f;

    [Header("Area Attack")]
    public GameObject dangerZonePrefab;
    public GameObject projectilePrefab;
    public Transform attackSpawnPoint;
    public float warningDuration = 2.5f;
    public float attackCooldown = 8f;

    private float attackTimer = 5f;
    private bool isAttacking = false;

    [Header("Minions")]
    public MinionSpawner minionSpawner;
    public float spawnCooldown = 15f;
    public int minionsPerWave = 3;

    private float spawnTimer = 10f;

    [Header("Aggression")]
    [Range(1f, 2f)]
    public float maxAggressionMult = 1.8f;

    [Header("References")]
    public BossArena arena;

    private Transform player;
    private bool isActive = false;
    private Rigidbody rb;

    void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation |
                             RigidbodyConstraints.FreezePositionY;
        }

        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("BossController: no encontró GameObject con tag 'Player'.");
    }

    public void ActivateBoss()
    {
        if (player == null)
            FindPlayer();

        isActive = true;
        attackTimer = 3f;
        spawnTimer = 8f;

        Debug.Log("Boss activado.");
    }

    void Update()
    {
        if (!isActive || player == null) return;

        float aggr = GetAggressionMultiplier();

        HandleLocomotion(aggr);
        HandleAreaAttack(aggr);
        HandleMinionSpawn();
    }

    void HandleLocomotion(float aggrMult)
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;

        if (dist <= 0.01f) return;

        Vector3 dirToPlayer = toPlayer.normalized;
        Vector3 desiredMove = Vector3.zero;

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

            float distError = dist - orbitDistance;
            Vector3 radial = dirToPlayer * distError * 0.8f;

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

    void HandleAreaAttack(float aggr)
    {
        if (isAttacking) return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            StartCoroutine(AreaAttack());
            attackTimer = attackCooldown / aggr;
        }
    }

    void HandleMinionSpawn()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            if (minionSpawner != null)
            {
                minionSpawner.SpawnWave(minionsPerWave);
                Debug.Log($"Boss spawneó {minionsPerWave} minions.");
            }
            else
            {
                Debug.LogError("BossController: MinionSpawner no asignado.");
            }

            spawnTimer = spawnCooldown;
        }
    }

    IEnumerator AreaAttack()
    {
        isAttacking = true;

        Vector3 warningPos = player.position;
        warningPos.y = 0.05f;

        GameObject warning = null;

        if (dangerZonePrefab != null)
        {
            warning = Instantiate(dangerZonePrefab, warningPos, Quaternion.identity);

            DangerZoneIndicator indicator = warning.GetComponent<DangerZoneIndicator>();

            if (indicator != null)
                indicator.ShowWarning(warningDuration);
        }
        else
        {
            Debug.LogError("BossController: DangerZonePrefab no asignado.");
        }

        yield return new WaitForSeconds(warningDuration);

        if (projectilePrefab == null)
        {
            Debug.LogError("BossController: ProjectilePrefab no asignado.");
        }
        else if (attackSpawnPoint == null)
        {
            Debug.LogError("BossController: AttackSpawnPoint no asignado.");
        }
        else if (player == null)
        {
            Debug.LogError("BossController: Player no asignado.");
        }
        else
        {
            Vector3 targetPos = player.position;
            targetPos.y = attackSpawnPoint.position.y;

            Vector3 spawnPos = attackSpawnPoint.position;

            Vector3 dirToTarget = (targetPos - spawnPos).normalized;

            if (dirToTarget != Vector3.zero)
            {
                spawnPos += dirToTarget * 1.2f;
            }

            GameObject proj = Instantiate(
                projectilePrefab,
                spawnPos,
                Quaternion.identity
            );

            BossProjectile bossProjectile = proj.GetComponent<BossProjectile>();

            if (bossProjectile != null)
            {
                bossProjectile.Initialize(targetPos);
                Debug.Log("Boss lanzó proyectil hacia: " + targetPos);
            }
            else
            {
                Debug.LogError("BossController: el projectilePrefab no tiene BossProjectile.");
            }
        }

        if (warning != null)
            Destroy(warning);

        isAttacking = false;
    }

    float GetAggressionMultiplier()
    {
        if (maxHealth <= 0f) return 1f;

        float hpRatio = currentHealth / maxHealth;
        return Mathf.Lerp(maxAggressionMult, 1f, hpRatio);
    }

    public void TakeDamage(int damage)
    {
        if (!isActive) return;

        currentHealth -= damage;

        if (currentHealth < 0f)
            currentHealth = 0f;

        Debug.Log($"Boss recibió {damage} daño. HP: {currentHealth}/{maxHealth}");

        StartCoroutine(HitFlash());

        if (currentHealth <= 0f)
            Die();
    }

    IEnumerator HitFlash()
    {
        Renderer rend = GetComponentInChildren<Renderer>();

        if (rend == null) yield break;

        Color original = rend.material.color;
        rend.material.color = Color.red;

        yield return new WaitForSeconds(0.12f);

        if (rend != null)
            rend.material.color = original;
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