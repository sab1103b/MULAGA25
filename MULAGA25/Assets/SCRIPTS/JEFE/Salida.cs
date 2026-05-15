using UnityEngine;

public class Salida : MonoBehaviour
{
    [Header("Orbes")]
    public Transform orbesalida;

    [Header("Llaves")]
    public Transform llavesalida;

    [Header("Puerta")]
    public Transform puerta;
    public float doorRotationAngle = 80f;
    public float doorSpeed = 2f;

    [Header("Detección")]
    public float activationDistance = 0.5f;

    [Header("Rotación Orbe")]
    public float rotationSpeed = 5f;
    public float orbeRotationAngle = -90f;

    private Quaternion puertaInitialRot;
    private Quaternion puertaOpenRot;

    private Quaternion orbeInitialRot;
    private Quaternion orbeTargetRot;

    private bool isOpen = false;
    private bool permanentlyLocked = false;

    void Start()
    {
        puertaInitialRot = puerta.rotation;
        puertaOpenRot = puertaInitialRot * Quaternion.Euler(0f, doorRotationAngle, 0f);

        orbeInitialRot = orbesalida.rotation;
        orbeTargetRot = orbeInitialRot * Quaternion.Euler(0f, orbeRotationAngle, 0f);
    }

    void Update()
    {
        bool enContacto = Vector3.Distance(orbesalida.position, llavesalida.position) < activationDistance;

        // Rotación del orbe
        if (enContacto)
        {
            orbesalida.rotation = Quaternion.Lerp(
                orbesalida.rotation,
                orbeTargetRot,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            orbesalida.rotation = Quaternion.Lerp(
                orbesalida.rotation,
                orbeInitialRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // Lógica de apertura
        if (!isOpen && !permanentlyLocked)
        {
            if (enContacto)
                isOpen = true;
        }

        // ROTACIÓN DE LA PUERTA (no movimiento)
        if (isOpen)
        {
            puerta.rotation = Quaternion.Lerp(
                puerta.rotation,
                puertaOpenRot,
                doorSpeed * Time.deltaTime
            );
        }
        else
        {
            puerta.rotation = Quaternion.Lerp(
                puerta.rotation,
                puertaInitialRot,
                doorSpeed * Time.deltaTime
            );
        }
    }

    public void CerrarPuerta()
    {
        isOpen = false;
        permanentlyLocked = true;
    }
}