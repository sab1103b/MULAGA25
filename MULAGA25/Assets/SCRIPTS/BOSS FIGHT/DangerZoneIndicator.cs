using UnityEngine;
using System.Collections;

public class DangerZoneIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    public float radius = 0.08f;
    public Color warningColor = new Color(1f, 0.2f, 0f, 0.6f);
    public Color dangerColor = new Color(1f, 0f, 0f, 0.9f);

    [Header("Debug")]
    public bool debugScale = true;

    private Renderer zoneRenderer;

    private void Awake()
    {
        zoneRenderer = GetComponentInChildren<Renderer>();
        ApplyRadius();
    }

    public void SetRadius(float r)
    {
        radius = r;
        ApplyRadius();
    }

    private void ApplyRadius()
    {
        if (zoneRenderer == null)
        {
            zoneRenderer = GetComponentInChildren<Renderer>();
        }

        if (zoneRenderer == null)
        {
            transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
            return;
        }

        MeshFilter meshFilter = zoneRenderer.GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
            return;
        }

        Vector3 meshSize = meshFilter.sharedMesh.bounds.size;

        float targetDiameter = radius * 2f;

        float scaleX = targetDiameter / meshSize.x;
        float scaleZ = targetDiameter / meshSize.z;

        transform.localScale = new Vector3(scaleX, 0.01f, scaleZ);

        if (debugScale)
        {
            Debug.Log(
                "[DangerZone] Radius: " + radius +
                " | Diameter: " + targetDiameter +
                " | Mesh Size: " + meshSize +
                " | Final Scale: " + transform.localScale +
                " | World Scale: " + transform.lossyScale
            );
        }
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
            {
                zoneRenderer.material.color = current;
            }

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