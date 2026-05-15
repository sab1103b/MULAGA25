using UnityEngine;

public class Salida : MonoBehaviour
{
    [Header("Orbes")]
    public Transform orbesalida;

    [Header("Llaves")]
    public Transform llavesalida;

    [Header("Puerta")]
    public Transform puerta;
    public float doorSpeed = 2f;

    [Header("Ángulos Puerta")]
    public float closedAngle = 232f;
    public float openAngle = 142f;

    [Header("Detección")]
    public float activationDistance = 0.5f;

    [Header("Rotación Orbe")]
    public float rotationSpeed = 5f;
    public float orbeRotationAngle = -90f;

    private Quaternion puertaClosedRot;
    private Quaternion puertaOpenRot;

    private Quaternion orbeInitialRot;
    private Quaternion orbeTargetRot;

    private bool isOpen = false;
    private bool permanentlyLocked = false;

    void Start()
    {
        puertaClosedRot = Quaternion.Euler(0f, closedAngle, 0f);
        puertaOpenRot = Quaternion.Euler(0f, openAngle, 0f);
    }

    void Update()
    {
        if (orbesalida == null || llavesalida == null) return;

        bool enContacto = Vector3.Distance(orbesalida.position, llavesalida.position) < activationDistance;

        // Rotación orbe
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

        // lógica de puerta
        if (!permanentlyLocked)
            isOpen = enContacto;

        Quaternion targetRot = isOpen ? puertaOpenRot : puertaClosedRot;

        puerta.rotation = Quaternion.Lerp(
            puerta.rotation,
            targetRot,
            doorSpeed * Time.deltaTime
        );
    }

    // 🔥 REGISTRO DEL ORBE SPAWN DEL BOSS
    public void RegistrarOrbeBoss(Transform nuevoOrbe)
    {
        orbesalida = nuevoOrbe;

        orbeInitialRot = orbesalida.rotation;
        orbeTargetRot = orbeInitialRot * Quaternion.Euler(0f, orbeRotationAngle, 0f);
    }

    public void CerrarPuerta()
    {
        isOpen = false;
        permanentlyLocked = true;
    }
}