
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // This instance
    private GameObject managerObjInstance;
    private static GameManager instance;

    // Connect Title screen, menu and game
    [SerializeField] private string titleSceneName;
    [SerializeField] private string mainMenuSceneName;
    [SerializeField] private string gameplaySceneName;


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

    public void OpenMainMenu()
    {
        LoadScene(mainMenuSceneName);
    }

    public void StartGame()
    {
        LoadScene(gameplaySceneName);
    }

    public void ReturnToTitle()
    {
        LoadScene(titleSceneName);
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Assign a destination scene in GameManager.", this);
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning($"Scene '{sceneName}' cannot be loaded.", this);
            return;
        }
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void OnDestory()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// New game option from start menu
    /// </summary>
    public void NewGame()
    {
        StartGame();
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