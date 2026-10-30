
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject gameTitle;
    [SerializeField] private GameObject menuButtons;

    [Header("Settings")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        BackToMainMenu();
        AudioListener.volume = volumeSlider.value;
    }

    public void OpenSettings()
    {
        gameTitle.SetActive(false);
        menuButtons.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        settingsPanel.SetActive(false);
        gameTitle.SetActive(true);
        menuButtons.SetActive(true);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void PlayGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("No GameManager found. Start from TitleScene.", this);
            return;
        }

        GameManager.Instance.StartGame();
    }

    public void QuitGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("No GameManager found. Start from TitleScene.", this);
            return;
        }

        GameManager.Instance.QuitGame();
    }
}
