using UnityEngine;
using UnityEngine.InputSystem;

public class LeftHandGrenadeController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform leftGrenadeSocket;
    [SerializeField] private Transform pickupCheckPoint;
    [SerializeField] private GameObject leftControllerVisual;

    [Header("Input")]
    [SerializeField] private InputActionReference pickupAction;
    [SerializeField] private InputActionReference dropAction;
    [SerializeField] private InputActionReference throwAction;

    [Header("Detección")]
    [SerializeField] private float pickupRadius = 0.2f;
    [SerializeField] private LayerMask grenadePickupMask = ~0;

    [Header("Soltar")]
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, -0.05f, 0.20f);
    [SerializeField] private float dropForce = 0.5f;

    [Header("Lanzar")]
    [SerializeField] private float throwForwardOffset = 0.12f;
    [SerializeField] private float throwForce = 8f;
    [SerializeField] private float throwUpForce = 1.5f;
    [SerializeField] private float spinForce = 8f;

    private GrenadePickupItem equippedGrenade;

    private void OnEnable()
    {
        EnableAction(pickupAction);
        EnableAction(dropAction);
        EnableAction(throwAction);
    }

    private void OnDisable()
    {
        DisableAction(pickupAction);
        DisableAction(dropAction);
        DisableAction(throwAction);
    }

    private void Update()
    {
        HandlePickup();
        HandleDrop();
        HandleThrow();
    }

    private void HandlePickup()
    {
        if (equippedGrenade != null) return;
        if (!PressedThisFrame(pickupAction)) return;

        GrenadePickupItem nearest = FindNearestGrenadePickup();
        if (nearest == null) return;

        equippedGrenade = nearest;
        equippedGrenade.EquipTo(leftGrenadeSocket);

        if (leftControllerVisual != null)
            leftControllerVisual.SetActive(false);
    }

    private void HandleDrop()
    {
        if (equippedGrenade == null) return;
        if (!PressedThisFrame(dropAction)) return;

        equippedGrenade.DropFrom(transform, dropOffset, dropForce);
        equippedGrenade = null;

        if (leftControllerVisual != null)
            leftControllerVisual.SetActive(true);
    }

    private void HandleThrow()
    {
        if (equippedGrenade == null) return;
        if (!PressedThisFrame(throwAction)) return;

        equippedGrenade.ThrowFrom(transform, throwForwardOffset, throwForce, throwUpForce, spinForce);
        equippedGrenade = null;

        if (leftControllerVisual != null)
            leftControllerVisual.SetActive(true);
    }

    private GrenadePickupItem FindNearestGrenadePickup()
    {
        if (pickupCheckPoint == null) return null;

        Collider[] hits = Physics.OverlapSphere(
            pickupCheckPoint.position,
            pickupRadius,
            grenadePickupMask,
            QueryTriggerInteraction.Collide
        );

        GrenadePickupItem nearest = null;
        float bestDist = float.MaxValue;

        foreach (Collider hit in hits)
        {
            GrenadePickupItem item = hit.GetComponentInParent<GrenadePickupItem>();
            if (item == null) continue;

            float dist = Vector3.Distance(pickupCheckPoint.position, item.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = item;
            }
        }

        return nearest;
    }

    private bool PressedThisFrame(InputActionReference actionRef)
    {
        return actionRef != null &&
               actionRef.action != null &&
               actionRef.action.WasPressedThisFrame();
    }

    private void EnableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null)
            actionRef.action.Enable();
    }

    private void DisableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null)
            actionRef.action.Disable();
    }

    private void OnDrawGizmosSelected()
    {
        if (pickupCheckPoint == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pickupCheckPoint.position, pickupRadius);
    }
}