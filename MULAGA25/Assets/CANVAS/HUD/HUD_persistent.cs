using UnityEngine;

public class HUD_Persistent : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}