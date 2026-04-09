using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsLevelUnlocked(int level)
    {
        if (level == 1) return true;
        return PlayerPrefs.GetInt("Level_" + level, 0) == 1;
    }

    public void UnlockLevel(int level)
    {
        PlayerPrefs.SetInt("Level_" + level, 1);
        PlayerPrefs.Save();
    }
}