using System;
using UnityEngine;

public class MosquitoHealth : MonoBehaviour
{
    [SerializeField]
    private float health = 100f;
    private float initialHealth;

    private MosquitoManager mosquitoManager;

    public bool IsDead => health <= 0f;
    public event Action<MosquitoHealth> OnDied;

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
        if (IsDead || !gameObject.activeInHierarchy)
            return;
        
        health -= damage;

        Debug.Log($"Mosquito took {damage} damage. Health: {health}");

        if (health <= 0)
        {
            Despawn();
            OnDied?.Invoke(this);
        }
    }

    /// <summary>Returns a spawned mosquito to its owner, or disables a placed mosquito.</summary>
    public void Despawn()
    {
        MosquitoAgent mosquito = GetComponent<MosquitoAgent>();
        if (mosquitoManager != null && mosquitoManager.TryDespawnMosquito(mosquito)) return;
        if (mosquito != null) mosquito.ResetAgent();
        gameObject.SetActive(false);
    }
}
