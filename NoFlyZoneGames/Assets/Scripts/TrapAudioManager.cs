using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coordinates trap audio so nearby effects do not overwhelm the player.
/// </summary>
public class TrapAudioManager : MonoBehaviour
{
    public static TrapAudioManager Instance { get; private set; }

    [Header("Audio Limits")]
    [SerializeField, Min(1)] private int maxOneShotVoices = 3;
    [SerializeField, Min(0)] private int maxContinuousVoices = 2;

    [SerializeField, Min(0.05f)]
    private float continuousRefreshInterval = 0.2f;

    [Header("One-Shot 3D Audio")]
    [SerializeField, Min(0.1f)] private float oneShotMinDistance = 1f;
    [SerializeField, Min(0.1f)] private float oneShotMaxDistance = 12f;

    private readonly List<AudioSource> oneShotSources =
        new List<AudioSource>();

    private readonly List<int> oneShotPriorities =
        new List<int>();

    private readonly List<TrapAudioFeedback> continuousEffects =
        new List<TrapAudioFeedback>();

    private readonly List<TrapAudioFeedback> selectedContinuousEffects =
        new List<TrapAudioFeedback>();

    private AudioListener cachedListener;
    private float nextContinuousRefresh;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        cachedListener = FindAnyObjectByType<AudioListener>();

        CreateOneShotVoices();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextContinuousRefresh)
            return;

        RefreshContinuousVoices();

        nextContinuousRefresh = Time.unscaledTime +
            Mathf.Max(0.05f, continuousRefreshInterval);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < continuousEffects.Count; i++)
        {
            if (continuousEffects[i] != null)
                continuousEffects[i].SetLoopAudible(false);
        }

        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Plays a one-shot sound if a voice is available or can be replaced
    /// by a higher-priority sound. Lower priority numbers are more important.
    /// </summary>
    public void PlayOneShot(
        AudioClip clip,
        Vector3 position,
        float volume,
        int priority)
    {
        if (clip == null || oneShotSources.Count == 0)
            return;

        int clampedPriority = Mathf.Clamp(priority, 0, 256);

        int voiceIndex = FindAvailableVoice();

        if (voiceIndex < 0)
            voiceIndex = FindReplaceableVoice(clampedPriority);

        // Drop lower-priority sounds rather than stacking unlimited audio
        if (voiceIndex < 0)
            return;

        AudioSource source = oneShotSources[voiceIndex];

        source.Stop();
        source.transform.position = position;
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.priority = clampedPriority;
        source.Play();

        oneShotPriorities[voiceIndex] = clampedPriority;
    }

    /// <summary>
    /// Registers a trap that is requesting a continuous looping effect.
    /// </summary>
    public void RegisterContinuous(TrapAudioFeedback feedback)
    {
        if (feedback == null || !feedback.HasContinuousClip)
            return;

        if (!continuousEffects.Contains(feedback))
            continuousEffects.Add(feedback);

        nextContinuousRefresh = 0f;
    }

    /// <summary>
    /// Removes a trap from the list of requested looping effects.
    /// </summary>
    public void UnregisterContinuous(TrapAudioFeedback feedback)
    {
        if (feedback == null)
            return;

        continuousEffects.Remove(feedback);
        feedback.SetLoopAudible(false);

        nextContinuousRefresh = 0f;
    }

    private void CreateOneShotVoices()
    {
        oneShotSources.Clear();
        oneShotPriorities.Clear();

        int voiceCount = Mathf.Max(1, maxOneShotVoices);

        float maxDistance = Mathf.Max(
            oneShotMinDistance, oneShotMaxDistance);

        for (int i = 0; i < voiceCount; i++)
        {
            GameObject voiceObject = new GameObject(
                "Trap One-Shot Voice " + (i + 1));

            voiceObject.transform.SetParent(transform, false);

            AudioSource source = voiceObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 1f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = oneShotMinDistance;
            source.maxDistance = maxDistance;
            source.priority = 128;

            oneShotSources.Add(source);
            oneShotPriorities.Add(256);
        }
    }

    private int FindAvailableVoice()
    {
        for (int i = 0; i < oneShotSources.Count; i++)
        {
            if (!oneShotSources[i].isPlaying)
                return i;
        }

        return -1;
    }

    private int FindReplaceableVoice(int incomingPriority)
    {
        int replaceIndex = -1;
        int lowestPriority = -1;

        for (int i = 0; i < oneShotSources.Count; i++)
        {
            if (oneShotPriorities[i] > lowestPriority)
            {
                lowestPriority = oneShotPriorities[i];
                replaceIndex = i;
            }
        }

        // Only interrupt a sound when the incoming sound is more important.
        return incomingPriority < lowestPriority ? replaceIndex : -1;
    }

    private void RefreshContinuousVoices()
    {
        for (int i = continuousEffects.Count - 1; i >= 0; i--)
        {
            if (continuousEffects[i] == null)
                continuousEffects.RemoveAt(i);
        }

        selectedContinuousEffects.Clear();

        Vector3 listenerPosition = GetListenerPosition();

        int voiceLimit = Mathf.Max(0, maxContinuousVoices);

        // Select the nearest requesting traps, breaking distance ties by priority.
        for (int slot = 0; slot < voiceLimit; slot++)
        {
            TrapAudioFeedback closest = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < continuousEffects.Count; i++)
            {
                TrapAudioFeedback candidate = continuousEffects[i];

                if (candidate == null ||
                    !candidate.WantsContinuousAudio ||
                    selectedContinuousEffects.Contains(candidate))
                {
                    continue;
                }

                float distance = (
                    candidate.transform.position - listenerPosition
                ).sqrMagnitude;

                bool isCloser = distance < closestDistance;

                bool winsTie =
                    Mathf.Approximately(distance, closestDistance) &&
                    (closest == null ||
                    candidate.ContinuousPriority < closest.ContinuousPriority);

                if (isCloser || winsTie)
                {
                    closest = candidate;
                    closestDistance = distance;
                }
            }

            if (closest == null)
                break;

            selectedContinuousEffects.Add(closest);
        }

        for (int i = 0; i < continuousEffects.Count; i++)
        {
            TrapAudioFeedback feedback = continuousEffects[i];

            if (feedback != null)
            {
                feedback.SetLoopAudible(
                    selectedContinuousEffects.Contains(feedback));
            }
        }
    }

    private Vector3 GetListenerPosition()
    {
        if (cachedListener == null)
            cachedListener = FindAnyObjectByType<AudioListener>();

        if (cachedListener != null)
            return cachedListener.transform.position;

        Camera mainCamera = Camera.main;

        return mainCamera != null
            ? mainCamera.transform.position
            : transform.position;
    }
}
