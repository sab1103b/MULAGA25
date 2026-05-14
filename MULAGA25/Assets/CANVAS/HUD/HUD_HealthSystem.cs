using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUD_HealthSystem : MonoBehaviour
{
    public Image[] hearts;

    public Sprite fullHeart;
    public Sprite brokenHeart;

    private int currentHealth;

    void OnEnable()
    {
        // Cada vez que la escena se carga/reinicia
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        ResetHearts();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetHearts();
    }

    void ResetHearts()
    {
        currentHealth = hearts.Length;
        UpdateHearts();
    }

    public void SetHealth(int health)
    {
        currentHealth =
            Mathf.Clamp(
                health,
                0,
                hearts.Length
            );

        UpdateHearts();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite =
                (i < currentHealth)
                ? fullHeart
                : brokenHeart;
        }
    }
}