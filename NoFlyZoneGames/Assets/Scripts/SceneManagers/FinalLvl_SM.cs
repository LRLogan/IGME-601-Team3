
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalLvl_SM : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MosquitoSpawner mosquitoSpawner;
    [SerializeField] private MosquitoManager mosquitoManager;

    [Header("Wave Settings")]
    [SerializeField] private List<int> numMosquitos = new();
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float spawnRetryInterval = 1f;

    [Header("UI")]
    // [SerializeField] private TMP_Text waveText;
    // [SerializeField] private TMP_Text countdownText;
    [SerializeField] private PlayerHUDController playerHUD;

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
        // countdownText.gameObject.SetActive(false);
        playerHUD.ShowCountdown(false);

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

        // countdownText.gameObject.SetActive(false);
        // waveText.text = $"Wave {currentWave + 1}";
        playerHUD.ShowCountdown(false);
        playerHUD.SetWave(currentWave + 1, numMosquitos.Count);

        SpawnRemainingMosquitoes();
    }

    private void SpawnRemainingMosquitoes()
    {
        int required = Mathf.Max(0, numMosquitos[currentWave]);

        while (spawnedThisWave < required)
        {
            MosquitoAgent mosquito = mosquitoSpawner.SpawnMosquito();

            if (mosquito == null)
            {
                // Keep the wave in Spawning state until
                // all required mosquitoes have spawned.
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

        // countdownText.gameObject.SetActive(true);
        playerHUD.SetCountdownLabel("NEXT WAVE IN");
        playerHUD.SetCountdown(countdown);
        playerHUD.ShowCountdown(true);
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
        // countdownText.text =
        //     $"Next Wave: {Mathf.CeilToInt(countdown)}";
        playerHUD.SetCountdown(countdown);
    }

    private void FinishLevel()
    {
        currentState = WaveState.Finished;

        // countdownText.gameObject.SetActive(false);
        // waveText.text = "All Waves Completed!";
        playerHUD.ShowCountdown(false);
    }

    // Connect this method to the Reset button's OnClick event.
    public void ResetLevel()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
