using UnityEngine;

public class HealItem : MonoBehaviour
{
    public int healAmount = 1;

    private void OnCollisionEnter(Collision collision)
    {
        PlayerModel player = FindFirstObjectByType<PlayerModel>();

        if (player != null)
        {
            if (!player.isDead && player.currentLives < player.maxLives)
            {
                player.currentLives += healAmount;

                if (player.currentLives > player.maxLives)
                    player.currentLives = player.maxLives;

                Debug.Log("Jugador curado. Vidas actuales: " + player.currentLives);

                if (player.hud != null)
                {
                    player.hud.SetHealth(player.currentLives);
                }

                Destroy(gameObject);
            }
        }
    }
}