using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Pool genérico con dos tipos de enemigo: melee y ranged.
/// La probabilidad de cada tipo se controla con <see cref="meleeProbability"/>.
/// </summary>
public class EnemyPool : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject meleePrefab;
    public GameObject rangedPrefab;

    [Header("Pool Sizes")]
    public int meleePoolSize = 8;
    public int rangedPoolSize = 5;

    [Header("Spawn Probability")]
    [Range(0f, 1f)]
    [Tooltip("Probabilidad de spawnear un enemigo melee (0 = siempre ranged, 1 = siempre melee)")]
    public float meleeProbability = 0.6f;

    // ── Pools internos ────────────────────────────────────────────────────────
    private List<GameObject> meleePool = new List<GameObject>();
    private List<GameObject> rangedPool = new List<GameObject>();

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        // Crear objetos inactivos al arrancar
        for (int i = 0; i < meleePoolSize; i++)
        {
            GameObject obj = Instantiate(meleePrefab, transform);
            obj.SetActive(false);
            meleePool.Add(obj);
        }

        for (int i = 0; i < rangedPoolSize; i++)
        {
            GameObject obj = Instantiate(rangedPrefab, transform);
            obj.SetActive(false);
            rangedPool.Add(obj);
        }
    }

    /// <summary>
    /// Devuelve un enemigo inactivo del pool elegido según probabilidad.
    /// Retorna null si ambos pools están llenos.
    /// </summary>
    public GameObject GetEnemy()
    {
        // Decidir tipo
        bool tryMeleeFirst = Random.value <= meleeProbability;

        GameObject enemy = tryMeleeFirst
            ? GetFromPool(meleePool) ?? GetFromPool(rangedPool)
            : GetFromPool(rangedPool) ?? GetFromPool(meleePool);

        if (enemy != null)
            enemy.SetActive(true);
        else
            Debug.LogWarning("[EnemyPool] Ambos pools están llenos.");

        return enemy;
    }

    /// <summary>
    /// Overload explícito: pide un tipo concreto.
    /// Cae en el pool del otro tipo si el solicitado está lleno.
    /// </summary>
    public GameObject GetEnemy(bool forceMelee)
    {
        GameObject enemy = forceMelee
            ? GetFromPool(meleePool) ?? GetFromPool(rangedPool)
            : GetFromPool(rangedPool) ?? GetFromPool(meleePool);

        if (enemy != null)
            enemy.SetActive(true);

        return enemy;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private GameObject GetFromPool(List<GameObject> pool)
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }
        return null;
    }
}