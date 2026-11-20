using UnityEngine;

// temporary health component for zapper testing
public class MosquitoHealth : MonoBehaviour
{
    [SerializeField]
    private float health = 100f;
    private float initialHealth;

    private MosquitoManager mosquitoManager;

    public bool IsDead => health <= 0f;
    
    void Start()
    {
        if (mosquitoManager == null) 
            mosquitoManager = FindFirstObjectByType<MosquitoManager>();
    }

    private void Awake()
    {
        initialHealth = health;
    }

    private void OnEnable()
    {
        health = initialHealth;
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