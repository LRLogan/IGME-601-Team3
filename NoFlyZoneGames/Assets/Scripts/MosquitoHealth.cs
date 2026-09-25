using UnityEngine;

// temporary health component for zapper testing
public class MosquitoHealth : MonoBehaviour
{
    [SerializeField]
    private float health = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        Debug.Log($"Mosquito took {damage} damage. Health: {health}");

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}