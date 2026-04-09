using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner spawner;

    [Header("Spawn Settings")]
    public int enemiesToSpawn = 5;
    public float delayBetweenSpawns = 0.5f;

    [Header("Behavior")]
    public bool spawnOnlyOnce = true;

    private bool hasSpawned = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("MainCamera")) return;

        if (spawnOnlyOnce && hasSpawned) return;

        hasSpawned = true;
        StartCoroutine(SpawnEnemies());
    }

    private System.Collections.IEnumerator SpawnEnemies()
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            spawner.SpawnEnemy();
            yield return new WaitForSeconds(delayBetweenSpawns);
        }
    }
}