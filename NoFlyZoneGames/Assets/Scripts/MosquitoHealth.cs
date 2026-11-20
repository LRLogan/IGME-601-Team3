using UnityEngine;

// temporary health component for zapper testing
public class MosquitoHealth : MonoBehaviour
{
    [SerializeField]
    private float health = 100f;

    private MosquitoManager mosquitoManager;

    public bool IsDead => health <= 0f;
    
    void Start()
    {
        if (mosquitoManager == null) 
            mosquitoManager = FindFirstObjectByType<MosquitoManager>();
    }

    public void SetManager(MosquitoManager manager)
    {
        mosquitoManager = manager;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
            return;
        
        health -= damage;

        Debug.Log($"Mosquito took {damage} damage. Health: {health}");

        if (health <= 0)
            mosquitoManager.DespawnMosquito
                (gameObject.GetComponent<MosquitoAgent>());
    }
}