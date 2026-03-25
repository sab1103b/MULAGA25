using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Systems")]
    public EnemyController enemyController;
    public CoverSystem coverController;

    [Header("Game State")]
    public bool enemiesActive = true;
    public bool coversActive = true;

    void Start()
    {
        ApplyState();
    }

    void ApplyState()
    {
        if (enemyController != null)
            enemyController.enabled = enemiesActive;

        if (coverController != null)
            coverController.enabled = coversActive;
    }
}

