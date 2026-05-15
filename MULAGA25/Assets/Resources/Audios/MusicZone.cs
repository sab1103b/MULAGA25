using UnityEngine;

public class MusicZone : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("MainCamera"))
        {
            triggered = true;
            MusicManager.Instance.UnlockBossMusic();
        }
    }
}