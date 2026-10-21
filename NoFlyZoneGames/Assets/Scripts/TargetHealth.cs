using UnityEngine;
using UnityEngine.AI;

public class TargetHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damagePerHit = 10;

    private int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Mosquito"))
        {
            return;
        }
        MosquitoAgent mosquito = other.GetComponent<MosquitoAgent>();
        if (mosquito == null)
        {
            return;
        }
        Debug.Log("Mosquito has hit the target");
        currentHealth -= damagePerHit;
    }
}
