using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VRLevelButton : MonoBehaviour
{
    public int levelNumber;
    public string sceneName;

    public StartGame startGame;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();

        ActualizarEstado();
    }

    void Update()
    {
        // Actualiza automáticamente el estado del botón
        ActualizarEstado();
    }

    void ActualizarEstado()
    {
        bool unlocked =
            GameProgress.Instance.IsLevelUnlocked(levelNumber);

        button.interactable = unlocked;
    }

    public void LoadLevel()
    {
        if (!GameProgress.Instance.IsLevelUnlocked(levelNumber))
            return;

        // Enviar escena + número de nivel
        startGame.SetLevel(sceneName, levelNumber);

        // Marcar visualmente como seleccionado
        EventSystem.current.SetSelectedGameObject(gameObject);

        Debug.Log("Nivel seleccionado: " + levelNumber);
    }
}