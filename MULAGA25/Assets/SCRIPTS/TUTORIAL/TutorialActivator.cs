using UnityEngine;
using System.Collections;

public class TutorialActivator : MonoBehaviour
{
    [Header("Objetos a revisar")]
    public GameObject[] objetos;

    [Header("Objeto que bajará")]
    public Transform objetoMover;

    [Header("Cantidad a bajar")]
    public float bajarY = 6f;

    [Header("Velocidad de bajada")]
    public float velocidad = 2f;

    private bool yaActivo = false;

    void Update()
    {
        if (yaActivo) return;

        int desactivados = 0;

        foreach (GameObject obj in objetos)
        {
            if (!obj.activeInHierarchy)
            {
                desactivados++;
            }
        }

        // Cuando los 4 estén desactivados
        if (desactivados >= 4)
        {
            yaActivo = true;
            StartCoroutine(BajarSuavemente());
        }
    }

    IEnumerator BajarSuavemente()
    {
        Vector3 posicionInicial = objetoMover.position;
        Vector3 posicionFinal = posicionInicial - new Vector3(0, bajarY, 0);

        float progreso = 0f;

        while (progreso < 1f)
        {
            progreso += Time.deltaTime * velocidad;

            objetoMover.position = Vector3.Lerp(
                posicionInicial,
                posicionFinal,
                progreso
            );

            yield return null;
        }

        objetoMover.position = posicionFinal;
    }
}