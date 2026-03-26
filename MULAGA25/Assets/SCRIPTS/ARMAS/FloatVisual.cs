using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatingVisual : MonoBehaviour
{
    [Header("Hover sobre superficie")]
    [SerializeField] private float hoverHeight = 0.08f;
    [SerializeField] private float floatAmplitude = 0.015f;
    [SerializeField] private float floatFrequency = 1.5f;
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Activación")]
    [SerializeField] private float activationDelay = 0.25f;
    [SerializeField] private float settleLinearSpeed = 0.15f;
    [SerializeField] private float settleAngularSpeed = 0.15f;

    [Header("Rotación visual")]
    [SerializeField] private float rotationSpeed = 35f;

    private Rigidbody rb;

    private bool effectEnabled = false;
    private bool waitingForSettle = false;
    private bool hoverMode = false;

    private float enableTime;
    private float phaseOffset;
    private float currentYaw;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        phaseOffset = Random.Range(0f, 10f);
        currentYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        if (!effectEnabled)
            return;

        if (hoverMode)
        {
            UpdateHoverMode();
            return;
        }

        if (waitingForSettle)
        {
            TryEnterHoverMode();
        }
    }

    private void TryEnterHoverMode()
    {
        if (rb == null) return;
        if (rb.isKinematic) return;
        if (Time.time < enableTime + activationDelay) return;

        if (rb.linearVelocity.magnitude > settleLinearSpeed) return;
        if (rb.angularVelocity.magnitude > settleAngularSpeed) return;

        if (!TryGetGround(out RaycastHit hit))
            return;

        hoverMode = true;
        waitingForSettle = false;

        currentYaw = transform.eulerAngles.y;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        Vector3 p = transform.position;
        p.y = hit.point.y + hoverHeight;
        transform.position = p;

        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);
    }

    private void UpdateHoverMode()
    {
        if (!TryGetGround(out RaycastHit hit))
        {
            ExitHoverToPhysics();
            return;
        }

        float yOffset = hoverHeight +
                        Mathf.Sin((Time.time + phaseOffset) * floatFrequency * Mathf.PI * 2f) * floatAmplitude;

        Vector3 p = transform.position;
        p.y = hit.point.y + yOffset;
        transform.position = p;

        currentYaw += rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);
    }

    private bool TryGetGround(out RaycastHit bestHit)
    {
        Vector3 origin = transform.position + Vector3.up * 0.3f;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            Vector3.down,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        float bestDistance = float.MaxValue;
        bestHit = default;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.rigidbody == rb) continue;
            if (hit.collider.transform == transform) continue;
            if (hit.collider.transform.IsChildOf(transform)) continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                bestHit = hit;
                found = true;
            }
        }

        return found;
    }

    private void ExitHoverToPhysics()
    {
        hoverMode = false;
        waitingForSettle = true;
        enableTime = Time.time;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    public void NotifyPickedUp()
    {
        effectEnabled = false;
        waitingForSettle = false;
        hoverMode = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public void NotifyDropped()
    {
        effectEnabled = true;
        waitingForSettle = true;
        hoverMode = false;
        enableTime = Time.time;
        currentYaw = transform.eulerAngles.y;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!effectEnabled)
            return;

        if (hoverMode)
        {
            ExitHoverToPhysics();
        }
    }
}