using UnityEngine;

// temporary health component for zapper testing
public class MosquitoHealth : MonoBehaviour
{
    [SerializeField]
    private float health = 100f;
    private MosquitoManager mosquitoManager;

    void Start()
    {
        mosquitoManager = FindFirstObjectByType<MosquitoManager>();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        Debug.Log($"Mosquito took {damage} damage. Health: {health}");

        if (health <= 0)
        {
            mosquitoManager.DespawnMosquito
                (this.gameObject.GetComponent<MosquitoAgent>());

        }
    }
}