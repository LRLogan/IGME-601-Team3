using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial_SM : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    [Header("Spawns")]
    [SerializeField] private Transform[] stageSpawnPoints;
    
    [SerializeField]
    Window[] windows;

    [SerializeField]
    GameObject[] tutorialButtons;

    [SerializeField]
    int MainLevelSceneIndex = 2;

    [Header("Mosquito Spawner for UV and Zapper Tutorial")]
    [SerializeField] private MosquitoSpawner[] spawners; 
    private MosquitoSpawner activeSpawner;
    [Header("UV and Zapper Tutorial Index")]
    [SerializeField] private int practiceStageIndex = 3;

    private MosquitoAgent practiceMosquito;
    private float nextSpawnAttemptTime;

    // Keeps track of the next button to activate
    private int currentStageIndex = 0;

    void Awake()
    {
        // Make sure all buttons start disabled
        foreach (GameObject button in tutorialButtons)
        {
            button.SetActive(false);
        }

        activeSpawner = spawners[currentStageIndex];
    }

    private void Update()
    {
        if (practiceMosquito != null &&
            practiceMosquito.gameObject.activeSelf)
            return;

        if (Time.time < nextSpawnAttemptTime)
            return;

        nextSpawnAttemptTime = Time.time + 0.5f;
        practiceMosquito = activeSpawner.SpawnMosquito();

        // for testing purposes, set the mosquito to idle so it doesn't fly away
        // practiceMosquito.SetTask(MosquitoAgent.Task.Idle);
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
        if (currentStageIndex >= tutorialButtons.Length)
            return;

        // Activate the next button
        tutorialButtons[currentStageIndex].SetActive(true);

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

        tutorialButtons[currentStageIndex].SetActive(false);

        currentStageIndex++;
        activeSpawner = spawners[currentStageIndex];
    }

    public void ToMainLevel()
    {
        SceneManager.LoadScene(MainLevelSceneIndex);
    }
}   