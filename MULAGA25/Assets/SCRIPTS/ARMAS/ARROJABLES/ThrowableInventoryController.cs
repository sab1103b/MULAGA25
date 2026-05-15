using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowableInventoryController : MonoBehaviour
{
    [Header("Player / HUD")]
    [SerializeField] private PlayerModel playerModel;

    [Header("Sockets")]
    [SerializeField] private Transform throwSocket;
    [SerializeField] private Transform storageSocket;
    [SerializeField] private Transform detectionOrigin;

    [Header("Input")]
    [SerializeField] private InputActionReference grabAction;
    [SerializeField] private InputActionReference cycleAction;
    [SerializeField] private InputActionReference throwAction;

    [Header("Detección")]
    [SerializeField] private float grabRadius = 1.2f;
    [SerializeField] private LayerMask throwableMask = ~0;

    [Header("Lanzamiento")]
    [SerializeField] private float forwardOffset = 0.25f;
    [SerializeField] private float throwForce = 8f;
    [SerializeField] private float throwUpForce = 1.5f;
    [SerializeField] private float spinForce = 8f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private readonly List<ThrowableItem> inventory = new List<ThrowableItem>();

    private int selectedIndex = -1;

    private void Awake()
    {
        if (playerModel == null)
            playerModel = GetComponentInParent<PlayerModel>();

        if (detectionOrigin == null)
            detectionOrigin = transform;

        if (throwSocket == null)
            throwSocket = transform;

        if (storageSocket == null)
        {
            GameObject storage = new GameObject("ThrowableStorageSocket");
            storage.transform.SetParent(transform, false);
            storage.transform.localPosition = Vector3.zero;
            storage.transform.localRotation = Quaternion.identity;
            storage.transform.localScale = Vector3.one;
            storageSocket = storage.transform;
        }
    }

    private void OnEnable()
    {
        if (grabAction != null)
        {
            grabAction.action.performed += OnGrabPressed;
            grabAction.action.Enable();
        }

        if (cycleAction != null)
        {
            cycleAction.action.performed += OnCyclePressed;
            cycleAction.action.Enable();
        }

        if (throwAction != null)
        {
            throwAction.action.performed += OnThrowPressed;
            throwAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (grabAction != null)
            grabAction.action.performed -= OnGrabPressed;

        if (cycleAction != null)
            cycleAction.action.performed -= OnCyclePressed;

        if (throwAction != null)
            throwAction.action.performed -= OnThrowPressed;
    }

    private void OnGrabPressed(InputAction.CallbackContext ctx)
    {
        GrabAndStoreThrowable();
    }

    private void OnCyclePressed(InputAction.CallbackContext ctx)
    {
        CycleSelectedItem();
    }

    private void OnThrowPressed(InputAction.CallbackContext ctx)
    {
        ThrowSelectedItem();
    }

    // ─────────────────────────────────────────────
    // AGARRAR:
    // Se almacena automáticamente.
    // NO se equipa en la mano.
    // También actualiza el PlayerModel/HUD.
    // ─────────────────────────────────────────────
    private void GrabAndStoreThrowable()
    {
        ThrowableItem item = FindClosestThrowable();

        if (item == null)
        {
            if (debugMode)
                Debug.Log("[THROWABLE INVENTORY] No hay objeto cerca para agarrar.");

            return;
        }

        if (inventory.Contains(item))
        {
            if (debugMode)
                Debug.Log("[THROWABLE INVENTORY] Ese objeto ya está almacenado.");

            return;
        }

        // Validar límite antes de guardar
        if (!CanStoreItem(item))
        {
            if (debugMode)
                Debug.Log("[THROWABLE INVENTORY] No se puede almacenar más de este tipo.");

            return;
        }

        inventory.Add(item);

        item.StoreTo(storageSocket);

        AddItemToPlayerModel(item);

        if (selectedIndex == -1)
            selectedIndex = 0;

        if (debugMode)
        {
            Debug.Log(
                "[THROWABLE INVENTORY] Almacenado: " + item.name +
                " | Seleccionado: " + GetSelectedItemName()
            );
        }
    }

    private ThrowableItem FindClosestThrowable()
    {
        Vector3 center = detectionOrigin.position;

        Collider[] hits = Physics.OverlapSphere(
            center,
            grabRadius,
            throwableMask,
            QueryTriggerInteraction.Collide
        );

        ThrowableItem closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            ThrowableItem item = hit.GetComponentInParent<ThrowableItem>();

            if (item == null) continue;

            if (inventory.Contains(item)) continue;

            float distance = Vector3.Distance(center, item.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = item;
            }
        }

        return closest;
    }

    // ─────────────────────────────────────────────
    // CICLAR:
    // Solo cambia el seleccionado.
    // No muestra nada en la mano.
    // ─────────────────────────────────────────────
    private void CycleSelectedItem()
    {
        if (inventory.Count == 0)
        {
            selectedIndex = -1;

            if (debugMode)
                Debug.Log("[THROWABLE INVENTORY] No hay objetos almacenados para ciclar.");

            return;
        }

        selectedIndex++;

        if (selectedIndex >= inventory.Count)
            selectedIndex = 0;

        if (debugMode)
            Debug.Log("[THROWABLE INVENTORY] Seleccionado: " + GetSelectedItemName());
    }

    // ─────────────────────────────────────────────
    // LANZAR:
    // Lanza solo el seleccionado.
    // También actualiza el PlayerModel/HUD.
    // ─────────────────────────────────────────────
    private void ThrowSelectedItem()
    {
        if (inventory.Count == 0 || selectedIndex < 0 || selectedIndex >= inventory.Count)
        {
            if (debugMode)
                Debug.Log("[THROWABLE INVENTORY] No hay objeto seleccionado para lanzar.");

            return;
        }

        ThrowableItem itemToThrow = inventory[selectedIndex];

        if (itemToThrow == null)
        {
            inventory.RemoveAt(selectedIndex);
            FixSelectedIndex();
            return;
        }

        inventory.RemoveAt(selectedIndex);

        RemoveItemFromPlayerModel(itemToThrow);

        itemToThrow.ThrowFrom(
            throwSocket,
            forwardOffset,
            throwForce,
            throwUpForce,
            spinForce
        );

        if (debugMode)
            Debug.Log("[THROWABLE INVENTORY] Lanzado: " + itemToThrow.name);

        FixSelectedIndex();

        if (debugMode)
            Debug.Log("[THROWABLE INVENTORY] Nuevo seleccionado: " + GetSelectedItemName());
    }

    // ─────────────────────────────────────────────
    // HUD / PLAYER MODEL
    // ─────────────────────────────────────────────
    private bool CanStoreItem(ThrowableItem item)
    {
        if (playerModel == null)
            return true;

        if (item is GrenadeItem)
            return playerModel.currentGrenades < playerModel.maxGrenades;

        if (item is ShieldBombItem)
            return playerModel.currentShields < playerModel.maxShields;

        return true;
    }

    private void AddItemToPlayerModel(ThrowableItem item)
    {
        if (playerModel == null)
        {
            Debug.LogWarning("[THROWABLE INVENTORY] No hay PlayerModel asignado. El HUD no se actualizará.");
            return;
        }

        if (item is GrenadeItem)
        {
            playerModel.AddGrenade(1);

            if (debugMode)
                Debug.Log("[HUD] Granada agregada. Total: " + playerModel.currentGrenades);
        }
        else if (item is ShieldBombItem)
        {
            playerModel.AddShield(1);

            if (debugMode)
                Debug.Log("[HUD] Escudo agregado. Total: " + playerModel.currentShields);
        }
    }

    private void RemoveItemFromPlayerModel(ThrowableItem item)
    {
        if (playerModel == null)
            return;

        if (item is GrenadeItem)
        {
            playerModel.UseGrenade(1);

            if (debugMode)
                Debug.Log("[HUD] Granada usada. Total: " + playerModel.currentGrenades);
        }
        else if (item is ShieldBombItem)
        {
            playerModel.UseShield(1);

            if (debugMode)
                Debug.Log("[HUD] Escudo usado. Total: " + playerModel.currentShields);
        }
    }

    private void FixSelectedIndex()
    {
        if (inventory.Count == 0)
        {
            selectedIndex = -1;
            return;
        }

        if (selectedIndex >= inventory.Count)
            selectedIndex = 0;

        if (selectedIndex < 0)
            selectedIndex = 0;
    }

    private string GetSelectedItemName()
    {
        if (inventory.Count == 0)
            return "Ninguno";

        if (selectedIndex < 0 || selectedIndex >= inventory.Count)
            return "Índice inválido";

        if (inventory[selectedIndex] == null)
            return "Null";

        return inventory[selectedIndex].name;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = detectionOrigin != null ? detectionOrigin : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin.position, grabRadius);
    }
}