using UnityEngine;
using UnityEngine.SceneManagement;

public class VRLevelButton : MonoBehaviour
{
    public int levelNumber;
    public string sceneName;

    public void LoadLevel()
    {
        if (LevelManager.instance.IsLevelUnlocked(levelNumber))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Nivel bloqueado");
        }
    }
}