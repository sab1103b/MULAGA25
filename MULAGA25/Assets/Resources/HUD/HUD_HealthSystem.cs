using UnityEngine;
using UnityEngine.UI;

public class HUD_HealthSystem : MonoBehaviour
{
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite brokenHeart;

    private int currentHealth;

    void Start()
    {
        currentHealth = hearts.Length;
        UpdateHearts();
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, hearts.Length);
        UpdateHearts();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = (i < currentHealth) ? fullHeart : brokenHeart;
        }
    }
}