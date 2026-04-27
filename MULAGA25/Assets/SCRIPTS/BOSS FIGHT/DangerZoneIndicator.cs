using UnityEngine;
using System.Collections;

public class DangerZoneIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    public float radius = 3f;
    public Color warningColor = new Color(1f, 0.2f, 0f, 0.6f);   // Rojo-naranja
    public Color dangerColor = new Color(1f, 0f, 0f, 0.9f);       // Rojo intenso al explotar
    private Renderer zoneRenderer;

    private void Awake()
    {
        zoneRenderer = GetComponent<Renderer>();
        transform.localScale = new Vector3(radius * 2, 0.01f, radius * 2);
    }

    public void ShowWarning(float duration)
    {
        StartCoroutine(WarningRoutine(duration));
    }

    private IEnumerator WarningRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Parpadeo que se acelera conforme se acerca el impacto
            float pulse = Mathf.Abs(Mathf.Sin(elapsed * (2f + t * 6f) * Mathf.PI));
            Color current = Color.Lerp(warningColor, dangerColor, t);
            current.a = pulse * 0.8f;

            if (zoneRenderer != null)
                zoneRenderer.material.color = current;

            yield return null;
        }
    }
}