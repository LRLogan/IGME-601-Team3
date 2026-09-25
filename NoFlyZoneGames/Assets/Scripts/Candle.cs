using System.Collections.Generic;
using UnityEngine;

public class Candle : MonoBehaviour
{
    // Set the inner radius of Candle's area of effect
    // The outer radius is controlled by the sphere collider
    // When a mosquito touches inner radius, it is repelled
    [SerializeField] private float innerRadius = 0.5f;
    // Make the deflection angle (on X axis only for now) adjustable
    [SerializeField] private float deflectionAngle = 30f;
    // Record the mosquito inside the outer range
    private List<UVTestMosquito> mosquitoInRange = new List<UVTestMosquito>();
    // Record the already repelled mosquito, do not repel them a second time until they are completely out of range 
    private HashSet<UVTestMosquito> repelledMosquito = new HashSet<UVTestMosquito>();

    // Update is called once per frame
    void Update()
    {
        foreach (UVTestMosquito mosquito in mosquitoInRange)
        {
            if (mosquito != null)
            {
                Repel(mosquito);
            }
        }
    }

    // Attempt to repel the mosquito once it gets inside the collider
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Mosquito"))
        {
            return;
        }
        UVTestMosquito mosquito = other.GetComponent<UVTestMosquito>();
        if (mosquito == null || mosquitoInRange.Contains(mosquito))
        {
            return;
        }
        mosquitoInRange.Add(mosquito);
        Debug.Log("Mosquito has entered effective range");
    }

    void Repel(UVTestMosquito mosquito)
    {
        // Check if the mosquito has already been repelled
        if (repelledMosquito.Contains(mosquito))
        {
            return;
        }
        Rigidbody body = mosquito.GetComponent<Rigidbody>();
        if (body == null)
        {
            return;
        }
        // Calculate the distance between mosquito and candle
        float distance = Vector3.Distance(body.position, transform.position);
        // If mosquito is within the inner radius, repel it
        if (distance > innerRadius)
        {
            return;
        }
        // Set a upward turn along x axis and turn the mosquito
        Vector3 angles = body.rotation.eulerAngles;
        angles.x -= deflectionAngle;
        Quaternion newRotation = Quaternion.Euler(angles);
        body.MoveRotation(newRotation);
        repelledMosquito.Add(mosquito);
        Debug.Log("Mosquito reached inner range and was repelled.");
    }

    // When the mosquito leaves the outer range, forget it so it can trigger another deflection
    private void OnTriggerExit(Collider other)
    {
        UVTestMosquito mosquito = other.GetComponent<UVTestMosquito>();
        if (mosquito == null)
        {
            return;
        }
        mosquitoInRange.Remove(mosquito);
        repelledMosquito.Remove(mosquito);
        Debug.Log("Mosquito left outer range.");
    }

    // Draw the inner range of effects for visual and testing purpose
    private void OnDrawGizmos()
    {
        // Draw the inner sphere
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, innerRadius);
    }
}
