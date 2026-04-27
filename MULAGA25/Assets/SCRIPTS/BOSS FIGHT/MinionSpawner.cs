using UnityEngine;
using UnityEngine.AI;

public class MinionSpawner : MonoBehaviour
{
    [Header("Referencias del sistema existente")]
    public EnemyPool pool;
    public Transform player;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("References")]
    public BossArena arena;

    public void SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
            SpawnOneMinion();
    }

    void SpawnOneMinion()
    {
        if (pool == null)
        {
            Debug.LogError("MinionSpawner: pool no asignado.");
            return;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("MinionSpawner: no hay spawn points.");
            return;
        }

        GameObject enemy = pool.GetEnemy();

        if (enemy == null)
        {
            Debug.LogWarning("MinionSpawner: pool vacío.");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        NavMeshHit hit;
        Vector3 spawnPosition = spawnPoint.position + spawnPoint.forward * 0.8f;

        if (NavMesh.SamplePosition(spawnPosition, out hit, 3f, NavMesh.AllAreas))
            spawnPosition = hit.position;
        else
            Debug.LogWarning("MinionSpawner: no encontró NavMesh cerca del spawn point.");

        enemy.SetActive(false);

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        enemy.transform.position = spawnPosition;
        enemy.transform.rotation = spawnPoint.rotation;

        Collider col = enemy.GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        enemy.SetActive(true);

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(spawnPosition);
            agent.isStopped = false;
        }

        MinionAI ai = enemy.GetComponent<MinionAI>();

        if (ai != null)
        {
            ai.arena = arena;
            ai.SetPlayer(player);
            Debug.Log("Minion spawneado y configurado.");
        }
        else
        {
            Debug.LogWarning("MinionSpawner: el enemigo no tiene MinionAI.");
        }
    }
}