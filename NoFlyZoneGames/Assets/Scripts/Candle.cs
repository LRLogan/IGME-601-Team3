using System.Collections.Generic;
using UnityEngine;

public class Candle : MonoBehaviour
{
    // Set the inner radius of Candle's area of effect
    // The outer radius is controlled by the sphere collider
    // When a mosquito touches inner radius, it is repelled
    [SerializeField] private float innerRadius = 0.5f;
    // Make the deflection angle (on X axis only for now) adjustable
    [SerializeField] private float deflectionAngle = 90f;
    // Set a distance of escape for the mosquito, which should be more than the outer radius
    [SerializeField] private float escapeDistance = 2.5f;
    // Record the mosquito inside the outer range
    private List<MosquitoAgent> mosquitoInRange = new List<MosquitoAgent>();
    // Record the already repelled mosquito, do not repel them a second time until they are completely out of range 
    private HashSet<MosquitoAgent> repelledMosquito = new HashSet<MosquitoAgent>();

    // Make the mosquito to turn around any axis
    // public enum TurnAxis
    // {
    //     X,
    //     Y,
    //     Z
    // }
    // [SerializeField] private TurnAxis turnAxis = TurnAxis.X;

    // Update is called once per frame
    void Update()
    {
        foreach (MosquitoAgent mosquito in mosquitoInRange)
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
        Debug.Log($"Candle trigger detected: {other.name}: {other.tag}", this);
        if (!other.CompareTag("Mosquito"))
        {
            return;
        }
        MosquitoAgent mosquito = other.GetComponent<MosquitoAgent>();
        if (mosquito == null || mosquitoInRange.Contains(mosquito))
        {
            return;
        }
        mosquitoInRange.Add(mosquito);
        Debug.Log("Mosquito has entered effective range");
    }

    void Repel(MosquitoAgent mosquito)
    {
        // Check if the mosquito has already been repelled
        if (repelledMosquito.Contains(mosquito))
        {
            return;
        }
        // Calculate the distance between mosquito and candle
        float distance = Vector3.Distance(mosquito.transform.position, transform.position);
        // If mosquito is within the inner radius, repel it
        if (distance > innerRadius)
        {
            return;
        }
        // To deflect the mosquito in other arbitrary directions
        // Vector3 axis = Vector3.zero;
        // switch (turnAxis)
        // {
        //     case TurnAxis.X:
        //         axis = Vector3.right;
        //         break;
        //     case TurnAxis.Y:
        //         axis = Vector3.up;
        //         break;
        //     case TurnAxis.Z:
        //         axis = Vector3.forward;
        //         break;
        // }
        Vector3 incomingDirection = mosquito.transform.forward;
        Vector3 escapeDirection = Quaternion.AngleAxis(deflectionAngle, Vector3.up) * incomingDirection;
        Vector3 candidateDestination = mosquito.transform.position + escapeDirection * escapeDistance;
        mosquito.MoveTo(candidateDestination);
        repelledMosquito.Add(mosquito);
        Debug.Log("Mosquito reached inner range and was repelled.");
    }

    // When the mosquito leaves the outer range, forget it so it can trigger another deflection
    private void OnTriggerExit(Collider other)
    {
        MosquitoAgent mosquito = other.GetComponent<MosquitoAgent>();
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
