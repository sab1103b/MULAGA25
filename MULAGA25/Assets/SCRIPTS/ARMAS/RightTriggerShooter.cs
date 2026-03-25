using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RightTriggerShooter : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference fireAction; // Asigna Controller/Trigger Press

    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private float spawnOffset = 0.1f;

    [Header("Cooldown")]
    [SerializeField] private int maxShots = 30;
    [SerializeField] private float cooldownTime = 8f;

    [Header("Referencia al arma")]
    [SerializeField] private WeaponEquipRightVR weaponEquip;

    private float nextFireTime = 0f;
    private int shotsFired = 0;
    private bool isCoolingDown = false;

    private void OnEnable()
    {
        if (fireAction != null && fireAction.action != null)
            fireAction.action.Enable();
    }

    private void OnDisable()
    {
        if (fireAction != null && fireAction.action != null)
            fireAction.action.Disable();
    }

    private void Update()
    {
        if (weaponEquip == null)
        {
            Debug.LogWarning("RightTriggerShooter: falta asignar WeaponEquipRightVR");
            return;
        }

        if (!weaponEquip.HasWeapon())
            return;

        if (isCoolingDown)
            return;

        if (fireAction == null || fireAction.action == null)
        {
            Debug.LogWarning("RightTriggerShooter: falta asignar Fire Action");
            return;
        }

        if (Time.time < nextFireTime)
            return;

        if (fireAction.action.WasPressedThisFrame())
        {
            if (shotsFired < maxShots)
            {
                Shoot();
                shotsFired++;
                nextFireTime = Time.time + fireRate;

                if (shotsFired >= maxShots)
                    StartCoroutine(CooldownRoutine());
            }
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("RightTriggerShooter: no hay bulletPrefab asignado");
            return;
        }

        Transform muzzleTransform = weaponEquip.GetMuzzle();

        if (muzzleTransform == null)
        {
            Debug.LogWarning("RightTriggerShooter: no hay muzzle asignado en el arma");
            return;
        }

        Vector3 spawnPos = muzzleTransform.position + muzzleTransform.forward * spawnOffset;
        Quaternion spawnRot = muzzleTransform.rotation;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRot);

        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = muzzleTransform.forward * bulletSpeed;
        }
        else
        {
            Debug.LogWarning("RightTriggerShooter: la bala no tiene Rigidbody");
        }
    }

    private IEnumerator CooldownRoutine()
    {
        if (isCoolingDown) yield break;

        isCoolingDown = true;
        Debug.Log("Cooldown iniciado");

        yield return new WaitForSeconds(cooldownTime);

        shotsFired = 0;
        nextFireTime = 0f;
        isCoolingDown = false;

        Debug.Log("Cooldown terminado");
    }
}