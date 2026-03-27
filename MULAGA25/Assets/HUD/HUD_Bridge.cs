using UnityEngine;

public class HUD_Bridge : MonoBehaviour
{
    public HUD_HealthSystem healthSystem;

    public void UpdateHealth(int health)
    {
        healthSystem.SetHealth(health);
    }
}