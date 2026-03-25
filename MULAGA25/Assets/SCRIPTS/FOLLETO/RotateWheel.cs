using UnityEngine;

public class FolletoWheel : MonoBehaviour
{
    public Transform player;          
    public GameObject[] folletos;     

    public float radius = 3f;         
    public float rotationSpeed = 20f;

    public Material hologramMaterial;

    void Start()
    {
        PositionFolletos();
        CheckUnlocked();
    }

    void Update()
    {
        if (player == null) return;

        // la rueda sigue siempre la posición del jugador
        transform.position = player.position;

        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        // los folletos siempre miran al jugador
        foreach (GameObject folleto in folletos)
        {
            if (folleto == null) continue;

            folleto.transform.LookAt(player);
            folleto.transform.Rotate(0, 90, 90);
        }
    }

    void PositionFolletos()
    {
        if (folletos.Length == 0) return;

        float angleStep = 360f / folletos.Length;

        for (int i = 0; i < folletos.Length; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 pos = new Vector3(x, 1f, z);

            folletos[i].transform.SetParent(transform);
            folletos[i].transform.localPosition = pos;
        }
    }

    void CheckUnlocked()
    {
        for (int i = 0; i < folletos.Length; i++)
        {
            FolletoItem item = folletos[i].GetComponent<FolletoItem>();

            if (item == null) continue;

            Renderer r = folletos[i].GetComponent<Renderer>();

            if (!CollectionManager.instance.collectedFolletos.Contains(item.folletoID))
            {
                r.material = hologramMaterial;
            }
        }
    }
}