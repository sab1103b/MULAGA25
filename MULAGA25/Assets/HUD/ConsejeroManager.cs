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
    // INTRODUCCIÓN
    public AudioClip Audio1; //
    public AudioClip Audio2; //
    public AudioClip Audio3; //
    public AudioClip Audio4; //
    public AudioClip Audio5; //
    // NIVEL
    public AudioClip Audio6; //
    public AudioClip Audio7; //
    public AudioClip Audio8; //
    public AudioClip Audio9; //
    public AudioClip Audio10; //
    public AudioClip Audio11; //
    public AudioClip Audio12; //
    public AudioClip Audio13; //
    public AudioClip Audio14; //
    public AudioClip Audio15; //

    [Header("Textos")]
    [TextArea(3, 5)]
    public string texto01;

    [TextArea(3, 5)]
    public string texto02;

    [TextArea(3, 5)]
    public string texto03;

    [TextArea(3, 5)]
    public string texto04;

    [TextArea(3, 5)]
    public string texto05;

    [TextArea(3, 5)]
    public string texto06;

    [TextArea(3, 5)]
    public string texto07;

    [TextArea(3, 5)]
    public string texto08;

    [TextArea(3, 5)]
    public string texto09;

    [TextArea(3, 5)]
    public string texto10;

    [TextArea(3, 5)]
    public string texto11;

    [TextArea(3, 5)]
    public string texto12;

    [TextArea(3, 5)]
    public string texto13;

    [TextArea(3, 5)]
    public string texto14;

    [TextArea(3, 5)]
    public string texto15;

    [Header("Configuración")]
    public float velocidadTexto = 0.03f;
    public float duracionMensaje = 5f;

    bool yaMostro01 = false;
    bool yaMostro02 = false;
    bool yaMostro03 = false;
    bool yaMostro04 = false;
    bool yaMostro05 = false;
    bool yaMostro06 = false;
    bool yaMostro07 = false;
    bool yaMostro08 = false;
    bool yaMostro09 = false;
    bool yaMostro10 = false;
    bool yaMostro11 = false;
    bool yaMostro12 = false;
    bool yaMostro13 = false;
    bool yaMostro14 = false;
    bool yaMostro15 = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        panelConsejero.SetActive(false);
        spriteConsejero.SetActive(false);

        // BUSCAR AUTOMÁTICAMENTE EL AUDIOSOURCE
        BuscarAudioSource();

        if (vozSource == null)
        {
            Debug.LogWarning("No se encontró AudioSource en la Main Camera");
        }
    }

    // MÉTODO GENERAL

    void BuscarAudioSource()
    {
        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            vozSource = mainCam.GetComponentInChildren<AudioSource>(true);
        }

        if (vozSource == null)
        {
            Debug.LogWarning("No se encontró AudioSource en Main Camera o sus hijos");
        }
    }

    public void MostrarMensaje(string mensaje, AudioClip audio)
    {
        if (vozSource == null)
        {
            BuscarAudioSource();
        }

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
    public void Evento01()
    {
        if (yaMostro01) return;

        yaMostro01 = true;

        MostrarMensaje(texto01, Audio1);
    }
    public void Evento02()
    {
        if (yaMostro02) return;

        yaMostro02 = true;

        MostrarMensaje(texto02, Audio2);
    }
    public void Evento03()
    {
        if (yaMostro03) return;

        yaMostro03 = true;

        MostrarMensaje(texto03, Audio3);
    }
    public void Evento04()
    {
        if (yaMostro04) return;

        yaMostro04 = true;

        MostrarMensaje(texto04, Audio4);
    }
    public void Evento05()
    {
        if (yaMostro05) return;

        yaMostro05 = true;

        MostrarMensaje(texto05, Audio5);
    }
    public void Evento06()
    {
        if (yaMostro06) return;

        yaMostro06 = true;

        MostrarMensaje(texto06, Audio6);
    }
    public void Evento07()
    {
        if (yaMostro07) return;

        yaMostro07 = true;

        MostrarMensaje(texto07, Audio7);
    }

    public void Evento08()
    {
        if (yaMostro08) return;

        yaMostro08 = true;

        MostrarMensaje(texto08, Audio8);
    }

    public void Evento09()
    {
        if (yaMostro09) return;

        yaMostro09 = true;

        MostrarMensaje(texto09, Audio9);
    }

    public void Evento10()
    {
        if (yaMostro10) return;

        yaMostro10 = true;

        MostrarMensaje(texto10, Audio10);
    }

    public void Evento11()
    {
        if (yaMostro11) return;

        yaMostro11 = true;

        MostrarMensaje(texto11, Audio11);
    }

    public void Evento12()
    {
        if (yaMostro12) return;

        yaMostro12 = true;

        MostrarMensaje(texto12, Audio12);
    }

    public void Evento13()
    {
        if (yaMostro13) return;

        yaMostro13 = true;

        MostrarMensaje(texto13, Audio13);
    }

    public void Evento14()
    {
        if (yaMostro14) return;

        yaMostro14 = true;

        MostrarMensaje(texto14, Audio14);
    }

    public void Evento15()
    {
        if (yaMostro15) return;

        yaMostro15 = true;

        MostrarMensaje(texto15, Audio15);
    }
}