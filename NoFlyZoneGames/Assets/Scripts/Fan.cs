using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour, IItemEffect
{
    private bool effectActive = true;
    private string itemName;
    [SerializeField]
    private float strength;

    [SerializeField] private TrapAudioFeedback audioFeedback;

    Transform pushPoint;

    // Tracks each mosquito affected by the fan without duplicates
    private readonly HashSet<MosquitoAgent> affectedMosquitoes =
        new HashSet<MosquitoAgent>();

    //[SerializeField]
    //private CapsuleCollider hitbox; // Hastag this isn't used?
    //private BoxCollider hitbox;
    public void Awake()
    {
        pushPoint = transform.Find("PushPoint");

        if (audioFeedback == null)
        {
            audioFeedback = GetComponent<TrapAudioFeedback>();
        }
    }

    public void Start()
    {
        itemName = "fan";
    }

    private void OnTriggerEnter(Collider other)
    {
        RegisterMosquito(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!effectActive)
            return;

        if (!other.CompareTag("Mosquito"))
            return;

        MosquitoAgent mosquito = other.GetComponentInParent<MosquitoAgent>();

        if (mosquito != null)
            RegisterMosquito(other);
            Push(mosquito);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Mosquito"))
            return;

        MosquitoAgent mosquito = other.GetComponentInParent<MosquitoAgent>();

        if (mosquito != null && affectedMosquitoes.Remove(mosquito))
            UpdateAudioState();
    }

    private void Update()
    {
        // Remove mosquitoes that were destroyed or disabled inside the fan
        int removed = affectedMosquitoes.RemoveWhere(
            mosquito => mosquito == null ||
                        !mosquito.gameObject.activeInHierarchy);

        if (removed > 0)
            UpdateAudioState();
    }

    private void OnDisable()
    {
        affectedMosquitoes.Clear();
        UpdateAudioState();
    }

    /// <summary>
    /// Pushes the given mosquito directly away from the fan.
    /// </summary>
    /// <param name="mosquito"></param>
    public void Push(MosquitoAgent mosquito)
    {
        // Switching off stops adding new force, but doesn't erase the force already applied
        if (!effectActive || mosquito == null)
        {
            return;
        }

        Debug.Log("Mosquito detected in Fan");

        // Get the push direction away from the fan.
        Vector3 direction = mosquito.transform.position - pushPoint.position;
        direction.Normalize();
        Debug.DrawRay(pushPoint.position, direction, Color.red);

        mosquito.AddForce(
            direction * strength,
            ForceMode.Force, 
            true); //locking the Y so the fan never pushes the mosquito down
    }

    // Utilize the item effect interface to turn the fan on and off
    public void SetEffectActive(bool active)
    {
        if (effectActive == active)
        {
            return;
        }

        effectActive = active;

        if (!active)
            affectedMosquitoes.Clear();

        UpdateAudioState();
    }

    /// <summary>
    /// Registers a mosquito when it enters the fan's influence area.
    /// </summary>
    /// <param name="other">The collider entering the trigger.</param>
    private void RegisterMosquito(Collider other)
    {
        if (!effectActive || !other.CompareTag("Mosquito"))
            return;

        MosquitoAgent mosquito = other.GetComponentInParent<MosquitoAgent>();

        if (mosquito != null && affectedMosquitoes.Add(mosquito))
            UpdateAudioState();
    }

    /// <summary>
    /// Updates the fan's looping audio based on its affected mosquito count.
    /// </summary>
    private void UpdateAudioState()
    {
        if (audioFeedback != null)
        {
            audioFeedback.SetContinuousActive(
                effectActive && affectedMosquitoes.Count > 0);
        }
    }
}
