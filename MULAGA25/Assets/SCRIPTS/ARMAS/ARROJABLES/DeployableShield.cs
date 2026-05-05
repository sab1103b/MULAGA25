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
    [Tooltip("Si está activo, el escudo mira hacia la cámara/jugador al desplegarse.")]
    [SerializeField] private bool faceCameraOnDeploy = true;

    private int currentHealth;
    private bool isDeployed = false;
    private bool isDestroying = false;

    private void Awake()
    {
        currentHealth = shieldHealth;

        // Empezamos pequeño para animar el despliegue
        transform.localScale = Vector3.zero;
    }

    // Llamado por ShieldBombItem al caer
    public void Deploy(Vector3 position, Vector3 surfaceNormal)
    {
        if (isDeployed) return;

        Vector3 finalPosition;
        Vector3 finalNormal;

        bool validFloor = TryGetValidFloor(position, surfaceNormal, out finalPosition, out finalNormal);

        if (!validFloor)
        {
            Debug.Log("[SHIELD] No se desplegó porque no encontró piso válido.");
            Destroy(gameObject);
            return;
        }

        isDeployed = true;

        transform.position = finalPosition;

        // El escudo queda vertical, apoyado en el piso.
        // No se alinea a paredes.
        transform.rotation = GetShieldRotation(finalPosition, finalNormal);

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

        // Primero revisamos si la superficie original ya parece piso.
        if (IsFloorNormal(surfaceNormal))
        {
            floorPosition = originalPosition + surfaceNormal.normalized * floorOffset;
            floorNormal = surfaceNormal.normalized;
            return true;
        }

        // Si no era piso, probablemente tocó pared.
        // Entonces buscamos piso hacia abajo.
        Vector3 rayStart = originalPosition + Vector3.up * groundCheckStartHeight;

        if (Physics.Raycast(
                rayStart,
                Vector3.down,
                out RaycastHit hit,
                groundCheckDistance,
                groundMask,
                QueryTriggerInteraction.Ignore))
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
        if (normal.sqrMagnitude <= 0.001f)
            return false;

        float angle = Vector3.Angle(normal.normalized, Vector3.up);

        return angle <= maxGroundAngle;
    }

    private Quaternion GetShieldRotation(Vector3 position, Vector3 floorNormal)
    {
        Vector3 forward;

        if (faceCameraOnDeploy && Camera.main != null)
        {
            forward = Camera.main.transform.position - position;
        }
        else
        {
            forward = transform.forward;
        }

        // Proyectamos la dirección sobre el plano del piso.
        // Así el escudo queda de pie sobre el piso y no acostado ni pegado a paredes.
        forward = Vector3.ProjectOnPlane(forward, floorNormal);

        if (forward.sqrMagnitude <= 0.001f)
            forward = Vector3.ProjectOnPlane(Vector3.forward, floorNormal);

        if (forward.sqrMagnitude <= 0.001f)
            forward = Vector3.Cross(Vector3.right, floorNormal);

        forward.Normalize();

        return Quaternion.LookRotation(forward, floorNormal);
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
        float waitTime = Mathf.Max(0f, lifetime - 1f);

        yield return new WaitForSeconds(waitTime);

        float blinkTime = 1f;
        float elapsed = 0f;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        while (elapsed < blinkTime)
        {
            elapsed += Time.deltaTime;

            bool visible = Mathf.FloorToInt(elapsed / 0.1f) % 2 == 0;

            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = visible;
            }

            yield return null;
        }

        DestroyShield();
    }

    public void TakeDamage(int damage)
    {
        if (!isDeployed) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            DestroyShield();
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

    public bool IsAlive()
    {
        return isDeployed && currentHealth > 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, deployedScale);

        Gizmos.color = Color.yellow;
        Vector3 rayStart = transform.position + Vector3.up * groundCheckStartHeight;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
    }
}