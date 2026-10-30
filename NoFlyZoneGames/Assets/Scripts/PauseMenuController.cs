using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private StarterAssetsInputs playerInputs;

    private bool isPaused;
    private float previousTimeScale;
    private bool controllerWasEnabled;
    private bool interactionWasEnabled;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        pausePanel.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        // Using Escape Key to control the pause and resume of game
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        if (isPaused)
        {
            return;
        }

        // Remember the current state so Resume restores it
        previousTimeScale = Time.timeScale;
        controllerWasEnabled = playerController.enabled;
        interactionWasEnabled = playerInteraction.enabled;

        isPaused = true;
        Time.timeScale = 0f;

        playerController.enabled = false;
        playerInteraction.enabled = false;

        SetCursorForMenu(true);
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;
        Time.timeScale = previousTimeScale;

        playerController.enabled = controllerWasEnabled;
        playerInteraction.enabled = interactionWasEnabled;

        pausePanel.SetActive(false);
        SetCursorForMenu(false);
    }

    public void ReturnToMainMenu()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("No Gamemanager found in the scene", this);
            return;
        }

        Time.timeScale = 1f;
        isPaused = false;
        SetCursorForMenu(true);

        GameManager.Instance.OpenMainMenu();
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
        if (isPaused)
        {
            Time.timeScale = previousTimeScale;
        }
    }
}
