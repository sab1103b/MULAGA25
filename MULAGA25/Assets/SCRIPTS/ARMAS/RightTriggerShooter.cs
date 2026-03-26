using UnityEngine;
using UnityEngine.InputSystem;

public class RightTriggerShooter : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference fireAction;

    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private float spawnOffset = 0.1f;
    [SerializeField] private bool automaticFire = true; // true = mantener gatillo, false = un disparo por pulsación

    [Header("Sobrecalentamiento")]
    [SerializeField] private int maxShots = 20;          // 100% = 20 disparos
    [SerializeField] private float cooldownTime = 2f;   // tiempo para volver de 100% a 0%

    [Header("Referencia al arma")]
    [SerializeField] private WeaponEquipRightVR weaponEquip;

    private float nextFireTime = 0f;

    // 0 = sin calor, maxShots = totalmente sobrecalentado
    private float currentHeat = 0f;

    private float CoolPerSecond
    {
        get
        {
            if (cooldownTime <= 0f) return maxShots;
            return maxShots / cooldownTime;
        }
    }

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
        bool hasWeapon = weaponEquip != null && weaponEquip.HasWeapon();
        bool hasInput = fireAction != null && fireAction.action != null;

        bool wantsToFire = false;

        if (hasWeapon && hasInput)
        {
            wantsToFire = automaticFire
                ? fireAction.action.IsPressed()
                : fireAction.action.WasPressedThisFrame();
        }

        // Enfriamiento progresivo cuando NO está disparando
        if (!wantsToFire && currentHeat > 0f)
        {
            currentHeat -= CoolPerSecond * Time.deltaTime;
            if (currentHeat < 0f)
                currentHeat = 0f;
        }

        if (!hasWeapon || !hasInput)
            return;

        if (!wantsToFire)
            return;

        if (Time.time < nextFireTime)
            return;

        // Si ya está al máximo de calor, no puede disparar
        if (currentHeat + 1f > maxShots)
            return;

        bool shotSuccess = Shoot();
        if (!shotSuccess)
            return;

        currentHeat += 1f;
        nextFireTime = Time.time + fireRate;
    }

    private bool Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("RightTriggerShooter: no hay bulletPrefab asignado");
            return false;
        }

        if (weaponEquip == null)
        {
            Debug.LogWarning("RightTriggerShooter: falta asignar WeaponEquipRightVR");
            return false;
        }

        Transform muzzleTransform = weaponEquip.GetMuzzle();

        if (muzzleTransform == null)
        {
            Debug.LogWarning("RightTriggerShooter: no hay muzzle asignado en el arma");
            return false;
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

        return true;
    }

    // Balas disponibles actualmente
    public int GetAvailableShots()
    {
        return Mathf.Clamp(Mathf.FloorToInt(maxShots - currentHeat), 0, maxShots);
    }

    // Porcentaje de sobrecalentamiento (0 a 1)
    public float GetOverheatPercent()
    {
        if (maxShots <= 0) return 0f;
        return currentHeat / maxShots;
    }

    // Porcentaje en formato 0 a 100
    public float GetOverheatPercent100()
    {
        return GetOverheatPercent() * 100f;
    }
}