using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    public void Die()
    {
        Debug.Log("Enemigo muerto");
        gameObject.SetActive(false);
    }
}