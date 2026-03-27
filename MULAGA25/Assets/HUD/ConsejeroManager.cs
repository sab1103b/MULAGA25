using UnityEngine;
using TMPro;
using System.Collections;

public class ConsejeroManager : MonoBehaviour
{
    public static ConsejeroManager Instance;

    [Header("Referencias UI")]
    public GameObject panelConsejero;
    public TextMeshProUGUI textoUI;
    public GameObject spriteConsejero;

    [Header("Textos")]
    [TextArea(3,5)]
    public string textoInicio;

    [TextArea(3,5)]
    public string Textochoque;

    [TextArea(3,5)]
    public string textoColeccionable;

    [TextArea(3,5)]
    public string textoBoss;

    [TextArea(3, 5)]
    public string textoarma;

    [TextArea(3, 5)]
    public string textonivel;

    [Header("Configuración")]
    public float velocidadTexto = 0.03f;
    public float duracionMensaje = 5f;

    
    bool yaMostroInicio = false;
    bool yaMostroChoque = false;
    bool yaMostroColeccionable = false;
    bool yaMostroBoss = false;
    bool YaRecogeelarma = false;
    bool Entraalnivel = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        panelConsejero.SetActive(false);
        spriteConsejero.SetActive(false);
    }

    
    public void MostrarMensaje(string mensaje)
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(MostrarMensajeCoroutine(mensaje));
    }

    IEnumerator MostrarMensajeCoroutine(string mensaje)
    {
        panelConsejero.SetActive(true);
        spriteConsejero.SetActive(true);

        textoUI.text = "";

        // EFECTO LETRA POR LETRA
        foreach (char letra in mensaje)
        {
            textoUI.text += letra;
            yield return new WaitForSeconds(velocidadTexto);
        }

        yield return new WaitForSeconds(duracionMensaje);

        panelConsejero.SetActive(false);
        spriteConsejero.SetActive(false);
    }

    public void EventoInicio()
    {
        if (yaMostroInicio) return;

        yaMostroInicio = true;
        MostrarMensaje(textoInicio);
    }

    public void EventoChoque()
    {
        if (yaMostroChoque) return;

        yaMostroChoque = true;
        MostrarMensaje(Textochoque);
    }

    public void EventoColeccionable()
    {
        if (yaMostroColeccionable) return;

        yaMostroColeccionable = true;
        MostrarMensaje(textoColeccionable);
    }

    public void EventoBoss()
    {
        if (yaMostroBoss) return;

        yaMostroBoss = true;
        MostrarMensaje(textoBoss);
    }

    public void EventoRecogeArma()
    {
        if (YaRecogeelarma) return;
        YaRecogeelarma = true;
        MostrarMensaje(textoarma);
    }

    public void EventoEntraNivel()
    {
        if (Entraalnivel) return;
        Entraalnivel = true;
        MostrarMensaje(textonivel);
    }
}