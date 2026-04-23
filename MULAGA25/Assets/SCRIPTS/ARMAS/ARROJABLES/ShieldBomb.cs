using System.Collections;
using UnityEngine;

public class ShieldBombItem : ThrowableItem
{
    [Header("Escudo")]
    [SerializeField] private GameObject deployableShieldPrefab;
    [SerializeField] private float fuseTime = 5f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Offset de despliegue")]
    [SerializeField] private float groundOffset = 0.05f;

    private Vector3 lastContactPoint;
    private Vector3 lastContactNormal = Vector3.up;

    protected override float GetFuseTime() => fuseTime;

    protected override void Awake()
    {
        base.Awake();
        explodeOnImpact = true;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown) return;
        if (hasTriggered) return;
        if (Time.time < armedTime) return;
        if (((1 << collision.gameObject.layer) & groundMask) == 0) return;

        ContactPoint contact = collision.GetContact(0);
        lastContactPoint = contact.point;
        lastContactNormal = contact.normal;

        TriggerActivation();
    }

    // ─── ÚNICO FuseRoutine, reemplaza el de la base ───────────
    protected override IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseTime);

        if (!hasTriggered)
        {
            if (Physics.Raycast(transform.position, Vector3.down,
                                out RaycastHit hit, 5f, groundMask))
            {
                lastContactPoint = hit.point;
                lastContactNormal = hit.normal;
            }
            else
            {
                lastContactPoint = transform.position;
                lastContactNormal = Vector3.up;
            }

            TriggerActivation();
        }
    }

    protected override void OnActivate()
    {
        if (deployableShieldPrefab == null)
        {
            Debug.LogWarning("ShieldBombItem: No hay prefab de escudo asignado.");
            Destroy(gameObject);
            return;
        }

        Vector3 spawnPos = lastContactPoint + lastContactNormal * groundOffset;

        GameObject shieldObj = Instantiate(deployableShieldPrefab,
                                           spawnPos, Quaternion.identity);

        DeployableShield shield = shieldObj.GetComponent<DeployableShield>();
        if (shield != null)
            shield.Deploy(spawnPos, lastContactNormal);

        Destroy(gameObject);
    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.15f);
    }
}