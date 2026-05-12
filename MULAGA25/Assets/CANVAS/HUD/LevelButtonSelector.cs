using UnityEngine;
using UnityEngine.UI;

public class LevelButtonSelector : MonoBehaviour
{
    [Header("Botones")]
    public Button[] botones;

    [Header("Colores")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.blue;

    private Button botonActual;

    void Start()
    {
        // Dejar todos normales al iniciar
        foreach (Button b in botones)
        {
            SetButtonColor(b, normalColor);
        }
    }

    public void SeleccionarBoton(Button botonSeleccionado)
    {
        // Resetear todos
        foreach (Button b in botones)
        {
            SetButtonColor(b, normalColor);
        }

        // Activar seleccionado
        SetButtonColor(botonSeleccionado, selectedColor);

        botonActual = botonSeleccionado;
    }

    void SetButtonColor(Button boton, Color color)
    {
        ColorBlock colors = boton.colors;

        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.selectedColor = color;
        colors.pressedColor = color;

        boton.colors = colors;
    }
}