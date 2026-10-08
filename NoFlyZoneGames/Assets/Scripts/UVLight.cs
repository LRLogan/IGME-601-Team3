using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UVLight : MonoBehaviour
{
    // Test utility: manually release the mosquito
    [Header("Setting below to manually release the mosquito")]
    [SerializeField] bool releaseMosquito;
    // Set a reachable point on the NavMesh near the light in Inspector
    [SerializeField] Transform attractionPoint;
    // Set a release point on the NavMesh. Right now mosquito can only be manually released
    // And it won't return Mosquito to it's original task/destination
    [SerializeField] Transform releasePoint;
    // TODO: Duration of UV Light attraction on the mosquito
    // [SerializeField] float attractionDuration = 5f;
    // Put mosquito in range into a list
    List<MosquitoAgent> affectedMosquito = new List<MosquitoAgent>();
    /* TODO: Implement attraction duration and release mosquito after effect ends
    class AttractionState
    {
        public MosquitoAgent mosquito;
        public float remainingTime;
        public bool released;
    }
    */
    [SerializeField] private float releasePointRad = 0.5f;

    void Update()
    {
        // Releasing mosquito once it has reached the desired point
        /*
        if (releaseMosquito)
        {
            foreach (MosquitoAgent mosquito in affectedMosquito)
            {
                if (mosquito != null)
                {
                    mosquito.MoveTo(releasePoint.position);
                }
            }
            affectedMosquito.Clear();
            releaseMosquito = false;
            return;
        }
        */

        // Checking each mosquito in the effected list
        for (int i = affectedMosquito.Count - 1; i >= 0; i--)
        {
            MosquitoAgent mosquito = affectedMosquito[i];

            if (mosquito == null)
            {
                affectedMosquito.RemoveAt(i);
                continue;
            }

            if (Vector3.Distance(
                mosquito.transform.position,
                attractionPoint.position) <= releasePointRad)
            {
                affectedMosquito.RemoveAt(i);
            }
        }
    }

    // Detect Mosquito entering UV light's area of effect, and get the mosquito
    void OnTriggerEnter(Collider other)
    {
        // Debug message for UV range detection
        Debug.Log($"UV trigger detected: {other.name}: {other.tag}", this);
        if (!other.CompareTag("Mosquito"))
        {
            return;
        }
        MosquitoAgent mosquito = other.GetComponent<MosquitoAgent>();
        // Ignore if no mosquito around, or mosquito already in the list
        if (mosquito == null || affectedMosquito.Contains(mosquito))
        {
            return;
        }
        // Check if the attraction point has been set yet
        if (attractionPoint == null)
        {
            Debug.LogWarning("Assign an attraction point to the UV light.", this);
            return;
        }
        affectedMosquito.Add(mosquito);
        // Request the mosquito to move toward the UV attraction point
        mosquito.MoveTo(attractionPoint.position);
    }


    // Detect Mosquito leaving UV light's area of effect, end UV Light's control over it
    void OnTriggerExit(Collider other)
    {
        MosquitoAgent mosquito = other.GetComponent<MosquitoAgent>();
        if (mosquito != null && affectedMosquito.Remove(mosquito))
        {
            Debug.Log("Mosquito has exited effective range");
        }
    }

    // Release all mosquitos if the UV light is removed or disabled
    void OnDisable()
    {
        foreach (MosquitoAgent mosquito in affectedMosquito)
        {
            if (mosquito != null)
            {
                mosquito.MoveTo(releasePoint.position);
            }
        }
        affectedMosquito.Clear();
    }
}
