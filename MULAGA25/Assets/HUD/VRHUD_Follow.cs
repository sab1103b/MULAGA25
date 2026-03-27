using UnityEngine;

public class VRHUD_Follow : MonoBehaviour
{
    public Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (!cameraTransform) return;

        transform.position = cameraTransform.position + cameraTransform.forward * 1.8f;

        Vector3 lookPos = cameraTransform.position;
        lookPos.y = transform.position.y;

        transform.LookAt(lookPos);
    }
}