
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalLvl_SM : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<MosquitoSpawner> mosquitoSpawners = new();
    [SerializeField] private MosquitoManager mosquitoManager;

    [Header("Wave Settings")]
    [SerializeField] private List<int> numMosquitos = new();
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float spawnRetryInterval = 1f;

    [Header("UI")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text countdownText;

    private enum WaveState
    {
        Spawning,
        Active,
        Countdown,
        Finished
    }

    private WaveState currentState;
    private int currentWave;
    private int spawnedThisWave;
    private float countdown;
    private float spawnRetryTimer;

    private void Start()
    {
        countdownText.gameObject.SetActive(false);

        if (numMosquitos.Count == 0)
        {
            FinishLevel();
            return;
        }

        StartWave();
    }

    private void Update()
    {
        switch (currentState)
        {
            case WaveState.Spawning:
                // Retry if a spawn location was unavailable.
                spawnRetryTimer -= Time.deltaTime;

                if (spawnRetryTimer <= 0f)
                    SpawnRemainingMosquitoes();
                break;

            case WaveState.Active:
                CheckWaveCompletion();
                break;

            case WaveState.Countdown:
                UpdateCountdown();
                break;

            case WaveState.Finished:
                break;
        }
    }

    private void StartWave()
    {
        currentState = WaveState.Spawning;
        spawnedThisWave = 0;
        spawnRetryTimer = 0f;

        countdownText.gameObject.SetActive(false);
        waveText.text = $"Wave {currentWave + 1}";

        SpawnRemainingMosquitoes();
    }

    private void SpawnRemainingMosquitoes()
    {
        if (currentWave < 0 || currentWave >= numMosquitos.Count)
        {
            FinishLevel();
            return;
        }

        int required = Mathf.Max(0, numMosquitos[currentWave]);

        if (mosquitoSpawners == null || mosquitoSpawners.Count == 0)
        {
            Debug.LogWarning("No mosquito spawners assigned.", this);
            return;
        }

        while (spawnedThisWave < required)
        {
            // Rotate through spawners instead of always starting at the first.
            int spawnerIndex = spawnedThisWave % mosquitoSpawners.Count;
            MosquitoSpawner spawner = mosquitoSpawners[spawnerIndex];

            if (spawner == null)
            {
                spawnRetryTimer = spawnRetryInterval;
                return;
            }

            MosquitoAgent mosquito = spawner.SpawnMosquito();

            if (mosquito == null)
            {
                // Retry spawning later if this spawner cannot find a location.
                spawnRetryTimer = spawnRetryInterval;
                return;
            }

            spawnedThisWave++;
        }

        currentState = WaveState.Active;
    }

    private void CheckWaveCompletion()
    {
        // All mosquitoes have been returned to the pool.
        if (mosquitoManager.ActiveMosquitoCount > 0)
            return;

        if (currentWave >= numMosquitos.Count - 1)
        {
            FinishLevel();
            return;
        }

        StartCountdown();
    }

    private void StartCountdown()
    {
        currentState = WaveState.Countdown;
        countdown = timeBetweenWaves;

        countdownText.gameObject.SetActive(true);
        UpdateCountdownDisplay();
    }

    private void UpdateCountdown()
    {
        countdown -= Time.deltaTime;
        countdown = Mathf.Max(0f, countdown);

        UpdateCountdownDisplay();

        if (countdown <= 0f)
        {
            currentWave++;
            StartWave();
        }
    }

    private void UpdateCountdownDisplay()
    {
        countdownText.text =
            $"Next Wave: {Mathf.CeilToInt(countdown)}";
    }

    private void FinishLevel()
    {
        currentState = WaveState.Finished;

        countdownText.gameObject.SetActive(false);
        waveText.text = "All Waves Completed!";
    }

    // Connect this method to the Reset button's OnClick event.
    public void ResetLevel()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
