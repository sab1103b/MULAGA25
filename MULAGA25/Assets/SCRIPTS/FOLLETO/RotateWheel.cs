using UnityEngine;

public class FolletoWheel : MonoBehaviour
{
    public Transform player;
    public GameObject[] folletos;

    public float radius = 3f;
    public float rotationSpeed = 20f;

    public Material hologramMaterial;

    private float currentRotation = 0f;

    void Start()
    {
        CheckUnlocked();
    }

    void Update()
    {
        if (player == null || folletos.Length == 0) return;

        // acumulamos rotación global
        currentRotation += rotationSpeed * Time.deltaTime;

        float angleStep = 360f / folletos.Length;

        for (int i = 0; i < folletos.Length; i++)
        {
            if (folletos[i] == null) continue;

            // -------------------------------
            // POSICIÓN EN CÍRCULO (alrededor del jugador)
            // -------------------------------
            float angle = (i * angleStep + currentRotation) * Mathf.Deg2Rad;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 worldPos = player.position + new Vector3(x, 1f, z);
            folletos[i].transform.position = worldPos;

            // -------------------------------
            // ROTACIÓN: mirar al jugador (solo eje Y)
            // -------------------------------
            Vector3 target = player.position;
            target.y = folletos[i].transform.position.y;

            folletos[i].transform.LookAt(target);
            folletos[i].transform.Rotate(0, 90, 90);
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