using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    [Header("Referencia")]
    public Transform player;

    [Header("Altura fija del minimapa")]
    public float height = 6.14f;

    [Header("Suavizado")]
    public float followSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Offset opcional")]
    public Vector3 offset;

    private Vector3 targetPosition;

    void LateUpdate()
    {
        if (player == null) return;

        // Sigue solo X y Z del jugador, Y siempre fija
        targetPosition = new Vector3(
            player.position.x + offset.x,
            height + offset.y,
            player.position.z + offset.z
        );

        // Movimiento suave sin vibración
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // ROTACIÓN (solo eje Y)
        // =========================
        float targetYRotation = player.eulerAngles.y;

        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
        // 90f en X para que mire hacia abajo (típico minimapa)

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}