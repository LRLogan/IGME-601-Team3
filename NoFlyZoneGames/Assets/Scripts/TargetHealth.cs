using UnityEngine;
using System;
using UnityEngine.UI;

public class TargetHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damagePerHit = 10;
    [SerializeField] private MosquitoManager mManager;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private FinalLvl_SM sceneManager;
    public event Action<TargetHealth, MosquitoAgent> OnMosquitoReachedTarget;

    private int currentHealth;

    void Awake()
    {
        // Set the target to full health at the beginning of the game
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.wholeNumbers = true;
        }
        UpdateHealthBar();
    }

    private void OnTriggerEnter(Collider other)
    {
        MosquitoAgent mosquito = other.GetComponentInParent<MosquitoAgent>();
        if (mosquito == null || !mosquito.CompareTag("Mosquito"))
        {
            return;
        }

        if (mManager == null)
        {
            return;
        }

        // Count the hit only if manager accepts the active mosquito
        // Further callbacks for the despawned mosquito cannot cause more damage
        if (!mManager.TryDespawnMosquito(mosquito))
        {
            return;
        }

        OnMosquitoReachedTarget?.Invoke(this, mosquito);

        currentHealth = Mathf.Clamp(currentHealth - damagePerHit, 0, maxHealth);
        UpdateHealthBar();
        Debug.Log($"Mosquito hit the target. Health: {currentHealth}", this);

        if (currentHealth <= 0)
        {
            Debug.Log("Target destroyed!", this);
            if (sceneManager != null)
                sceneManager.FailLevel();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

    }
}
