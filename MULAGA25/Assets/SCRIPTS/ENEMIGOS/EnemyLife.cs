using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    [Header("Drop Settings")]
    public GameObject item1;
    public GameObject item2;
    public GameObject item3;

    [Range(0, 1)] public float probItem1 = 0.5f;
    [Range(0, 1)] public float probItem2 = 0.3f;
    [Range(0, 1)] public float probItem3 = 0.2f;

    public void Die()
    {
        Debug.Log("Enemigo muerto");

        DropItem();

        gameObject.SetActive(false);
    }

    void DropItem()
    {
        float rand = Random.value;

        if (rand < probItem1)
        {
            Instantiate(item1, transform.position, Quaternion.identity);
        }
        else if (rand < probItem1 + probItem2)
        {
            Instantiate(item2, transform.position, Quaternion.identity);
        }
        else if (rand < probItem1 + probItem2 + probItem3)
        {
            Instantiate(item3, transform.position, Quaternion.identity);
        }
        // si no entra en nada → no dropea
    }
}