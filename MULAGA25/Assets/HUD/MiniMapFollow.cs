using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    [Header("Referencia")]
    public Transform player;

    [Header("Altura fija del minimapa")]
    public float height = 7.6f;

    [Header("Suavizado")]
    public float followSpeed = 8f;

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
    }
}