using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial_SM : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    [SerializeField] 
    private Transform[] stageSpawnPoints;
    
    [SerializeField]
    Window[] windows;

    [SerializeField]
    GameObject[] tutorialButtons;

    [SerializeField]
    int MainLevelSceneIndex = 2;

    // Keeps track of the next button to activate
    private int currentButtonIndex = 0;

    void Awake()
    {
        // Make sure all buttons start disabled
        foreach (GameObject button in tutorialButtons)
        {
            button.SetActive(false);
        }
    }

    private void OnEnable()
    {
        foreach (Window window in windows)
        {
            window.OnMosquitoPassed += ShowNextButton;
        }
    }

    private void OnDisable()
    {
        foreach (Window window in windows)
        {
            window.OnMosquitoPassed -= ShowNextButton;
        }
    }

    private void ShowNextButton()
    {
        // Make sure there are still buttons to show
        if (currentButtonIndex >= tutorialButtons.Length)
            return;

        // Activate the next button
        tutorialButtons[currentButtonIndex].SetActive(true);

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void NextStage(int index)
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = stageSpawnPoints[index].position;
        player.GetComponent<CharacterController>().enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        tutorialButtons[currentButtonIndex].SetActive(false);

        currentButtonIndex++;
    }

    public void ToMainLevel()
    {
        SceneManager.LoadScene(MainLevelSceneIndex);
    }
}   