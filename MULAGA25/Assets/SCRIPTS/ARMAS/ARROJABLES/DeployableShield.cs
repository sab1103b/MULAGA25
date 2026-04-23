using System.Collections;
using UnityEngine;

public class DeployableShield : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float deployDuration = 0.6f;   // Tiempo de animación de despliegue
    [SerializeField] private float lifetime = 15f;           // Segundos antes de desaparecer
    [SerializeField] private int shieldHealth = 100;
    [SerializeField] private GameObject deployEffectPrefab;
    [SerializeField] private GameObject destroyEffectPrefab;

    [Header("Escala final del escudo")]
    [SerializeField] private Vector3 deployedScale = new Vector3(1f, 2f, 0.15f);

    private int currentHealth;
    private bool isDeployed = false;

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
        isDeployed = true;

        // Orientamos el escudo perpendicular a la superficie
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(
            Vector3.ProjectOnPlane(Camera.main != null
                ? (Camera.main.transform.position - position).normalized
                : Vector3.forward,
            surfaceNormal),
            surfaceNormal
        );

        if (deployEffectPrefab != null)
        {
            GameObject fx = Instantiate(deployEffectPrefab, position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        StartCoroutine(DeployAnimation());
        StartCoroutine(LifetimeRoutine());
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
        yield return new WaitForSeconds(lifetime - 1f);

        // Parpadea antes de desaparecer
        float blinkTime = 1f;
        float elapsed = 0f;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        while (elapsed < blinkTime)
        {
            elapsed += Time.deltaTime;
            bool visible = (Mathf.FloorToInt(elapsed / 0.1f) % 2 == 0);
            foreach (Renderer r in renderers)
                r.enabled = visible;
            yield return null;
        }

        DestroyShield();
    }

    // Implementa IDamageable si quieres que el escudo reciba daño
    public void TakeDamage(int damage)
    {
        if (!isDeployed) return;
        currentHealth -= damage;
        if (currentHealth <= 0)
            DestroyShield();
    }

    private void DestroyShield()
    {
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
    }
}