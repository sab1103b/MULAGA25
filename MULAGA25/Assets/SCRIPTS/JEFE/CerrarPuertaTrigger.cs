using UnityEngine;

public class CerrarPuertaTrigger : MonoBehaviour
{
    public JefeRoomSystem system;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            system.CerrarPuerta();
        }
    }
}