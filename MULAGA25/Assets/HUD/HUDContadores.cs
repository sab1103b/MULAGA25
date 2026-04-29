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
        if (playerModel == null)
            playerModel = FindObjectOfType<PlayerModel>();

        ActualizarHUD();
    }

    void Update()
    {
        ActualizarHUD();
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