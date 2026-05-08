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

    [Header("Audio")]
    private AudioSource vozSource;

    [Header("Clips de Voz")]
    public AudioClip audioInicio;
    public AudioClip audioChoque;
    public AudioClip audioColeccionable;
    public AudioClip audioBoss;
    public AudioClip audioArma;
    public AudioClip audioNivel;

    [Header("Textos")]
    [TextArea(3, 5)]
    public string textoInicio;

    [TextArea(3, 5)]
    public string Textochoque;

    [TextArea(3, 5)]
    public string textoColeccionable;

    [TextArea(3, 5)]
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

        // BUSCAR AUTOMÁTICAMENTE EL AUDIOSOURCE
        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            vozSource = mainCam.GetComponent<AudioSource>();
        }

        if (vozSource == null)
        {
            Debug.LogWarning("No se encontró AudioSource en la Main Camera");
        }
    }

    // MÉTODO GENERAL
   
    public void MostrarMensaje(string mensaje, AudioClip audio)
    {
        StopAllCoroutines();

        // REPRODUCIR AUDIO
        if (vozSource != null)
        {
            if (vozSource.isPlaying)
            {
                vozSource.Stop();
            }

            if (audio != null)
            {
                vozSource.clip = audio;
                vozSource.Play();
            }
        }

        gameObject.SetActive(true);

        StartCoroutine(MostrarMensajeCoroutine(mensaje, audio));
    }

    // COROUTINE PRINCIPAL

    IEnumerator MostrarMensajeCoroutine(string mensaje, AudioClip audio)
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

        // ESPERAR AL AUDIO
        if (audio != null)
        {
            yield return new WaitForSeconds(audio.length);
        }
        else
        {
            yield return new WaitForSeconds(duracionMensaje);
        }

        panelConsejero.SetActive(false);
        spriteConsejero.SetActive(false);
    }

    // EVENTOS

    public void EventoInicio()
    {
        if (yaMostroInicio) return;

        yaMostroInicio = true;

        MostrarMensaje(textoInicio, audioInicio);
    }

    public void EventoChoque()
    {
        if (yaMostroChoque) return;

        yaMostroChoque = true;

        MostrarMensaje(Textochoque, audioChoque);
    }

    public void EventoColeccionable()
    {
        if (yaMostroColeccionable) return;

        yaMostroColeccionable = true;

        MostrarMensaje(textoColeccionable, audioColeccionable);
    }

    public void EventoBoss()
    {
        if (yaMostroBoss) return;

        yaMostroBoss = true;

        MostrarMensaje(textoBoss, audioBoss);
    }

    public void EventoRecogeArma()
    {
        if (YaRecogeelarma) return;

        YaRecogeelarma = true;

        MostrarMensaje(textoarma, audioArma);
    }

    public void EventoEntraNivel()
    {
        if (Entraalnivel) return;

        Entraalnivel = true;

        MostrarMensaje(textonivel, audioNivel);
    }
}