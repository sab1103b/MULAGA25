using UnityEngine;

public class PATRONES : MonoBehaviour
{
    public enum MovementPattern
    {
        Oscilante,
        PicadaCurva,
        Cazador,
        ZigZag,
        Rodeo
    }

    // ─────────────────────────────────────────────
    // REFERENCIAS
    // ─────────────────────────────────────────────

    [Header("Player Reference (Assign XR Origin Here)")]
    public Transform player;
    private Transform playerHead;

    // ─────────────────────────────────────────────
    // ANIMATOR
    // El Animator está en el hijo (Enemigo 2), por eso GetComponentInChildren
    // ─────────────────────────────────────────────

    private Animator animator;

    // Nombres EXACTOS de los parámetros que ya tienes en tu Animator:
    private const string PARAM_ATTACK = "Attack";   // Trigger → ataque_salto
    private const string PARAM_GOLPE  = "Golpe";    // Trigger → golpe

    // ─────────────────────────────────────────────
    // DETECCIÓN DE SUELO
    // ─────────────────────────────────────────────

    [Header("Ground Detection")]
    public float groundCheckDistance = 20f;
    public float groundOffset = 0.85f;
    public float heightSmooth = 8f;
    public LayerMask groundLayer;

    // ─────────────────────────────────────────────
    // EVITAR OBSTÁCULOS
    // ─────────────────────────────────────────────

    [Header("Obstacle Avoidance")]
    public float obstacleCheckDistance = 1.5f;
    public float obstacleRadius = 0.5f;
    public LayerMask obstacleLayer;

    // ─────────────────────────────────────────────
    // PATRÓN
    // ─────────────────────────────────────────────

    [Header("Pattern")]
    public MovementPattern pattern;

    // ─────────────────────────────────────────────
    // CONFIGURACIÓN GENERAL
    // ─────────────────────────────────────────────

    [Header("General Settings")]
    public float forwardSpeed = 4f;
    public float amplitude = 2f;
    public float frequency = 2f;
    public float zigZagInterval = 1.5f;
    public float orbitDistance = 4f;
    public float orbitSpeed = 60f;

    // ─────────────────────────────────────────────
    // ATAQUE MELEE
    // ─────────────────────────────────────────────

    [Header("Melee Attack")]
    [Tooltip("Distancia para activar golpe cuerpo a cuerpo")]
    public float meleeRange = 1.5f;
    [Tooltip("Distancia para activar ataque_salto (salto hacia el jugador)")]
    public float jumpAttackRange = 5f;
    [Tooltip("Segundos entre ataques")]
    public float meleeCooldown = 1.5f;

    private float meleeTimer = 0f;

    // ─────────────────────────────────────────────
    // ESTADOS INTERNOS
    // ─────────────────────────────────────────────

    // ZIG ZAG
    private float timer;
    private int zigZagDirection = 1;

    // PICADA CURVA
    private bool diveStarted = false;
    private float diveSide = 0f;
    private bool turning = false;
    private float turnAngle = 0f;
    private float maxTurn = 90f;
    public float turnSpeed = 120f;

    // CAZADOR
    private bool isJumping = false;
    private bool isRetreating = false;
    private Vector3 jumpStart;
    private Vector3 jumpTarget;
    private Vector3 retreatDirection;
    private float jumpTimer = 0f;

    public float jumpDistance = 10f;
    public float jumpHeight = 3f;
    public float jumpDuration = 1f;
    public float retreatDistance = 25f;

    // ─────────────────────────────────────────────
    // AUDIO
    // ─────────────────────────────────────────────

    [Header("Audio")]
    public AudioClip spawnSFX;
    public AudioClip deathSFX;
    private AudioSource audioSource;

    // ─────────────────────────────────────────────
    // ANTI OVERLAP (PARA EVITAR QUE SE APILEN VARIOS ENEMIGOS)
    // ─────────────────────────────────────────────
    [Header("Anti-Overlap")]
    public float separationDistance = 0.5f;
    public LayerMask enemyLayer;

    // ─────────────────────────────────────────────
    // UNITY
    // ─────────────────────────────────────────────

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // CLAVE: el Animator está en el hijo (Enemigo 2), no en el raíz
        animator = GetComponentInChildren<Animator>();

        if (audioSource != null)
        {
            audioSource.loop        = false;
            audioSource.playOnAwake = false;
        }

        if (animator == null)
            Debug.LogWarning("[PATRONES] No se encontró Animator en hijos de " + gameObject.name, this);
    }

    void Start()
    {
        if (player != null)
            playerHead = player.GetComponentInChildren<Camera>()?.transform;
    }

    void OnEnable()
    {
        PlaySpawnSFX();

        isJumping    = false;
        isRetreating = false;
        meleeTimer   = 0f;
        timer        = 0f;
        diveStarted  = false;
        turning      = false;
        turnAngle    = 0f;
    }

    void Update()
    {
        if (playerHead == null) return;

        timer      += Time.deltaTime;
        meleeTimer += Time.deltaTime;

        Vector3 previousPosition = transform.position;
        Vector3 targetPosition   = transform.position;

        switch (pattern)
        {
            case MovementPattern.Oscilante:   targetPosition = Oscilante();   break;
            case MovementPattern.PicadaCurva: targetPosition = PicadaCurva(); break;
            case MovementPattern.Cazador:     targetPosition = Cazador();     break;
            case MovementPattern.ZigZag:      targetPosition = ZigZag();      break;
            case MovementPattern.Rodeo:       targetPosition = Rodeo();       break;
        }

        targetPosition = AvoidObstacles(targetPosition);
        targetPosition = ApplySeparation(targetPosition);

        if (!isJumping)
            targetPosition = AdjustToGround(targetPosition);

        transform.position = targetPosition;

        // ── Rotación ──────────────────────────────────────────────
        Vector3 moveDir = transform.position - previousPosition;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            moveDir.y    = 0f;
            Quaternion targetRot = Quaternion.LookRotation(moveDir) * Quaternion.Euler(0, 180f, 0);
            transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, 6f * Time.deltaTime);
        }

        // ── Ataques ───────────────────────────────────────────────
        TryAttack();
    }

    // ─────────────────────────────────────────────
    // ATAQUES
    // ─────────────────────────────────────────────

    void TryAttack()
    {
        if (animator == null) return;
        if (meleeTimer < meleeCooldown) return;
        if (isJumping || isRetreating) return;

        float dist = Vector3.Distance(transform.position, playerHead.position);

        // Muy cerca → Golpe (cuerpo a cuerpo directo)
        if (dist <= meleeRange)
        {
            meleeTimer = 0f;
            animator.SetTrigger(PARAM_GOLPE);
        }
        // Distancia media → ataque_salto (se lanza hacia el jugador)
        else if (dist <= jumpAttackRange)
        {
            meleeTimer = 0f;
            animator.SetTrigger(PARAM_ATTACK);
        }
    }

    // ─────────────────────────────────────────────
    // PATRONES DE MOVIMIENTO
    // ─────────────────────────────────────────────

    Vector3 Oscilante()
    {
        Vector3 forwardDir = (playerHead.position - transform.position).normalized;
        forwardDir.y      *= 0.3f;
        Vector3 sideDir    = Vector3.Cross(forwardDir, Vector3.up).normalized;
        float oscillation  = Mathf.Sin(timer * frequency) * amplitude;
        Vector3 move       = forwardDir * forwardSpeed + sideDir * oscillation;
        return transform.position + move * Time.deltaTime;
    }

    Vector3 PicadaCurva()
    {
        float attackDistance = 5f;
        Vector3 toPlayer     = playerHead.position - transform.position;
        float distance       = toPlayer.magnitude;
        Vector3 forwardDir   = toPlayer.normalized;
        forwardDir.y        *= 0.3f;

        if (!diveStarted && distance > attackDistance)
            return transform.position + forwardDir * forwardSpeed * Time.deltaTime;

        if (!diveStarted)
        {
            diveStarted = true;
            turning     = true;
            diveSide    = Random.value < 0.5f ? -1f : 1f;
        }

        if (turning)
        {
            float step         = turnSpeed * Time.deltaTime;
            turnAngle         += step;
            Vector3 offset     = transform.position - playerHead.position;
            offset             = Quaternion.Euler(0, step * diveSide, 0) * offset;
            transform.position = playerHead.position + offset;

            if (turnAngle >= maxTurn)
                turning = false;

            return transform.position;
        }

        return transform.position + forwardDir * forwardSpeed * Time.deltaTime;
    }

    Vector3 Cazador()
    {
        float distance = Vector3.Distance(transform.position, playerHead.position);

        if (isJumping)
        {
            jumpTimer  += Time.deltaTime;
            float t     = jumpTimer / jumpDuration;
            Vector3 pos = Vector3.Lerp(jumpStart, jumpTarget, t);
            pos.y      += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            if (t >= 1f)
            {
                isJumping          = false;
                isRetreating       = true;
                retreatDirection   = (transform.position - playerHead.position).normalized;
                retreatDirection.y = 0f;
            }

            return pos;
        }

        if (isRetreating)
        {
            if (Vector3.Distance(transform.position, playerHead.position) >= retreatDistance)
                isRetreating = false;

            return transform.position + retreatDirection * forwardSpeed * Time.deltaTime;
        }

        Vector3 direction  = (playerHead.position - transform.position).normalized;
        direction.y       *= 0.4f;

        if (distance <= jumpDistance)
        {
            isJumping  = true;
            jumpTimer  = 0f;
            jumpStart  = transform.position;
            jumpTarget = playerHead.position;
        }

        return transform.position + direction * forwardSpeed * Time.deltaTime;
    }

    Vector3 ZigZag()
    {
        if (timer >= zigZagInterval)
        {
            zigZagDirection *= -1;
            timer            = 0f;
        }

        Vector3 forwardDir = (playerHead.position - transform.position).normalized;
        forwardDir.y      *= 0.3f;
        Vector3 sideDir    = Vector3.Cross(forwardDir, Vector3.up).normalized;
        Vector3 move       = forwardDir * forwardSpeed + sideDir * zigZagDirection * amplitude;
        return transform.position + move * Time.deltaTime;
    }

    Vector3 Rodeo()
    {
        float distance = Vector3.Distance(transform.position, playerHead.position);

        if (distance > orbitDistance)
        {
            Vector3 direction  = (playerHead.position - transform.position).normalized;
            direction.y       *= 0.4f;
            return transform.position + direction * forwardSpeed * Time.deltaTime;
        }
        else
        {
            Vector3 offset = transform.position - playerHead.position;
            offset         = Quaternion.Euler(0, orbitSpeed * Time.deltaTime, 0) * offset;
            return playerHead.position + offset;
        }
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
            float desiredY   = hit.point.y + groundOffset;
            targetPosition.y = Mathf.Lerp(transform.position.y, desiredY, heightSmooth * Time.deltaTime);
        }

        return targetPosition;
    }

    Vector3 AvoidObstacles(Vector3 targetPosition)
    {
        Vector3 currentPos = transform.position;
        Vector3 moveDir    = targetPosition - currentPos;
        float moveDistance = moveDir.magnitude;

        if (moveDistance < 0.001f) return targetPosition;

        moveDir.Normalize();
        RaycastHit hit;

        if (Physics.SphereCast(currentPos, obstacleRadius, moveDir, out hit, obstacleCheckDistance, obstacleLayer))
        {
            Vector3 slideDir = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;

            if (slideDir.sqrMagnitude < 0.01f)
                slideDir = Vector3.Cross(hit.normal, Vector3.up).normalized;

            return currentPos + slideDir * moveDistance;
        }

        return targetPosition;
    }

    // ─────────────────────────────────────────────
    // MUERTE
    // ─────────────────────────────────────────────

    public void TriggerDeath()
    {
        // Si más adelante agregas un trigger "Die" en el Animator,
        // descomenta esta línea:
        // if (animator != null) animator.SetTrigger("Die");
    }

    // ─────────────────────────────────────────────
    // AUDIO
    // ─────────────────────────────────────────────

    void PlaySpawnSFX()
    {
        if (audioSource != null && spawnSFX != null)
            audioSource.PlayOneShot(spawnSFX);
    }

    public void PlayDeathSFX()
    {
        if (deathSFX != null)
            AudioSource.PlayClipAtPoint(deathSFX, transform.position);
    }

    Vector3 ApplySeparation(Vector3 currentPosition)
    {
        Collider[] nearby = Physics.OverlapSphere(currentPosition, separationDistance, enemyLayer);

        Vector3 separation = Vector3.zero;
        int count = 0;

        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject) continue;

            Vector3 dir = currentPosition - col.transform.position;
            float dist = dir.magnitude;

            if (dist < 0.0001f) continue;

            separation += dir.normalized / dist;
            count++;
        }

        if (count == 0) return currentPosition;

        separation /= count;
        separation = separation.normalized * separationDistance;

        return currentPosition + separation;
    }
}