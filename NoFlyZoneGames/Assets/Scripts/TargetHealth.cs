using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class TargetHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int damagePerHit = 10;
    [SerializeField] private MosquitoManager mManager;
    [SerializeField] private Slider healthSlider;
    private HashSet<MosquitoAgent> arrivedMosquito = new HashSet<MosquitoAgent>();

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
        if (!other.CompareTag("Mosquito"))
        {
            return;
        }

        MosquitoAgent mosquito = other.GetComponent<MosquitoAgent>();
        if (mosquito == null)
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

        currentHealth = Mathf.Clamp(currentHealth - damagePerHit, 0, maxHealth);
        UpdateHealthBar();
        Debug.Log($"Mosquito hit the target. Health: {currentHealth}", this);
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

    }
}
