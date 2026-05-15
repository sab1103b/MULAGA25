using UnityEngine;
using System.Collections;

public class AreaDetector : MonoBehaviour
{
    [Header("Objeto exacto a detectar")]
    public GameObject objetoObjetivo;

    [Header("Objeto que se moverá")]
    public Transform objetoMover;

    [Header("Distancia")]
    public float moverY = 6f;

    [Header("Velocidad")]
    public float velocidad = 2f;

    [HideInInspector]
    public bool objetoDentro = false;

    private Vector3 posicionCerrada;
    private Vector3 posicionAbierta;

    private Coroutine movimientoActual;

    void Start()
    {
        posicionCerrada = objetoMover.position;
        posicionAbierta = posicionCerrada - new Vector3(0, moverY, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == objetoObjetivo)
        {
            objetoDentro = true;

            if (movimientoActual != null)
                StopCoroutine(movimientoActual);

            movimientoActual = StartCoroutine(MoverSuavemente(posicionAbierta));
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == objetoObjetivo)
        {
            objetoDentro = false;

            if (movimientoActual != null)
                StopCoroutine(movimientoActual);

            movimientoActual = StartCoroutine(MoverSuavemente(posicionCerrada));
        }
    }

    IEnumerator MoverSuavemente(Vector3 destino)
    {
        Vector3 inicio = objetoMover.position;

        float progreso = 0f;

        while (progreso < 1f)
        {
            progreso += Time.deltaTime * velocidad;

            objetoMover.position = Vector3.Lerp(inicio, destino, progreso);

            yield return null;
        }

        objetoMover.position = destino;
    }
}