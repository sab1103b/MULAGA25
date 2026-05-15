using UnityEngine;

public class HealItem : MonoBehaviour
{
    public int healAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        PlayerModel player = other.GetComponent<PlayerModel>();

        if (player == null) return;
        if (player.isDead) return;

        if (player.currentLives < player.maxLives)
        {
            player.currentLives += healAmount;

            if (player.currentLives > player.maxLives)
                player.currentLives = player.maxLives;

            Debug.Log("Jugador curado. Vidas actuales: " + player.currentLives);

            if (player.hud != null)
                player.hud.SetHealth(player.currentLives);

            Destroy(gameObject);
        }
    }
}