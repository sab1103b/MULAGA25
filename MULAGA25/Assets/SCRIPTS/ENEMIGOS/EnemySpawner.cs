using UnityEngine;

/// <summary>
/// Spawner actualizado que soporta enemigos melee (PATRONES) y ranged (PATRONES_Ranged).
/// Obtiene el enemigo del pool genérico y configura el componente correcto según el tipo.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public EnemyPool pool;
    public Transform player;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Audio")]
    public AudioClip spawnSound;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Spawnea un enemigo con tipo elegido por probabilidad del pool.
    /// </summary>
    public void SpawnEnemy()
    {
        GameObject enemy = pool.GetEnemy();
        if (enemy == null) return;

        PlaceEnemy(enemy);
        ConfigureEnemy(enemy);

        // Sonido de spawn
        if (audioSource != null && spawnSound != null)
            audioSource.PlayOneShot(spawnSound);
    }

    // ── Posicionamiento ───────────────────────────────────────────────────────

    void PlaceEnemy(GameObject enemy)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] No hay spawnPoints asignados.");
            return;
        }

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        enemy.transform.position = spawn.position;
        enemy.transform.rotation = spawn.rotation;
    }

    // ── Configuración de componente ───────────────────────────────────────────

    void ConfigureEnemy(GameObject enemy)
    {
        // ¿Es melee?
        PATRONES melee = enemy.GetComponent<PATRONES>();
        if (melee != null)
        {
            melee.player = player;
            melee.pattern = (PATRONES.MovementPattern)Random.Range(0, 5);
            return;
        }

        // ¿Es ranged?
        PATRONES_Rango ranged = enemy.GetComponent<PATRONES_Rango>();
        if (ranged != null)
        {
            ranged.player = player;
            // El modo de ataque (ráfaga vs disparo rápido) se decide
            // automáticamente en PATRONES_Ranged según la distancia al jugador.
            return;
        }

        Debug.LogWarning($"[EnemySpawner] {enemy.name} no tiene PATRONES ni PATRONES_Ranged.", enemy);
    }
}