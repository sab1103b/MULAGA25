using UnityEngine;

public class ConsejeroInicioDelay : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(LlamarConsejero), 3f);
    }

    void LlamarConsejero()
    {
        ConsejeroManager.Instance.EventoEntraNivel();
    }
}