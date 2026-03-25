using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FolletoCollect : MonoBehaviour
{
    XRGrabInteractable grab;
    FolletoItem item;

    void Awake()    
    {
        grab = GetComponent<XRGrabInteractable>();
        item = GetComponent<FolletoItem>();
    }

    void OnEnable()
    {
        if (grab != null)
            grab.selectEntered.AddListener(CollectFolleto);
    }

    void OnDisable()
    {
        if (grab != null)
            grab.selectEntered.RemoveListener(CollectFolleto);
    }

    void CollectFolleto(SelectEnterEventArgs args)
    {
        if (CollectionManager.instance != null && item != null)
        {
            CollectionManager.instance.AddFolleto(item.folletoID);
            Destroy(gameObject);
        }
    }
}