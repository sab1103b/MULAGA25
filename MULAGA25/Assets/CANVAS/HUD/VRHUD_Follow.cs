using UnityEngine;
using UnityEngine.SceneManagement;

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
        BuscarCamara();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuscarCamara();
    }

    void BuscarCamara()
    {
        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            cameraTransform = mainCam.transform;
        }
    }

    void LateUpdate()
    {
        // Si la cámara desaparece o cambia
        if (cameraTransform == null)
        {
            BuscarCamara();
            return;
        }

        // -------------------------------
        // POSICIÓN OBJETIVO
        // -------------------------------
        Vector3 targetPosition =
            cameraTransform.position +
            cameraTransform.TransformDirection(offset);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionSmooth * Time.deltaTime
        );

        // -------------------------------
        // ROTACIÓN
        // -------------------------------
        Quaternion targetRotation =
            Quaternion.LookRotation(cameraTransform.forward, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmooth * Time.deltaTime
        );
    }
}