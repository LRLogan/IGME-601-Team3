using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Runs the four tutorial lessons, retries, and completion choices.</summary>
public class Tutorial_SM : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [Header("Rooms (in lesson order)")]
    [SerializeField] private Transform[] stageSpawnPoints;
    [SerializeField] private Window[] windows;
    [SerializeField] private GameObject[] tutorialButtons;
    [SerializeField] private MosquitoSpawner[] spawners;
    [SerializeField] private TargetHealth[] targets;
    [SerializeField] private MosquitoAgent firstRoomMosquito;
    [SerializeField] private int practiceStageIndex = 3;
    [SerializeField] private float retryDelay = 2f;

    [Header("UI")]
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private GameObject mainMenuButton;
    [SerializeField] private PauseMenuController pauseMenu;
    [SerializeField] private int MainLevelSceneIndex = 2;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private static readonly string[] objectives =
    {
        "ROOM 1 / 4 - LEARN TO PLACE ITEMS\nThis mosquito stays still so you can practise. Place a fan to push it out the window.",
        "ROOM 2 / 4 - PROTECT THE TARGET\nA mosquito is flying toward the person. Place a fan to push it out the window before it reaches them.",
        "ROOM 3 / 4 - GUIDE ITS ROUTE\nProtect the person in this more complex room. Use the candle and fans to guide the mosquito out the window.",
        "ROOM 4 / 4 - PUT IT ALL TOGETHER\nProtect the person. Guide the mosquito out the window OR attract it with the UV light and kill it with the zapper."
    };

    private FirstPersonController playerController;
    private PlayerInteraction playerInteraction;
    private StarterAssetsInputs playerInputs;
    private MosquitoHealth activeHealth;
    private MosquitoAgent activeMosquito;
    private int currentStageIndex;
    private bool roomComplete;
    private float nextSpawnAttemptTime;
    private float feedbackUntil;

    public int CurrentStageIndex => currentStageIndex;
    public bool IsRoomComplete => roomComplete;
    public MosquitoAgent ActiveMosquito => activeMosquito;

    private void Awake()
    {
        playerController = player.GetComponent<FirstPersonController>();
        playerInteraction = player.GetComponent<PlayerInteraction>();
        playerInputs = player.GetComponent<StarterAssetsInputs>();
        HideCompletion();
    }

    private void OnEnable()
    {
        foreach (Window window in windows)
            if (window != null) window.OnMosquitoExited += OnWindowExit;
        foreach (TargetHealth target in targets)
            if (target != null) target.OnMosquitoReachedTarget += OnTargetReached;
        if (activeHealth != null) activeHealth.OnDied += OnMosquitoDied;
    }

    private void OnDisable()
    {
        foreach (Window window in windows)
            if (window != null) window.OnMosquitoExited -= OnWindowExit;
        foreach (TargetHealth target in targets)
            if (target != null) target.OnMosquitoReachedTarget -= OnTargetReached;
        if (activeHealth != null) activeHealth.OnDied -= OnMosquitoDied;
    }

    private void Start()
    {
        BeginStage(0);
    }

    private void Update()
    {
        if (roomComplete || Time.timeScale == 0f) return;

        if (feedbackText != null && Time.time >= feedbackUntil)
            feedbackText.text = string.Empty;

        // Room one only ever uses its placed, idle mosquito.
        if (currentStageIndex == 0 ||
            (activeMosquito != null && activeMosquito.gameObject.activeInHierarchy) ||
            Time.time < nextSpawnAttemptTime)
            return;

        nextSpawnAttemptTime = Time.time + 0.5f;
        TrackMosquito(spawners[currentStageIndex].SpawnMosquito());
    }

    private void TrackMosquito(MosquitoAgent mosquito)
    {
        if (activeHealth != null) activeHealth.OnDied -= OnMosquitoDied;
        activeMosquito = mosquito;
        activeHealth = mosquito != null ? mosquito.GetComponent<MosquitoHealth>() : null;
        if (activeHealth != null) activeHealth.OnDied += OnMosquitoDied;
    }

    private void BeginStage(int index)
    {
        currentStageIndex = index;
        roomComplete = false;
        HideCompletion();
        if (objectiveText != null) objectiveText.text = objectives[index];
        if (feedbackText != null) feedbackText.text = string.Empty;
        nextSpawnAttemptTime = Time.time;

        if (index == 0)
        {
            firstRoomMosquito.ResetAgent();
            TrackMosquito(firstRoomMosquito);
        }
        else
        {
            TrackMosquito(null);
        }

        SetPlayerControl(true);
    }

    private void OnWindowExit(Window window, MosquitoAgent mosquito)
    {
        if (!roomComplete && window == windows[currentStageIndex] && mosquito == activeMosquito)
            CompleteRoom();
    }

    private void OnTargetReached(TargetHealth target, MosquitoAgent mosquito)
    {
        if (roomComplete || mosquito != activeMosquito || target != targets[currentStageIndex]) return;
        RetryRoom("Attempt failed: the mosquito reached the target. Try again! Another mosquito is spawning.");
    }

    private void OnMosquitoDied(MosquitoHealth health)
    {
        if (roomComplete || health != activeHealth) return;
        if (currentStageIndex == practiceStageIndex)
            CompleteRoom();
        else
            RetryRoom("For this lesson, push the mosquito out the window. Try again!");
    }

    private void RetryRoom(string message)
    {
        TrackMosquito(null);
        nextSpawnAttemptTime = Time.time + retryDelay;
        feedbackUntil = Time.time + retryDelay + 3f;
        if (feedbackText != null) feedbackText.text = message;
    }

    private void CompleteRoom()
    {
        roomComplete = true;
        TrackMosquito(null);
        if (feedbackText != null) feedbackText.text = string.Empty;
        completionPanel.SetActive(true);
        tutorialButtons[currentStageIndex].SetActive(true);
        bool finalRoom = currentStageIndex == stageSpawnPoints.Length - 1;
        mainMenuButton.SetActive(finalRoom);
        completionText.text = finalRoom
            ? "TUTORIAL COMPLETE\nYou're ready! Return to the menu or play the next level."
            : $"ROOM {currentStageIndex + 1} COMPLETE\nGreat work! Continue when you're ready.";
        SetPlayerControl(false);
        EventSystem.current?.SetSelectedGameObject(tutorialButtons[currentStageIndex]);
    }

    private void HideCompletion()
    {
        foreach (GameObject button in tutorialButtons)
            if (button != null) button.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.SetActive(false);
        if (completionPanel != null) completionPanel.SetActive(false);
    }

    /// <summary>Only the completed room's button can advance to its next lesson.</summary>
    public void NextStage(int index)
    {
        if (!roomComplete || index != currentStageIndex + 1 || index >= stageSpawnPoints.Length) return;

        playerInteraction.ReturnHeldItem();
        CharacterController controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.transform.SetPositionAndRotation(stageSpawnPoints[index].position, stageSpawnPoints[index].rotation);
        controller.enabled = true;
        BeginStage(index);
    }

    private void SetPlayerControl(bool enabled)
    {
        playerController.enabled = enabled;
        playerInteraction.enabled = enabled;
        if (pauseMenu != null) pauseMenu.enabled = enabled;
        playerInputs.move = Vector2.zero;
        playerInputs.look = Vector2.zero;
        playerInputs.jump = false;
        playerInputs.sprint = false;
        playerInputs.cursorLocked = enabled;
        playerInputs.cursorInputForLook = enabled;
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }

    public void ToMainLevel()
    {
        if (!roomComplete || currentStageIndex != stageSpawnPoints.Length - 1) return;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(MainLevelSceneIndex);
    }

    public void ToMainMenu()
    {
        if (!roomComplete || currentStageIndex != stageSpawnPoints.Length - 1) return;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
