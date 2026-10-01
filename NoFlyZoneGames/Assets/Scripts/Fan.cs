using UnityEngine;

public class Fan : MonoBehaviour
{
    private string itemName;
    [SerializeField]
    private float radius;
    [SerializeField]
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
    /// Pushes the given rigid body directly away from the fan
    /// </summary>
    /// <param name="rb"></param>
    public void Push(Rigidbody rb)
    {
        //Get the correct push direction away from the fan
        Vector3 direction = rb.position - transform.position;
        direction.normalize();
        rb.AddForce(direction * strength, ForceMode.Impulse);
    }
}
