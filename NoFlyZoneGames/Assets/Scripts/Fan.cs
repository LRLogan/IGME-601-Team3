using System.Threading.Tasks;
using UnityEngine;

public class Fan : MonoBehaviour, IItemEffect
{
    private bool effectActive = true;
    private string itemName;
    [SerializeField]
    private float radius;
    [SerializeField]
    private float strength;

    Transform pushPoint;

    //[SerializeField]
    //private CapsuleCollider hitbox; // Hastag this isn't used?
    //private BoxCollider hitbox;
    public void Awake()
    {

        pushPoint = transform.Find("PushPoint");
    }

    public void Start()
    {
        itemName = "fan";
    }

    private void OnTriggerStay(Collider other)
    {
        if (!effectActive)
            return;

        if (!other.CompareTag("Mosquito"))
            return;

        MosquitoAgent mosquito = other.GetComponentInParent<MosquitoAgent>();

        if (mosquito != null)
            Push(mosquito);
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
