using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using UnityEngine.SceneManagement;
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private StarterAssetsInputs playerInputs;

    [Header("Label")]
    [SerializeField] private GameObject pauseLabel;
    [SerializeField] private GameObject waveCompleteLabel;
    [SerializeField] private GameObject gameOverLabel;

    [Header("Resume Button")]
    [SerializeField] private GameObject resumeButton;

    [Header("HUD")]
    [SerializeField] private GameObject hudCanvas;

    // private bool isPaused;
    private float previousTimeScale;
    private bool controllerWasEnabled;
    private bool interactionWasEnabled;

    private enum MenuState
    {
        Closed,
        Paused,
        GameOver,
        WaveComplete
    }

    private MenuState currentMenuState = MenuState.Closed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        pausePanel.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (currentMenuState == MenuState.GameOver ||
            currentMenuState == MenuState.WaveComplete)
            return;
        
        // Using Escape Key to control the pause and resume of game
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentMenuState == MenuState.Paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void OpenMenu() {
        // Remember the current state so Resume restores it
        previousTimeScale = Time.timeScale;
        controllerWasEnabled = playerController.enabled;
        interactionWasEnabled = playerInteraction.enabled;

        currentMenuState = MenuState.Paused;
        Time.timeScale = 0f;

        playerController.enabled = false;
        playerInteraction.enabled = false;

        SetCursorForMenu(true);
        pausePanel.SetActive(true);

        if (hudCanvas != null)
            hudCanvas.SetActive(false);
    }

    private void PauseGame()
    {
        if (currentMenuState == MenuState.Paused || currentMenuState == MenuState.GameOver)
            return;

        currentMenuState = MenuState.Paused;
        UpdateMenuUI();

        OpenMenu();
    }

    public void ResumeGame()
    {
        if (currentMenuState != MenuState.Paused)
            return;

        currentMenuState = MenuState.Closed;
        Time.timeScale = previousTimeScale;

        playerController.enabled = controllerWasEnabled;
        playerInteraction.enabled = interactionWasEnabled;

        pausePanel.SetActive(false);
        SetCursorForMenu(false);
        
        if (hudCanvas != null)
            hudCanvas.SetActive(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void ReturnToMainMenu()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("No Gamemanager found in the scene", this);
            return;
        }

        Time.timeScale = 1f;
        currentMenuState = MenuState.Closed;
        SetCursorForMenu(true);

        GameManager.Instance.OpenMainMenu();
    }

    public void ShowGameOver()
    {
        if (currentMenuState == MenuState.GameOver)
            return;

        currentMenuState = MenuState.GameOver;
        UpdateMenuUI();

        OpenMenu();
    }

    public void ShowWaveComplete()
    {
        if (currentMenuState == MenuState.GameOver || currentMenuState == MenuState.WaveComplete)
            return;

        currentMenuState = MenuState.WaveComplete;
        UpdateMenuUI();

        OpenMenu();
    }

    private void UpdateMenuUI()
    {
        pauseLabel.SetActive(currentMenuState == MenuState.Paused);
        waveCompleteLabel.SetActive(currentMenuState == MenuState.WaveComplete);
        gameOverLabel.SetActive(currentMenuState == MenuState.GameOver);

        resumeButton.SetActive(currentMenuState == MenuState.Paused);
    }

    private void SetCursorForMenu(bool menuOpen)
    {
        playerInputs.cursorLocked = !menuOpen;
        playerInputs.cursorInputForLook = !menuOpen;
        playerInputs.look = Vector2.zero;

        Cursor.lockState = menuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = menuOpen;
    }

    private void OnDestroy()
    {
        if (currentMenuState == MenuState.Paused)
        {
            Time.timeScale = previousTimeScale;
        }
    }
}
