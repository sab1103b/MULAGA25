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

    [Header("Player Reference (Assign XR Origin Here)")]
    public Transform player; 
    private Transform playerHead;

    [Header("Pattern")]
    public MovementPattern pattern;

    [Header("General Settings")]
    public float forwardSpeed = 4f;
    public float amplitude = 2f;
    public float frequency = 2f;
    public float smoothFactor = 5f;
    public float zigZagInterval = 1.5f;
    public float orbitDistance = 4f;
    public float orbitSpeed = 60f;


    
    [Header("Limits")]
    public float xLimit = 10f;
    public float yMaxLimit = 6f;
    public float groundLimit = 0.5f;   
    public float zMinLimit = -50f;
    public float zMaxLimit = 5f;
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
    public float jumpHeight = 6f;
    public float jumpDuration = 1f;
    public float retreatDistance = 25f;

    void Start()
    {
        if (player != null)
        {
            playerHead = player.GetComponentInChildren<Camera>()?.transform;
        }
    }

    void Update()
    {
        if (playerHead == null) return;

        timer += Time.deltaTime;

        Vector3 previousPosition = transform.position;
        Vector3 targetPosition = transform.position;

        switch (pattern)
        {
            case MovementPattern.Oscilante:
                targetPosition = Oscilante();
                break;

            case MovementPattern.PicadaCurva:
                targetPosition = PicadaCurva();
                break;

            case MovementPattern.Cazador:
                targetPosition = Cazador();
                break;

            case MovementPattern.ZigZag:
                targetPosition = ZigZag();
                break;

            case MovementPattern.Rodeo:
                targetPosition = Rodeo();
                break;
        }

        // APLICAR MOVIMIENTO
        transform.position = targetPosition;

        // ROTACIÓN HACIA MOVIMIENTO
        Vector3 moveDir = (transform.position - previousPosition);

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            moveDir.y = 0; // evita inclinación rara en VR

            Quaternion targetRot = Quaternion.LookRotation(moveDir) * Quaternion.Euler(0, 180f, 0);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                6f * Time.deltaTime
            );
        }

        ApplyLimits();
    }
    // PATRONES

    Vector3 Oscilante()
    {
        // Dirección base hacia el jugador
        Vector3 forwardDir = (playerHead.position - transform.position).normalized;
        forwardDir.y *= 0.3f; // suaviza vertical para VR

        // Dirección lateral perpendicular
        Vector3 sideDir = Vector3.Cross(forwardDir, Vector3.up).normalized;

        // Movimiento oscilante lateral
        float oscillation = Mathf.Sin(timer * frequency) * amplitude;

        Vector3 move =
            forwardDir * forwardSpeed +
            sideDir * oscillation;

        return transform.position + move * Time.deltaTime;
    }

    Vector3 PicadaCurva()
    {
        float attackDistance = 5f;

        Vector3 toPlayer = playerHead.position - transform.position;
        float distance = toPlayer.magnitude;

        Vector3 forwardDir = toPlayer.normalized;
        forwardDir.y *= 0.3f;

        // Antes de iniciar el ataque
        if (!diveStarted && distance > attackDistance)
        {
            return transform.position + forwardDir * forwardSpeed * Time.deltaTime;
        }

        // Inicia ataque
        if (!diveStarted)
        {
            diveStarted = true;
            turning = true;

            diveSide = Random.value < 0.5f ? -1f : 1f;
        }

        // =====================
        // GIRO DE 90°
        // =====================

        if (turning)
        {
            float step = turnSpeed * Time.deltaTime;

            turnAngle += step;

            Vector3 offset = transform.position - playerHead.position;

            offset = Quaternion.Euler(0, step * diveSide, 0) * offset;

            transform.position = playerHead.position + offset;

            if (turnAngle >= maxTurn)
            {
                turning = false;
            }

            return transform.position;
        }

        // =====================
        // Después del giro
        // =====================

        return transform.position + forwardDir * forwardSpeed * Time.deltaTime;
    }

    Vector3 Cazador()
    {
        float distance = Vector3.Distance(transform.position, playerHead.position);

        // ======================
        // SALTO
        // ======================
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;

            float t = jumpTimer / jumpDuration;

            // interpolación horizontal fija (no sigue al jugador)
            Vector3 pos = Vector3.Lerp(jumpStart, jumpTarget, t);

            // arco del salto
            pos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            if (t >= 1f)
            {
                isJumping = false;
                isRetreating = true;

                retreatDirection = (transform.position - playerHead.position).normalized;
                retreatDirection.y = 0;
            }

            return pos;
        }

        // ======================
        // RETIRADA
        // ======================
        if (isRetreating)
        {
            if (Vector3.Distance(transform.position, playerHead.position) >= retreatDistance)
            {
                isRetreating = false;
            }

            return transform.position + retreatDirection * forwardSpeed * Time.deltaTime;
        }

        // ======================
        // PERSECUCIÓN
        // ======================
        Vector3 direction = (playerHead.position - transform.position).normalized;
        direction.y *= 0.4f;

        if (distance <= jumpDistance)
        {
            isJumping = true;
            jumpTimer = 0f;

            jumpStart = transform.position;
            jumpTarget = playerHead.position;
        }

        return transform.position + direction * forwardSpeed * Time.deltaTime;
    }

    Vector3 ZigZag()
    {
        if (timer >= zigZagInterval)
        {
            zigZagDirection *= -1;
            timer = 0;
        }

        // Dirección hacia el jugador
        Vector3 forwardDir = (playerHead.position - transform.position).normalized;
        forwardDir.y *= 0.3f; // suaviza movimiento vertical para VR

        // Movimiento lateral del zigzag (perpendicular)
        Vector3 sideDir = Vector3.Cross(forwardDir, Vector3.up).normalized;

        Vector3 move =
            forwardDir * forwardSpeed +
            sideDir * zigZagDirection * amplitude;

        return transform.position + move * Time.deltaTime;
    }

    Vector3 Rodeo()
    {
        float distance = Vector3.Distance(transform.position, playerHead.position);

        if (distance > orbitDistance)
        {
            Vector3 direction = (playerHead.position - transform.position).normalized;
            direction.y *= 0.4f;

            return transform.position + direction * forwardSpeed * Time.deltaTime;
        }
        else
        {
            Vector3 offset = transform.position - playerHead.position;
            offset = Quaternion.Euler(0, orbitSpeed * Time.deltaTime, 0) * offset;

            return playerHead.position + offset;
        }
    }

    // ==========================
    // LÍMITES
    // ==========================

    void ApplyLimits()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, -xLimit, xLimit);

        if (pos.y < groundLimit)
            pos.y = groundLimit;

        pos.y = Mathf.Clamp(pos.y, groundLimit, yMaxLimit);

        pos.z = Mathf.Clamp(pos.z, zMinLimit, zMaxLimit);

        transform.position = pos;
    }
}