using UnityEngine;

public class LobbyNivel1Object : MonoBehaviour
{
    [Header("Objeto visual en lobby")]
    public GameObject objetoLobby;

    void Start()
    {
        if (GameProgress.Instance != null &&
            GameProgress.Instance.nivel1ObjetoRecogido)
        {
            objetoLobby.SetActive(true);
        }
        else
        {
            objetoLobby.SetActive(false);
        }
    }
}