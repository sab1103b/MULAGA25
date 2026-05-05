using UnityEngine;
using System.Collections;

public class DangerZoneIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    public float radius = 1.8f;
    public Color warningColor = new Color(1f, 0.2f, 0f, 0.6f);
    public Color dangerColor = new Color(1f, 0f, 0f, 0.9f);

    private Renderer zoneRenderer;

    private void Awake()
    {
        zoneRenderer = GetComponent<Renderer>();
        ApplyRadius();
    }

    public void SetRadius(float r)
    {
        radius = r;
        ApplyRadius();
    }

    private void ApplyRadius()
    {
        transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
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

            float pulse = Mathf.Abs(Mathf.Sin(elapsed * (2f + t * 6f) * Mathf.PI));

            Color current = Color.Lerp(warningColor, dangerColor, t);
            current.a = pulse * 0.8f;

            if (zoneRenderer != null)
                zoneRenderer.material.color = current;

            yield return null;
        }

        if (zoneRenderer != null)
        {
            Color final = dangerColor;
            final.a = 1f;
            zoneRenderer.material.color = final;
        }
    }
}