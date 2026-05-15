using System.Collections;
using UnityEngine;

public class DeployableShield : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float deployDuration = 0.6f;
    [SerializeField] private float lifetime = 15f;
    [SerializeField] private int shieldHealth = 100;
    [SerializeField] private GameObject deployEffectPrefab;
    [SerializeField] private GameObject destroyEffectPrefab;

    [Header("Escala final del escudo")]
    [SerializeField] private Vector3 deployedScale = new Vector3(1f, 2f, 0.15f);

    [Header("Detección de piso")]
    [Tooltip("Capas que cuentan como piso. Puedes dejar Everything si no usas capas.")]
    [SerializeField] private LayerMask groundMask = ~0;

    [Tooltip("Qué tan inclinado puede estar el piso. 0 = solo piso plano, 45 = rampas.")]
    [SerializeField] private float maxGroundAngle = 35f;

    [Tooltip("Altura desde donde se lanza el raycast hacia abajo para encontrar piso.")]
    [SerializeField] private float groundCheckStartHeight = 1.5f;

    [Tooltip("Distancia máxima para buscar el piso hacia abajo.")]
    [SerializeField] private float groundCheckDistance = 4f;

    [Tooltip("Pequeño offset para que el escudo no quede metido dentro del piso.")]
    [SerializeField] private float floorOffset = 0.03f;

    [Header("Orientación")]
    [Tooltip("Ajusta si el mesh sale girado. Prueba 0, 90, 180, 270.")]
    [SerializeField] private float meshRotationOffset = 0f;

    // Asignado por ShieldBombItem antes de llamar Deploy()
    [HideInInspector] public Vector3 throwerForward = Vector3.forward;

    private int currentHealth;
    private bool isDeployed = false;
    private bool isDestroying = false;

    private void Awake()
    {
        currentHealth = shieldHealth;
        transform.localScale = Vector3.zero;
    }

    public void Deploy(Vector3 position, Vector3 surfaceNormal)
    {
        if (isDeployed) return;

        Vector3 finalPosition;
        Vector3 finalNormal;

        bool validFloor = TryGetValidFloor(position, surfaceNormal, out finalPosition, out finalNormal);

        if (!validFloor)
        {
            Debug.Log("[SHIELD] No se desplegó: no encontró piso válido.");
            Destroy(gameObject);
            return;
        }

        isDeployed = true;

        // Kinematic: nadie puede empujarlo
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        transform.position = finalPosition;
        transform.rotation = GetShieldRotation(finalNormal);

        if (deployEffectPrefab != null)
        {
            GameObject fx = Instantiate(deployEffectPrefab, finalPosition, Quaternion.identity);
            Destroy(fx, 3f);
        }

        StartCoroutine(DeployAnimation());
        StartCoroutine(LifetimeRoutine());
    }

    private bool TryGetValidFloor(
        Vector3 originalPosition,
        Vector3 surfaceNormal,
        out Vector3 floorPosition,
        out Vector3 floorNormal)
    {
        floorPosition = originalPosition;
        floorNormal = Vector3.up;

        if (IsFloorNormal(surfaceNormal))
        {
            floorPosition = originalPosition + surfaceNormal.normalized * floorOffset;
            floorNormal = surfaceNormal.normalized;
            return true;
        }

        // Tocó pared — busca piso debajo
        Vector3 rayStart = originalPosition + Vector3.up * groundCheckStartHeight;

        if (Physics.Raycast(
                rayStart, Vector3.down, out RaycastHit hit,
                groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (IsFloorNormal(hit.normal))
            {
                floorPosition = hit.point + hit.normal.normalized * floorOffset;
                floorNormal = hit.normal.normalized;
                return true;
            }
        }

        return false;
    }

    private bool IsFloorNormal(Vector3 normal)
    {
        if (normal.sqrMagnitude <= 0.001f) return false;
        float angle = Vector3.Angle(normal.normalized, Vector3.up);
        return angle <= maxGroundAngle;
    }

    private Quaternion GetShieldRotation(Vector3 floorNormal)
    {
        // throwerForward ya viene horizontal (Y=0) desde ShieldBombItem.
        // Lo proyectamos sobre el plano del piso por si el suelo es una rampa.
        Vector3 forward = Vector3.ProjectOnPlane(throwerForward, floorNormal);

        if (forward.sqrMagnitude <= 0.001f)
            forward = Vector3.ProjectOnPlane(Vector3.forward, floorNormal);

        if (forward.sqrMagnitude <= 0.001f)
            forward = Vector3.Cross(Vector3.right, floorNormal);

        forward.Normalize();

        return Quaternion.LookRotation(forward, floorNormal)
               * Quaternion.Euler(0f, meshRotationOffset, 0f);
    }

    private IEnumerator DeployAnimation()
    {
        float elapsed = 0f;
        while (elapsed < deployDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / deployDuration);
            transform.localScale = Vector3.Lerp(Vector3.zero, deployedScale, t);
            yield return null;
        }
        transform.localScale = deployedScale;
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, lifetime - 1f));

        float elapsed = 0f;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            bool visible = Mathf.FloorToInt(elapsed / 0.1f) % 2 == 0;
            foreach (Renderer r in renderers)
                if (r != null) r.enabled = visible;
            yield return null;
        }

        DestroyShield();
    }

    public void TakeDamage(int damage)
    {
        if (!isDeployed) return;
        currentHealth -= damage;
        if (currentHealth <= 0) DestroyShield();
    }

    private void DestroyShield()
    {
        if (isDestroying) return;
        isDestroying = true;

        if (destroyEffectPrefab != null)
        {
            GameObject fx = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        Destroy(gameObject);
    }

    public bool IsAlive() => isDeployed && currentHealth > 0;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, deployedScale);

        Gizmos.color = Color.yellow;
        Vector3 rayStart = transform.position + Vector3.up * groundCheckStartHeight;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
    }
}