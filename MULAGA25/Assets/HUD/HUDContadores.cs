using UnityEngine;
using TMPro;

public class HUDContadores : MonoBehaviour
{
    public static HUDContadores Instance;

    public TextMeshProUGUI textoBombas;
    public TextMeshProUGUI textoEscudos;

    int bombas = 0;
    int escudos = 0;

    void Awake()
    {
        Instance = this;
    }

    public void AgregarBomba()
    {
        bombas++;
        textoBombas.text = bombas.ToString();
    }

    public void AgregarEscudo()
    {
        escudos++;
        textoEscudos.text = escudos.ToString();
    }
}