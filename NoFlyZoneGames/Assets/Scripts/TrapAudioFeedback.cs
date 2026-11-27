using UnityEngine;

/// <summary>
/// Reusable audio feedback for traps that influence mosquitoes.
/// Continuous sounds are looped and faded; one-shots use the scene audio manager.
/// </summary>
[DisallowMultipleComponent]
public class TrapAudioFeedback : MonoBehaviour
{
    [Header("Continuous Effect")]
    [SerializeField] private AudioClip continuousClip;
    [SerializeField, Range(0f, 1f)]
    private float continuousVolume = 0.3f;

    [SerializeField, Min(0f)] private float fadeDuration = 0.25f;
    [SerializeField, Min(0.1f)] private float continuousMinDistance = 1f;
    [SerializeField, Min(0.1f)] private float continuousMaxDistance = 10f;
    [SerializeField, Range(0, 256)] private int continuousPriority = 100;
    [SerializeField] private AudioSource continuousSource;

    [Header("One-Shot Effect")]
    [SerializeField] private AudioClip oneShotClip;
    [SerializeField, Range(0f, 1f)]
    private float oneShotVolume = 0.7f;

    [SerializeField, Range(0, 256)] private int oneShotPriority = 64;
    [SerializeField, Min(0f)]
    private float minimumOneShotInterval = 0.05f;

    private TrapAudioManager audioManager;
    private bool wantsContinuousAudio;
    private bool loopAudible;
    private bool warnedAboutManager;
    private float currentLoopVolume;
    private float nextOneShotTime;
    private float nextManagerSearchTime;

    public bool HasContinuousClip =>
        continuousClip != null ||
        (continuousSource != null && continuousSource.clip != null);

    public bool WantsContinuousAudio => wantsContinuousAudio;
    public int ContinuousPriority => continuousPriority;

    private void Awake()
    {
        ConfigureContinuousSource();
    }

    private void Update()
    {
        RetryManagerRegistration();
        UpdateContinuousVolume();
    }

    private void OnDisable()
    {
        wantsContinuousAudio = false;
        loopAudible = false;

        if (audioManager != null)
            audioManager.UnregisterContinuous(this);

        currentLoopVolume = 0f;

        if (continuousSource != null)
        {
            continuousSource.volume = 0f;
            continuousSource.Stop();
        }
    }

    /// <summary>
    /// Starts or stops this trap's continuous sound request.
    /// The manager decides whether the sound gets one of the available voices.
    /// </summary>
    public void SetContinuousActive(bool active)
    {
        if (!HasContinuousClip)
            return;

        if (wantsContinuousAudio == active)
            return;

        wantsContinuousAudio = active;

        if (active)
        {
            if (TryGetAudioManager())
                audioManager.RegisterContinuous(this);
            else
                WarnMissingManager();
        }
        else if (audioManager != null)
        {
            audioManager.UnregisterContinuous(this);
        }

        if (!active)
            loopAudible = false;
    }

    /// <summary>
    /// Plays the one-shot clip assigned in the Inspector.
    /// </summary>
    public void PlayOneShot()
    {
        PlayOneShot(oneShotClip);
    }

    /// <summary>
    /// Plays a supplied one-shot clip through the shared audio manager.
    /// </summary>
    public void PlayOneShot(AudioClip clip)
    {
        if (clip == null || Time.time < nextOneShotTime)
            return;

        // Do not overlap repeated copies of the same trap sound
        float repeatDelay = Mathf.Max(
            minimumOneShotInterval, clip.length);

        nextOneShotTime = Time.time + repeatDelay;

        if (TryGetAudioManager())
        {
            audioManager.PlayOneShot(
                clip,
                transform.position,
                oneShotVolume,
                oneShotPriority);
        }
        else
        {
            WarnMissingManager();
        }
    }

    /// <summary>
    /// Called by TrapAudioManager to grant or revoke a looping audio voice.
    /// </summary>
    public void SetLoopAudible(bool audible)
    {
        loopAudible = audible;
    }

    private void ConfigureContinuousSource()
    {
        if (continuousSource == null && continuousClip != null)
        {
            GameObject sourceObject =
                new GameObject("Continuous Trap Audio");

            sourceObject.transform.SetParent(transform, false);

            continuousSource = sourceObject.AddComponent<AudioSource>();
        }

        if (continuousSource == null)
            return;

        if (continuousClip == null)
            continuousClip = continuousSource.clip;
        else
            continuousSource.clip = continuousClip;

        continuousSource.playOnAwake = false;
        continuousSource.loop = true;
        continuousSource.spatialBlend = 1f;
        continuousSource.dopplerLevel = 0f;
        continuousSource.rolloffMode = AudioRolloffMode.Linear;
        continuousSource.minDistance = continuousMinDistance;

        continuousSource.maxDistance = Mathf.Max(
            continuousMinDistance, continuousMaxDistance);

        continuousSource.volume = 0f;
        continuousSource.Stop();
    }

    private void RetryManagerRegistration()
    {
        if (!wantsContinuousAudio || audioManager != null)
            return;

        if (TryGetAudioManager())
            audioManager.RegisterContinuous(this);
    }

    private void UpdateContinuousVolume()
    {
        if (continuousSource == null || !HasContinuousClip)
            return;

        bool shouldPlay = wantsContinuousAudio && loopAudible;

        float targetVolume = shouldPlay ? continuousVolume : 0f;

        if (shouldPlay && !continuousSource.isPlaying)
        {
            continuousSource.volume = currentLoopVolume;
            continuousSource.Play();
        }

        float fadeRate = fadeDuration <= 0f
            ? float.MaxValue
            : Mathf.Max(continuousVolume, 0.01f) / fadeDuration;

        currentLoopVolume = Mathf.MoveTowards(
            currentLoopVolume,
            targetVolume,
            fadeRate * Time.deltaTime);

        continuousSource.volume = currentLoopVolume;

        if (!shouldPlay &&
            currentLoopVolume <= 0.001f &&
            continuousSource.isPlaying)
        {
            continuousSource.Stop();
        }
    }

    private bool TryGetAudioManager()
    {
        if (audioManager != null)
            return true;

        if (Time.unscaledTime < nextManagerSearchTime)
            return false;

        nextManagerSearchTime = Time.unscaledTime + 1f;

        audioManager = TrapAudioManager.Instance;

        if (audioManager == null)
            audioManager = FindAnyObjectByType<TrapAudioManager>();

        return audioManager != null;
    }

    private void WarnMissingManager()
    {
        if (warnedAboutManager)
            return;

        Debug.LogWarning(
            "TrapAudioFeedback needs a TrapAudioManager in the scene.",
            this);

        warnedAboutManager = true;
    }
}
