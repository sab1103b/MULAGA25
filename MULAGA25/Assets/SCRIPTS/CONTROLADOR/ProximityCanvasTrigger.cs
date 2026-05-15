using UnityEngine;
using System.Collections;

public class ProximityCanvasTrigger : MonoBehaviour
{
    [Header("Configuración")]
    public float detectionDistance = 6f;
    public Canvas infoCanvas;

    [Header("Animación")]
    public float fadeSpeed = 4f;
    public float scaleSpeed = 6f;

    [Header("Escalas")]
    public Vector3 hiddenScale = Vector3.one * 0.8f;
    public Vector3 visibleScale = Vector3.one;

    private Transform mainCam;
    private CanvasGroup canvasGroup;
    private bool isVisible = false;

    void Start()
    {
        // Buscar Main Camera (prioridad: tag)
        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");

        if (camObj != null)
            mainCam = camObj.transform;
        else
            mainCam = Camera.main.transform;

        // Preparar Canvas
        canvasGroup = infoCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = infoCanvas.gameObject.AddComponent<CanvasGroup>();

        // Estado inicial oculto
        infoCanvas.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        infoCanvas.transform.localScale = Vector3.one * 0.8f;
    }

    void Update()
    {
        if (mainCam == null) return;

        float distance = Vector3.Distance(mainCam.position, transform.position);

        if (distance <= detectionDistance)
        {
            if (!isVisible)
            {
                StopAllCoroutines();
                StartCoroutine(ShowCanvas());
            }
        }
        else
        {
            if (isVisible)
            {
                StopAllCoroutines();
                StartCoroutine(HideCanvas());
            }
        }
    }

    IEnumerator ShowCanvas()
    {
        isVisible = true;

        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;

            infoCanvas.transform.localScale = Vector3.Lerp(
                infoCanvas.transform.localScale,
                visibleScale,
                Time.deltaTime * scaleSpeed
            );

            yield return null;
        }

        canvasGroup.alpha = 1f;
        infoCanvas.transform.localScale = visibleScale;
    }

    IEnumerator HideCanvas()
    {
        isVisible = false;

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;

            infoCanvas.transform.localScale = Vector3.Lerp(
                infoCanvas.transform.localScale,
                hiddenScale,
                Time.deltaTime * scaleSpeed
            );

            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}