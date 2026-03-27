using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class WeaponEquipRightVR : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform rightController;
    [SerializeField] private Transform rightWeaponSocket;
    [SerializeField] private GameObject rightControllerVisual;
    [SerializeField] private FloatingVisual floatingVisual;

    [Header("Puntos del arma")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform gripPoint; // Empty en la empuñadura

    [Header("Input")]
    [SerializeField] private InputActionReference equipAction;

    [Header("Ajustes")]
    [SerializeField] private float equipDistance = 0.35f;
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, -0.05f, 0.25f);
    [SerializeField] private float dropForce = 0.75f;

    [Header("Collider principal del arma")]
    [SerializeField] private Collider mainCollider;

    private Rigidbody rb;
    private bool isEquipped = false;

    // Guardamos escala real en mundo
    private Vector3 originalWorldScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalWorldScale = transform.lossyScale;

        if (mainCollider == null)
            mainCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        bool startsEquipped = (rightWeaponSocket != null && transform.parent == rightWeaponSocket);
        isEquipped = startsEquipped;

        if (floatingVisual != null)
        {
            if (startsEquipped)
                floatingVisual.NotifyPickedUp();
            else
                floatingVisual.NotifyDropped();
        }

        if (rightControllerVisual != null)
            rightControllerVisual.SetActive(!startsEquipped);

        SetWorldScale(originalWorldScale);
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
        ConsejeroManager.Instance.EventoRecogeArma();
        if (floatingVisual != null)
            floatingVisual.NotifyPickedUp();

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

        // Mantener escala visual real
        SetWorldScale(originalWorldScale);

        // Alinear usando el punto de agarre
        AlignUsingGripPoint();

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

        SetWorldScale(originalWorldScale);

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

        if (floatingVisual != null)
            floatingVisual.NotifyDropped();

        if (rightControllerVisual != null)
            rightControllerVisual.SetActive(true);
    }

    private void AlignUsingGripPoint()
    {
        if (gripPoint == null)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            return;
        }

        if (gripPoint.parent != transform)
        {
            Debug.LogWarning("WeaponEquipRightVR: GripPoint debería ser hijo directo del arma.");
        }

        // Queremos que GripPoint coincida exactamente con el socket
        Quaternion targetLocalRotation = Quaternion.Inverse(gripPoint.localRotation);
        Vector3 scaledGripLocalPos = Vector3.Scale(transform.localScale, gripPoint.localPosition);
        Vector3 targetLocalPosition = -(targetLocalRotation * scaledGripLocalPos);

        transform.localRotation = targetLocalRotation;
        transform.localPosition = targetLocalPosition;
    }

    private void SetWorldScale(Vector3 targetWorldScale)
    {
        Transform parent = transform.parent;

        if (parent == null)
        {
            transform.localScale = targetWorldScale;
            return;
        }

        Vector3 parentWorldScale = parent.lossyScale;

        transform.localScale = new Vector3(
            SafeDivide(targetWorldScale.x, parentWorldScale.x),
            SafeDivide(targetWorldScale.y, parentWorldScale.y),
            SafeDivide(targetWorldScale.z, parentWorldScale.z)
        );
    }

    private float SafeDivide(float value, float divisor)
    {
        if (Mathf.Abs(divisor) < 0.0001f)
            return value;

        return value / divisor;
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