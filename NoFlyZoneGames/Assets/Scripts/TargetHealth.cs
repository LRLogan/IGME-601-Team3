using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class TargetHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damagePerHit = 10;
    [SerializeField] private MosquitoManager mManager;
    private HashSet<MosquitoAgent> arrivedMosquito = new HashSet<MosquitoAgent>();

    private int currentHealth;

    void Awake()
    {
        // Set the target to full health at the beginning of the game
        currentHealth = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Mosquito"))
        {
            return;
        }
        MosquitoAgent mosquito = other.GetComponent<MosquitoAgent>();
        if (mosquito == null || arrivedMosquito.Contains(mosquito))
        {
            // If no mosquito or mosquito already registered, ignore
            return;
        }
        // Register the mosquito
        arrivedMosquito.Add(mosquito);
        // Reduce target health, no lower than 0
        currentHealth = Mathf.Clamp(currentHealth - damagePerHit, 0, maxHealth);
        Debug.Log($"Mosquito has hit the target, current health is {currentHealth}");
        // Unregister and de-spawn the mosquito from map
        mManager.UnregisterMosquito(mosquito);
        Destroy(mosquito.gameObject);
    }
}
