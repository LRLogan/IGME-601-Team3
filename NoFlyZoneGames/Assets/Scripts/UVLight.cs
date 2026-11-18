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
    private readonly Dictionary<Collider, Action> cancelByCollider = new();


    // Detect Mosquito leaving UV light's area of effect, end UV Light's control over it
    void OnTriggerExit(Collider other)
    {
        if (cancelByCollider.TryGetValue(other, out Action cancel))
        {
            cancel();
            cancelByCollider.Remove(other);
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

        // Keep this entry until the mosquito leaves, even after the timer expires.
        // Otherwise OnTriggerStay would immediately attract it again.
        cancelByCollider[other] = mosquito.MoveToTemporarily(
            attractionPoint.position, releasePointRad, attractionDuration);
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
        foreach (Action cancel in cancelByCollider.Values)
            cancel();

        cancelByCollider.Clear();
    }
}
