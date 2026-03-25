using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyPool pool;
    public Transform player;

    public Transform[] spawnPoints;

    public void SpawnEnemy()
    {
        GameObject enemy = pool.GetEnemy();

        if (enemy == null) return;

        // Elegir spawnpoint aleatorio
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        enemy.transform.position = spawn.position;
        enemy.transform.rotation = spawn.rotation;

        PATRONES pat = enemy.GetComponent<PATRONES>();

        pat.player = player;
        pat.pattern = (PATRONES.MovementPattern)Random.Range(0, 5);
    }
}