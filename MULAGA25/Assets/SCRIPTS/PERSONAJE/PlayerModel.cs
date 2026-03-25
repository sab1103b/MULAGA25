using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [Header("Lives")]
    public int maxLives = 3;
    public int currentLives;

    [Header("Collectibles")]
    public int posterFragments = 0;

    void Awake()
    {
        currentLives = maxLives;
    }

    public void TakeDamage(int amount)
    {
        currentLives -= amount;

        if (currentLives < 0)
            currentLives = 0;

        Debug.Log("Vidas restantes: " + currentLives);
    }

    public void AddFragment()
    {
        posterFragments++;

        Debug.Log("Fragmentos recolectados: " + posterFragments);
    }

}