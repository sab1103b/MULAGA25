using UnityEngine;

public class MovimientoSinusoidal : MonoBehaviour
{
    [Header("Movimiento")]
    public float amplitud = 1f;      // cuánto sube y baja
    public float frecuencia = 1f;    // qué tan rápido oscila

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * frecuencia) * amplitud;

        transform.position = new Vector3(
            posicionInicial.x,
            posicionInicial.y + offsetY,
            posicionInicial.z
        );
    }
}