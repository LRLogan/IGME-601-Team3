using System.Collections.Generic;
using UnityEngine;

public class UVLight : MonoBehaviour
{
    // Test utility: manually release the mosquito
    [Header("Setting below to manually release the mosquito")]
    [SerializeField] bool releaseMosquito;
    // Stop the mosquito from clipping into the UV Light object
    [SerializeField] float stopRange = 1f;
    // Duration of UV Light attraction on the mosquito
    [SerializeField] float attractionDuration = 5f;
    // Speed of attracted mosquito toward the UV light
    [SerializeField] float attractionSpeed = 1f;
    // Put mosquito in range into a list
    List<UVTestMosquito> affectedMosquito = new List<UVTestMosquito>();
    /* TODO: Implement attraction duration and release mosquito after effect ends
    class AttractionState
    {
        public UVTestMosquito mosquito;
        public float remainingTime;
        public bool released;
    }
    */

    void Update()
    {
        foreach (UVTestMosquito mosquito in affectedMosquito)
        {
            if (mosquito != null)
            {
                UVAttraction(mosquito);
            }
        }
        // Test utility: releasing mosquito
        if (releaseMosquito)
        {
            foreach (UVTestMosquito mosquito in affectedMosquito)
            {
                if (mosquito != null)
                {
                    mosquito.EndExternalControl();
                }
            }
            affectedMosquito.Clear();
            releaseMosquito = false;
            return;
        }

    }

    // Detect Mosquito entering UV light's area of effect, and get the mosquito
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Mosquito"))
        {
            return;
        }
        UVTestMosquito mosquito = other.GetComponent<UVTestMosquito>();
        // Ignore if no mosquito around, or mosquito already in the list
        if (mosquito == null || affectedMosquito.Contains(mosquito))
        {
            return;
        }
        affectedMosquito.Add(mosquito);
        // Switch movement control to the UV Light
        mosquito.BeginExternalControl();
        Debug.Log("Mosquito has entered effective range.");

    }

    void UVAttraction(UVTestMosquito mosquito)
    {
        Rigidbody mosBody = mosquito.GetComponent<Rigidbody>();
        if (mosBody == null)
        {
            return;
        }
        float distanceToLight = Vector3.Distance(mosBody.position, transform.position);
        // Calculate how the mosquito would move toward the UV light
        if (distanceToLight > stopRange)
        {
            float step = Mathf.Min(attractionSpeed * Time.fixedDeltaTime, distanceToLight - stopRange);
            Vector3 nextPos = Vector3.MoveTowards(mosBody.position, transform.position, step);
            mosBody.MovePosition(nextPos);
        }
    }

    // Detect Mosquito leaving UV light's area of effect, end UV Light's control over it
    void OnTriggerExit(Collider other)
    {
        UVTestMosquito mosquito = other.GetComponent<UVTestMosquito>();
        if (mosquito != null && affectedMosquito.Remove(mosquito))
        {
            mosquito.EndExternalControl();
            Debug.Log("Mosquito has exited effective range");
        }
    }

    // Release all mosquitos if the UV light is removed or disabled
    void OnDisable()
    {
        foreach (UVTestMosquito mosquito in affectedMosquito)
        {
            if (mosquito != null)
            {
                mosquito.EndExternalControl();
            }
        }
    }
}
