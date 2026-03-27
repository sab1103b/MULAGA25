using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeftHandGrenadeController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform leftGrenadeSocket;
    [SerializeField] private Transform grenadeStorageSocket;
    [SerializeField] private Transform pickupCheckPoint;
    [SerializeField] private GameObject leftControllerVisual;

    [Header("Input")]
    [SerializeField] private InputActionReference pickupAction;       // Recoger y guardar
    [SerializeField] private InputActionReference useGrenadeAction;   // Si no hay granada en mano: equipa. Si ya hay: lanza.

    [Header("Inventario")]
    [SerializeField] private int maxStoredGrenades = 3;
    [SerializeField] private bool useLastStoredFirst = true; // true = última en entrar, primera en salir

    [Header("Detección")]
    [SerializeField] private float pickupRadius = 0.2f;
    [SerializeField] private LayerMask grenadePickupMask = ~0;

    [Header("Lanzar")]
    [SerializeField] private float throwForwardOffset = 0.12f;
    [SerializeField] private float throwForce = 8f;
    [SerializeField] private float throwUpForce = 1.5f;
    [SerializeField] private float spinForce = 8f;

    private readonly List<GrenadePickupItem> storedGrenades = new List<GrenadePickupItem>();
    private GrenadePickupItem equippedGrenade;

    private void Start()
    {
        UpdateControllerVisual();
    }

    private void OnEnable()
    {
        EnableAction(pickupAction);
        EnableAction(useGrenadeAction);
    }

    private void OnDisable()
    {
        DisableAction(pickupAction);
        DisableAction(useGrenadeAction);
    }

    private void Update()
    {
        CleanupStoredGrenades();

        HandlePickupAndStore();
        HandleUseGrenade();
    }

    private void HandlePickupAndStore()
    {
        if (!PressedThisFrame(pickupAction)) return;
        if (equippedGrenade != null) return;
        if (storedGrenades.Count >= maxStoredGrenades) return;

        GrenadePickupItem nearest = FindNearestGrenadePickup();
        if (nearest == null) return;
        if (storedGrenades.Contains(nearest)) return;

        storedGrenades.Add(nearest);
        nearest.StoreTo(GetStorageSocket());

        UpdateControllerVisual();

        Debug.Log("Granadas guardadas: " + storedGrenades.Count + "/" + maxStoredGrenades);
    }

    private void HandleUseGrenade()
    {
        if (!PressedThisFrame(useGrenadeAction)) return;

        // Si ya hay una granada en la mano, este mismo botón la lanza.
        if (equippedGrenade != null)
        {
            equippedGrenade.ThrowFrom(transform, throwForwardOffset, throwForce, throwUpForce, spinForce);
            equippedGrenade = null;

            UpdateControllerVisual();

            Debug.Log("Granada lanzada. Restantes: " + storedGrenades.Count + "/" + maxStoredGrenades);
            return;
        }

        // Si no hay granada en mano, este botón saca una del inventario.
        if (storedGrenades.Count == 0) return;
        if (leftGrenadeSocket == null) return;

        int index = useLastStoredFirst ? storedGrenades.Count - 1 : 0;

        equippedGrenade = storedGrenades[index];
        storedGrenades.RemoveAt(index);

        if (equippedGrenade == null)
        {
            equippedGrenade = null;
            return;
        }

        equippedGrenade.EquipTo(leftGrenadeSocket);

        UpdateControllerVisual();

        Debug.Log("Granada equipada. Restantes en inventario: " + storedGrenades.Count + "/" + maxStoredGrenades);
    }

    private Transform GetStorageSocket()
    {
        return grenadeStorageSocket != null ? grenadeStorageSocket : transform;
    }

    private void UpdateControllerVisual()
    {
        // Si hay granada visible en la mano, ocultamos el control.
        if (leftControllerVisual != null)
            leftControllerVisual.SetActive(equippedGrenade == null);
    }

    private void CleanupStoredGrenades()
    {
        for (int i = storedGrenades.Count - 1; i >= 0; i--)
        {
            if (storedGrenades[i] == null)
                storedGrenades.RemoveAt(i);
        }
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
            if (item == equippedGrenade) continue;
            if (storedGrenades.Contains(item)) continue;

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

    public bool HasStoredGrenades()
    {
        return storedGrenades.Count > 0;
    }

    public bool HasGrenadeInHand()
    {
        return equippedGrenade != null;
    }

    public int GetStoredGrenadeCount()
    {
        return storedGrenades.Count;
    }

    public int GetMaxStoredGrenades()
    {
        return maxStoredGrenades;
    }

    private void OnDrawGizmosSelected()
    {
        if (pickupCheckPoint == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pickupCheckPoint.position, pickupRadius);
    }
}