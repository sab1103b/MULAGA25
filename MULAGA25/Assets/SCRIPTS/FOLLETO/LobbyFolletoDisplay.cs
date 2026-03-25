using UnityEngine;

public class LobbyFolletoDisplay : MonoBehaviour
{
    public int folletoID;

    public Material hologramMaterial;

    Renderer r;
    Material originalMaterial;

    void Start()
    {
        r = GetComponent<Renderer>();
        originalMaterial = r.material;

        if (CollectionManager.instance == null)
        {
            Debug.Log("No hay CollectionManager");
            return;
        }

        if (CollectionManager.instance.collectedFolletos.Contains(folletoID))
        {
            r.material = originalMaterial;
            Debug.Log("Mostrando folleto real " + folletoID);
        }
        else
        {
            r.material = hologramMaterial;
            Debug.Log("Mostrando holograma " + folletoID);
        }
    }
}