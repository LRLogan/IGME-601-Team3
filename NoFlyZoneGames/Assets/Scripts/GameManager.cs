
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // This instance
    private GameObject managerObjInstance;
    private GameManager instance;

    [SerializeField] private string startingSceneName;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            managerObjInstance = gameObject;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Exit the game when Escape is pressed
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            QuitGame();
        }
    }

    /// <summary>
    /// New game option from start menu
    /// </summary>
    public void NewGame()
    {
        SceneManager.LoadScene(startingSceneName);
    }

    /// <summary>
    /// Exit Play Mode in the Editor or quit the built game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();

        // If running in the Unity Editor, stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}