using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeftHandThrowableController : MonoBehaviour
{
    // ── Modo actual ────────────────────────────────────────────
    public enum ThrowMode { Grenade, Shield }
    [SerializeField] private PlayerModel playerModel;

    [Header("─── Modo actual ───────────────────────────────────")]
    [SerializeField] private ThrowMode currentMode = ThrowMode.Grenade;

    // ── Referencias ────────────────────────────────────────────
    [Header("─── Sockets ─────────────────────────────────────────")]
    [SerializeField] private Transform leftHandSocket;          // Donde aparece el objeto en la mano
    [SerializeField] private Transform grenadeStorageSocket;    // Donde se guardan visualmente las granadas
    [SerializeField] private Transform shieldStorageSocket;     // Donde se guardan visualmente los escudos
    [SerializeField] private Transform pickupCheckPoint;        // Centro del overlap de recogida
    [SerializeField] private GameObject leftControllerVisual;   // Mesh del control izquierdo

    // ── Input ──────────────────────────────────────────────────
    [Header("─── Input ───────────────────────────────────────────")]
    [SerializeField] private InputActionReference gripAction;       // Recoger objeto (Grip)
    [SerializeField] private InputActionReference triggerAction;    // Equipar / Lanzar (Trigger)
    [SerializeField] private InputActionReference cycleModeAction;  // Ciclar modo (Botón Y)

    // ── Inventario ─────────────────────────────────────────────
    [Header("─── Inventario ─────────────────────────────────────")]
    [SerializeField] private int maxStoredGrenades = 3;
    [SerializeField] private int maxStoredShields = 3;
    [SerializeField] private bool useLastStoredFirst = true;

    // ── Detección de recogida ──────────────────────────────────
    [Header("─── Pickup ──────────────────────────────────────────")]
    [SerializeField] private float pickupRadius = 0.25f;
    [SerializeField] private LayerMask pickupMask = ~0;

    // ── Parámetros de lanzamiento ──────────────────────────────
    [Header("─── Lanzamiento ─────────────────────────────────────")]
    [SerializeField] private float throwForwardOffset = 0.12f;
    [SerializeField] private float throwForce = 8f;
    [SerializeField] private float throwUpForce = 1.5f;
    [SerializeField] private float spinForce = 8f;

    // ── UI / Feedback ──────────────────────────────────────────
    [Header("─── UI Feedback (opcional) ──────────────────────────")]
    [SerializeField] private TMPro.TextMeshProUGUI modeIndicatorText; // Texto que muestra el modo
    [SerializeField] private GameObject grenadeIndicatorUI;
    [SerializeField] private GameObject shieldIndicatorUI;

    // ── Estado interno ─────────────────────────────────────────
    private readonly List<GrenadeItem> storedGrenades = new List<GrenadeItem>();
    private readonly List<ShieldBombItem> storedShields = new List<ShieldBombItem>();

    private ThrowableItem equippedItem;   // El item actualmente en la mano

    // ══════════════════════════════════════════════════════════════
    //   Unity Lifecycle
    // ══════════════════════════════════════════════════════════════

    private void Start()
    {
        RefreshUI();
        UpdateControllerVisual();

        if (playerModel == null)
            playerModel = FindObjectOfType<PlayerModel>();
    }

    private void OnEnable()
    {
        EnableAction(gripAction);
        EnableAction(triggerAction);
        EnableAction(cycleModeAction);
    }

    private void OnDisable()
    {
        DisableAction(gripAction);
        DisableAction(triggerAction);
        DisableAction(cycleModeAction);
    }

    private void Update()
    {
        CleanupLists();
        HandleCycleMode();
        HandleGrip();
        HandleTrigger();
    }

    // ══════════════════════════════════════════════════════════════
    //   Input Handlers
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Botón Y: Cicla entre Grenade y Shield.
    /// Solo funciona si NO hay un objeto en la mano.
    /// </summary>
    private void HandleCycleMode()
    {
        if (!PressedThisFrame(cycleModeAction)) return;
        if (equippedItem != null) return; // No cambia de modo con algo en la mano

        currentMode = currentMode == ThrowMode.Grenade
            ? ThrowMode.Shield
            : ThrowMode.Grenade;

        RefreshUI();
        Debug.Log($"[ThrowController] Modo cambiado a: {currentMode}");
    }

    /// <summary>
    /// Grip: Recoge el objeto más cercano del tipo del modo actual.
    /// </summary>
    private void HandleGrip()
    {
        if (!PressedThisFrame(gripAction)) return;
        if (equippedItem != null) return; // No recoge si ya tiene algo en mano

        switch (currentMode)
        {
            case ThrowMode.Grenade:
                if (playerModel.currentGrenades >= playerModel.maxGrenades) return;
                TryPickup<GrenadeItem>(storedGrenades, maxStoredGrenades, grenadeStorageSocket);
                break;

            case ThrowMode.Shield:
                if (playerModel.currentShields >= playerModel.maxShields) return;
                TryPickup<ShieldBombItem>(storedShields, maxStoredShields, shieldStorageSocket);
                break;
        }

        RefreshUI();
    }

    /// <summary>
    /// Trigger:
    ///   - Si hay item en mano → lanza.
    ///   - Si no hay item en mano → equipa uno del inventario (según modo).
    /// </summary>
    private void HandleTrigger()
    {
        if (!PressedThisFrame(triggerAction)) return;

        // ── Si hay algo en la mano, lanzar ──
        if (equippedItem != null)
        {
            equippedItem.ThrowFrom(
                leftHandSocket != null ? leftHandSocket : transform,
                throwForwardOffset, throwForce, throwUpForce, spinForce
            );
            equippedItem = null;
            UpdateControllerVisual();
            RefreshUI();
            return;
        }

        // ── Si no hay nada, equipar según modo ──
        switch (currentMode)
        {
            case ThrowMode.Grenade:
                EquipFromList(storedGrenades);
                break;
            case ThrowMode.Shield:
                EquipFromList(storedShields);
                break;
        }

        UpdateControllerVisual();
        RefreshUI();
    }

    // ══════════════════════════════════════════════════════════════
    //   Lógica de Pickup / Equip
    // ══════════════════════════════════════════════════════════════

    private void TryPickup<T>(List<T> list, int max, Transform storageSocket)
        where T : ThrowableItem
    {
        if (list.Count >= max) return;

        T nearest = FindNearest<T>();
        if (nearest == null) return;
        if (list.Contains(nearest)) return;

        list.Add(nearest);

        if (typeof(T) == typeof(GrenadeItem))
            playerModel.AddGrenade();

        if (typeof(T) == typeof(ShieldBombItem))
            playerModel.AddShield();

        nearest.StoreTo(storageSocket != null ? storageSocket : transform);

        Debug.Log($"[ThrowController] Recogido {typeof(T).Name}. " +
                  $"Guardados: {list.Count}/{max}");
    }

    private void EquipFromList<T>(List<T> list) where T : ThrowableItem
    {
        if (list.Count == 0) return;
        if (leftHandSocket == null) return;

        int index = useLastStoredFirst ? list.Count - 1 : 0;
        T item = list[index];
        list.RemoveAt(index);

        if (typeof(T) == typeof(GrenadeItem))
            playerModel.UseGrenade();

        if (typeof(T) == typeof(ShieldBombItem))
            playerModel.UseShield();

        if (item == null) return;

        equippedItem = item;
        equippedItem.EquipTo(leftHandSocket);

        Debug.Log($"[ThrowController] Equipado {typeof(T).Name}. " +
                  $"Restantes: {list.Count}");
    }

    private T FindNearest<T>() where T : ThrowableItem
    {
        if (pickupCheckPoint == null) return null;

        Collider[] hits = Physics.OverlapSphere(
            pickupCheckPoint.position, pickupRadius, pickupMask,
            QueryTriggerInteraction.Collide);

        T nearest = null;
        float bestDist = float.MaxValue;

        foreach (Collider hit in hits)
        {
            T item = hit.GetComponentInParent<T>();
            if (item == null) continue;
            if ((ThrowableItem)item == equippedItem) continue;

            // Evita duplicados en listas
            if (item is GrenadeItem gi && storedGrenades.Contains(gi)) continue;
            if (item is ShieldBombItem si && storedShields.Contains(si)) continue;

            float dist = Vector3.Distance(pickupCheckPoint.position, item.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = item;
            }
        }

        return nearest;
    }

    // ══════════════════════════════════════════════════════════════
    //   Mantenimiento de listas
    // ══════════════════════════════════════════════════════════════

    private void CleanupLists()
    {
        for (int i = storedGrenades.Count - 1; i >= 0; i--)
            if (storedGrenades[i] == null) storedGrenades.RemoveAt(i);

        for (int i = storedShields.Count - 1; i >= 0; i--)
            if (storedShields[i] == null) storedShields.RemoveAt(i);

        if (equippedItem != null && equippedItem.gameObject == null)
            equippedItem = null;
    }

    // ══════════════════════════════════════════════════════════════
    //   UI / Visual
    // ══════════════════════════════════════════════════════════════

    private void UpdateControllerVisual()
    {
        if (leftControllerVisual != null)
            leftControllerVisual.SetActive(equippedItem == null);
    }

    private void RefreshUI()
    {
        if (modeIndicatorText != null)
        {
            string icon = currentMode == ThrowMode.Grenade ? "💣" : "🛡";
            int count = currentMode == ThrowMode.Grenade
                ? storedGrenades.Count
                : storedShields.Count;
            int max = currentMode == ThrowMode.Grenade
                ? maxStoredGrenades
                : maxStoredShields;

            modeIndicatorText.text = $"{icon} {currentMode}  [{count}/{max}]";
        }

        if (grenadeIndicatorUI != null)
            grenadeIndicatorUI.SetActive(currentMode == ThrowMode.Grenade);
        if (shieldIndicatorUI != null)
            shieldIndicatorUI.SetActive(currentMode == ThrowMode.Shield);
    }

    // ══════════════════════════════════════════════════════════════
    //   API Pública
    // ══════════════════════════════════════════════════════════════

    public ThrowMode GetCurrentMode() => currentMode;
    public bool HasItemInHand() => equippedItem != null;
    public int GetStoredGrenades() => storedGrenades.Count;
    public int GetStoredShields() => storedShields.Count;

    // ══════════════════════════════════════════════════════════════
    //   Helpers Input
    // ══════════════════════════════════════════════════════════════

    private bool PressedThisFrame(InputActionReference r) =>
        r != null && r.action != null && r.action.WasPressedThisFrame();

    private void EnableAction(InputActionReference r)
    {
        if (r != null && r.action != null) r.action.Enable();
    }

    private void DisableAction(InputActionReference r)
    {
        if (r != null && r.action != null) r.action.Disable();
    }

    // ══════════════════════════════════════════════════════════════
    //   Gizmos
    // ══════════════════════════════════════════════════════════════

    private void OnDrawGizmosSelected()
    {
        if (pickupCheckPoint == null) return;
        Gizmos.color = currentMode == ThrowMode.Grenade ? Color.yellow : Color.cyan;
        Gizmos.DrawWireSphere(pickupCheckPoint.position, pickupRadius);
    }
}