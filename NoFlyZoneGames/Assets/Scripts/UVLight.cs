using System;
using System.Collections.Generic;
using UnityEngine;

public class UVLight : MonoBehaviour, IItemEffect
{
    // Utilize the interface
    private bool effectActive = true;

    // Set a reachable point on the NavMesh near the light in Inspector
    [SerializeField] Transform attractionPoint;
    [SerializeField, Min(0.1f)] float attractionDuration = 3f;
    [SerializeField, Min(0.05f)] float releasePointRad = 0.5f;

    [SerializeField] private TrapAudioFeedback audioFeedback;

    /// <summary>
    /// Stores the cancellation callback and audio expiration time for
    /// each mosquito currently registered with the UV light.
    /// </summary>
    private class AttractionEntry
    {
        public Action Cancel;
        public float ExpiresAt;
        public bool AudioActive;

        public AttractionEntry(Action cancel, float expiresAt)
        {
            Cancel = cancel;
            ExpiresAt = expiresAt;
            AudioActive = true;
        }
    }

    private readonly Dictionary<Collider, AttractionEntry> cancelByCollider = new();

    private void Awake()
    {
        if (audioFeedback == null)
            audioFeedback = GetComponent<TrapAudioFeedback>();
    }

    private void Update()
    {
        // Stop counting attractions whose temporary movement has expired
        bool audioStateChanged = false;

        foreach (AttractionEntry entry in cancelByCollider.Values)
        {
            if (entry.AudioActive && Time.time >= entry.ExpiresAt)
            {
                entry.AudioActive = false;
                audioStateChanged = true;
            }
        }

        if (audioStateChanged)
            UpdateAudioState();
    }

    // Detect Mosquito leaving UV light's area of effect, end UV Light's control over it
    void OnTriggerExit(Collider other)
    {
        if (cancelByCollider.TryGetValue(other, out AttractionEntry entry))
        {
            entry.Cancel?.Invoke();
            cancelByCollider.Remove(other);

            UpdateAudioState();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // A pooled mosquito can re-enter without having sent an exit.
        OnTriggerExit(other);
        TryRegisterMosquito(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryRegisterMosquito(other);
    }

    private void TryRegisterMosquito(Collider other)
    {
        if (!effectActive || !isActiveAndEnabled || attractionPoint == null ||
            cancelByCollider.ContainsKey(other) ||
            !other.CompareTag("Mosquito") ||
            !other.TryGetComponent(out MosquitoAgent mosquito))
        {
            return;
        }

        Action cancel = mosquito.MoveToTemporarily(
            attractionPoint.position,
            releasePointRad,
            attractionDuration);

        // Keep this entry until the mosquito leaves, even after the timer expires.
        // Otherwise OnTriggerStay would immediately attract it again.
        cancelByCollider[other] = new AttractionEntry(cancel, Time.time + attractionDuration);

        UpdateAudioState();
    }

    // Release all mosquitos if the UV light is removed or disabled
    void OnDisable()
    {
        EndCurrentEffects();
    }

    // Using IItemEffect interface
    public void SetEffectActive(bool active)
    {
        effectActive = active;
        if (!active)
            EndCurrentEffects();
    }

    private void EndCurrentEffects()
    {
        foreach (AttractionEntry entry in cancelByCollider.Values)
            entry.Cancel?.Invoke();

        cancelByCollider.Clear();

        UpdateAudioState();
    }

    /// <summary>
    /// Determines whether any mosquito is still being actively attracted.
    /// </summary>
    private bool HasActiveAttraction()
    {
        foreach (AttractionEntry entry in cancelByCollider.Values)
        {
            if (entry.AudioActive)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Updates the UV light's continuous audio based on active attractions.
    /// </summary>
    private void UpdateAudioState()
    {
        if (audioFeedback != null)
        {
            audioFeedback.SetContinuousActive(
                effectActive && HasActiveAttraction());
        }
    }
}
