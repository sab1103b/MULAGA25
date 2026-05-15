using UnityEngine;

/// <summary>
/// Gestiona la muerte del enemigo.
/// Llama TriggerDeath() antes de desactivar el objeto
/// para que la animación de muerte se reproduzca.
///
/// IMPORTANTE: Si usas object pooling (SetActive(false)),
/// la animación de muerte necesita terminar ANTES de desactivar.
/// Usa el método DieWithDelay() si quieres esperar a que termine.
/// </summary>
public class EnemyLife : MonoBehaviour
{
    [Header("Drop Settings")]
    public GameObject item1;
    public GameObject item2;
    public GameObject item3;

    [Range(0, 1)] public float probItem1 = 0.5f;
    [Range(0, 1)] public float probItem2 = 0.3f;
    [Range(0, 1)] public float probItem3 = 0.2f;

    [Header("Death Animation")]
    [Tooltip("Duración de la animación de muerte en segundos. " +
             "El objeto se desactiva después de este tiempo.")]
    public float deathAnimationDuration = 1.5f;

    public void Die()
    {
        Debug.Log("Enemigo muerto: " + gameObject.name);

        // SFX de muerte
        GetComponent<PATRONES>()?.PlayDeathSFX();
        GetComponent<PATRONES_Rango>()?.PlayDeathSFX();

        // Trigger de animación de muerte
        GetComponent<PATRONES>()?.TriggerDeath();
        GetComponent<PATRONES_Rango>()?.TriggerDeath();

        // Drops
        DropItem();

        // Desactivar después de que termine la animación
        StartCoroutine(DeactivateAfterDelay());
    }

    private System.Collections.IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        gameObject.SetActive(false);
    }

    void DropItem()
    {
        float rand = Random.value;

        if (rand < probItem1)
            Instantiate(item1, transform.position, Quaternion.identity);
        else if (rand < probItem1 + probItem2)
            Instantiate(item2, transform.position, Quaternion.identity);
        else if (rand < probItem1 + probItem2 + probItem3)
            Instantiate(item3, transform.position, Quaternion.identity);
    }
}