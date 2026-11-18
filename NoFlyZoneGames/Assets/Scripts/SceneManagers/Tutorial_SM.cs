using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial_SM : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    [SerializeField]
    GameObject spawn1;

    [SerializeField]
    GameObject spawn2;

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

        // Move to the next button
        currentButtonIndex++;
    }

    public void ToPartTwo()
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = spawn1.transform.position;
        player.GetComponent<CharacterController>().enabled = true;
        //Lock and Hide Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        tutorialButtons[currentButtonIndex].SetActive(false);

    }

    public void ToPartThree()
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = spawn2.transform.position;
        player.GetComponent<CharacterController>().enabled = true;
        //Lock and Hide Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        tutorialButtons[currentButtonIndex].SetActive(false);

    }

    public void ToMainLevel()
    {
        SceneManager.LoadScene(MainLevelSceneIndex);
    }
}   