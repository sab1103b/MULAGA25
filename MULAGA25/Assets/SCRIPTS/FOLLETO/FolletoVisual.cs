using UnityEngine;

public class FolletoVisual : MonoBehaviour
{
    public MonoBehaviour referenciaID; // 🔥 arrastras cualquier script que tenga el ID

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();

        if (referenciaID != null && rend != null && CollectionManager.instance != null)
        {
            int id = ObtenerID(referenciaID);

            Texture tex = CollectionManager.instance.ObtenerTextura(id);

            if (tex != null)
                rend.material.mainTexture = tex;
        }
    }

    int ObtenerID(MonoBehaviour script)
    {

        if (script is FolletoItem fi1)
            return fi1.folletoID;

        if (script is LobbyFolletoDisplay fi2)
            return fi2.folletoID;

        Debug.LogWarning("No se pudo obtener ID del script");
        return -1;
    }

}