using UnityEngine;

public class Fan : MonoBehaviour, IItemEffect
{
    private bool effectActive = true;
    private string itemName;
    private float radius;
    private float strength;

    [SerializeField]
    private BoxCollider hitbox;

    public void Start()
    {
        itemName = "fan";
        radius = 15.0f;
        strength = 5f;

        // Change length of fan hitbox to radius size
        hitbox.size = new Vector3(radius, 1, 1);
        hitbox.center = new Vector3(radius / 2, 0, 0);
    }

    public void OnTriggerStay(Collider other)
    {
        if (!effectActive)
        {
            return;
        }

        if (other.CompareTag("Mosquito"))
        {
            Debug.Log("Mosquito detected!");
            MosquitoAgent mosquito = other.GetComponent<MosquitoAgent>();

            if (mosquito != null)
            {
                Push(mosquito);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Mosquito"))
        {
            //mosquitoRb.linearVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Pushes the given mosquito directly away from the fan.
    /// </summary>
    /// <param name="mosquito"></param>
    public void Push(MosquitoAgent mosquito)
    {
        // Switching off stops adding new force, but doesn't erase the force already applied
        if (!effectActive)
        {
            return;
        }

        // Get the push direction away from the fan.
        Vector3 direction = Vector3.forward;
        direction.Normalize();

        mosquito.AddForce(
            direction * strength,
            ForceMode.Force);
    }

    // Utilize the item effect interface to turn the fan on and off
    public void SetEffectActive(bool active)
    {
        if (effectActive == active)
        {
            return;
        }

        effectActive = active;
    }
}
