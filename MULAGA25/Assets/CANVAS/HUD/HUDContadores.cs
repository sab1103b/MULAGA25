using UnityEngine;
using TMPro;

public class HUDContadores : MonoBehaviour
{
    public static HUDContadores Instance;

    public TextMeshProUGUI textoBombas;
    public TextMeshProUGUI textoEscudos;

    [Header("Referencia Jugador")]
    public PlayerModel playerModel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuscarPlayerModel();
        ActualizarHUD();
    }

    void Update()
    {
        // Si cambia de escena o desaparece
        if (playerModel == null)
        {
            BuscarPlayerModel();
        }

        ActualizarHUD();
    }

    void BuscarPlayerModel()
    {
        playerModel = FindAnyObjectByType<PlayerModel>();
    }

    public void ActualizarHUD()
    {
        if (playerModel == null) return;

        if (textoBombas != null)
            textoBombas.text = playerModel.currentGrenades.ToString();

        if (textoEscudos != null)
            textoEscudos.text = playerModel.currentShields.ToString();
    }

    public void AgregarBomba()
    {
        if (playerModel == null) return;

        playerModel.AddGrenade();
        ActualizarHUD();
    }

    public void AgregarEscudo()
    {
        if (playerModel == null) return;

        playerModel.AddShield();
        ActualizarHUD();
    }
}