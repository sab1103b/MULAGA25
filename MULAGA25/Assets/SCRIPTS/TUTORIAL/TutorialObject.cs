using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TutorialObject : MonoBehaviour
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
        // Si ya completó tutorial, no aparecer
        if (GameProgress.Instance != null &&
            GameProgress.Instance.tutorialCompletado)
        {
            Destroy(gameObject);
        }
    }

    void ObjetoRecogido(SelectEnterEventArgs args)
    {
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.CompletarTutorial();
        }

        Destroy(gameObject);
    }
}