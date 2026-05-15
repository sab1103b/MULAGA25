using UnityEngine;

public class BossArena : MonoBehaviour
{
    [Header("Arena Settings")]
    public Collider arenaBounds;      // BoxCollider SEPARADO solo para los límites
    public GameObject exitBlocker;
    public GameObject bossObject;

    // El trigger de entrada es el BoxCollider de ESTE mismo GameObject
    // arenaBounds es un BoxCollider HIJO separado, más grande, para IsInsideArena
    private bool fightStarted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!fightStarted && other.CompareTag("Player"))
            StartBossFight();
    }

    private void StartBossFight()
    {
        fightStarted = true;

        if (exitBlocker != null)
            exitBlocker.SetActive(true);

        if (bossObject != null)
            bossObject.GetComponent<BossController>()?.ActivateBoss();

        Debug.Log("Boss Fight iniciado. Salida bloqueada.");
    }

    public void EndBossFight()
    {
        if (exitBlocker != null)
            exitBlocker.SetActive(false);

        fightStarted = false;
        Debug.Log("Boss derrotado. Salida desbloqueada.");
    }

    public bool IsInsideArena(Vector3 position)
    {
        // Si no hay arenaBounds asignado, no restringir
        if (arenaBounds == null) return true;
        return arenaBounds.bounds.Contains(position);
    }
}