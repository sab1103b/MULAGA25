using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner spawner;

    [Header("Enemy Settings")]
    public int maxEnemies = 10;
    public float spawnInterval = 2f;

    private float timer;
    private int currentEnemies;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentEnemies < maxEnemies)
        {
            spawner.SpawnEnemy();
            currentEnemies++;
            timer = 0;
        }
    }

    public void EnemyDied()
    {
        currentEnemies--;
    }
}