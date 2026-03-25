using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class WeaponEquipRightVR : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform rightController;
    [SerializeField] private Transform rightWeaponSocket;
    [SerializeField] private GameObject rightControllerVisual;

    [Header("Input")]
    [SerializeField] private InputActionReference equipAction;

    [Header("Disparo")]
    [SerializeField] private Transform muzzle;

    [Header("Ajustes")]
    [SerializeField] private float equipDistance = 0.35f;
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, -0.05f, 0.25f);
    [SerializeField] private float dropForce = 0.75f;

    [Header("Collider principal del arma")]
    [SerializeField] private Collider mainCollider;

    private Rigidbody rb;
    private bool isEquipped = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (equipAction != null && equipAction.action != null)
            equipAction.action.Enable();
    }

    private void OnDisable()
    {
        if (equipAction != null && equipAction.action != null)
            equipAction.action.Disable();
    }

    private void Update()
    {
        HandleEquipToggle();
    }

    private void HandleEquipToggle()
    {
        if (equipAction == null || equipAction.action == null) return;
        if (!equipAction.action.WasPressedThisFrame()) return;
        if (rightController == null || rightWeaponSocket == null) return;

        if (isEquipped)
        {
            UnequipWeapon();
            return;
        }

        float distance = Vector3.Distance(transform.position, rightController.position);
        if (distance <= equipDistance)
        {
            EquipWeapon();
        }
    }

    private void EquipWeapon()
    {
        isEquipped = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        if (mainCollider != null)
            mainCollider.enabled = false;

        transform.SetParent(rightWeaponSocket, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        if (rightControllerVisual != null)
            rightControllerVisual.SetActive(false);
    }

    private void UnequipWeapon()
    {
        isEquipped = false;

        transform.SetParent(null, true);

        if (rightController != null)
        {
            transform.position = rightController.TransformPoint(dropOffset);
            transform.rotation = rightController.rotation;
        }

        if (mainCollider != null)
            mainCollider.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.detectCollisions = true;

            if (rightController != null)
                rb.linearVelocity = rightController.forward * dropForce;
            else
                rb.linearVelocity = Vector3.zero;

            rb.angularVelocity = Vector3.zero;
        }

        if (rightControllerVisual != null)
            rightControllerVisual.SetActive(true);
    }

public bool HasWeapon()
{
    return isEquipped;
}

public Transform GetMuzzle()
{
    return muzzle;
}
}