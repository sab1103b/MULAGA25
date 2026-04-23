using UnityEngine;

public class VRHUD_Follow : MonoBehaviour
{
    public Transform cameraTransform;

    [Header("Distancia y posición")]
    public Vector3 offset = new Vector3(0f, 0f, 0.33f);

    [Header("Suavizado")]
    public float positionSmooth = 10f;
    public float rotationSmooth = 12f;

    void Start()
    {
        if (!cameraTransform)
            cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (!cameraTransform) return;

        // -------------------------------
        // POSICIÓN OBJETIVO
        // -------------------------------
        Vector3 targetPosition = cameraTransform.position + cameraTransform.TransformDirection(offset);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionSmooth * Time.deltaTime
        );

        // -------------------------------
        // ROTACIÓN CORRECTA (alineada con la cámara)
        // -------------------------------
        Quaternion targetRotation = Quaternion.LookRotation(cameraTransform.forward, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmooth * Time.deltaTime
        );
    }
}