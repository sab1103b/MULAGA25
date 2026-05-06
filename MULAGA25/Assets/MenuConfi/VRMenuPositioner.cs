using UnityEngine;

public class VRMenuPositioner : MonoBehaviour
{
    [Header("Canvas del menú")]
    public Transform panel; 

    [Header("Referencia a la cámara XR")]
    public Transform xrCamera;

    [Header("Distancia")]
    public float distancia = 1.5f;

    [Header("Altura")]
    public float alturaOffset = -0.2f;

    public void PlaceMenu()
    {
        if (xrCamera == null || panel == null) return;

        // Dirección horizontal
        Vector3 forward = xrCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        // Posición frente al jugador
        Vector3 pos = xrCamera.position + forward * distancia;
        pos.y += alturaOffset;

        panel.position = pos;

        // Rotación hacia el jugador
        panel.LookAt(xrCamera);
        panel.eulerAngles = new Vector3(0, panel.eulerAngles.y, 0);
    }
}