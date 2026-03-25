using System.Collections.Generic;
using UnityEngine;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager instance;

    [Header("Folletos recolectados")]
    public List<int> collectedFolletos = new List<int>();

    [Header("Texturas de folletos")]
    public Texture[] texturas = new Texture[5];

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddFolleto(int id)
    {
        if (!collectedFolletos.Contains(id))
        {
            collectedFolletos.Add(id);
            Debug.Log("Folleto agregado: " + id);
        }
    }

    //  obtener textura por ID
    public Texture ObtenerTextura(int id)
    {
        if (id >= 0 && id < texturas.Length)
            return texturas[id];

        return null;
    }

    // saber si está desbloqueado
    public bool EstaDesbloqueado(int id)
    {
        return collectedFolletos.Contains(id);
    }
}