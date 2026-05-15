using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySpawnManager : MonoBehaviour
{
    void Start()
    {
        if (SceneManager.GetActiveScene().name != "LobbyScene")
            return;

        if (GameProgress.Instance != null &&
            GameProgress.Instance.tutorialCompletado)
        {
            // MOVER XR RIG
            transform.position = new Vector3(
                -8.3f,
                0.435f,
                59.7f
            );

            // ROTACIÓN
            transform.rotation = Quaternion.Euler(
                0f,
                90f,
                0f
            );
        }
    }
}