using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Nivel1Object : MonoBehaviour
{
    XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        if (grab != null)
            grab.selectEntered.AddListener(ObjetoRecogido);
    }

    void OnDisable()
    {
        if (grab != null)
            grab.selectEntered.RemoveListener(ObjetoRecogido);
    }

    void Start()
    {
        // Si ya lo recogió, no aparece en nivel
        if (GameProgress.Instance != null &&
            GameProgress.Instance.nivel1ObjetoRecogido)
        {
            Destroy(gameObject);
        }
    }

    void ObjetoRecogido(SelectEnterEventArgs args)
    {
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.RecogerNivel1();
            GameProgress.Instance.UnlockLevel(2);
        }

        Destroy(gameObject);
    }
}