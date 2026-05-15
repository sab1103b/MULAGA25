using UnityEngine;

public class CerrarPuertaTrigger : MonoBehaviour
{
    [Header("Sistema Puerta")]
    public JefeRoomSystem system;

    [Header("Arena Bounds")]
    public Collider arenaBounds;   // BoxCollider separado para límites de arena

    [Header("Boss")]
    public GameObject bossObject;

    private bool fightStarted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (fightStarted) return;

        if (other.CompareTag("MainCamera"))
        {
            fightStarted = true;

            // Cierra la puerta permanentemente
            if (system != null)
                system.CerrarPuerta();

            // Activar jefe
            if (bossObject != null)
                bossObject.GetComponent<BossController>()?.ActivateBoss();

            Debug.Log("Boss Fight iniciada.");
        }
    }

    public bool IsInsideArena(Vector3 position)
    {
        if (arenaBounds == null)
            return true;

        return arenaBounds.bounds.Contains(position);
    }
}