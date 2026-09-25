using UnityEngine;
using UnityEngine.Rendering;

public class Fan : MonoBehaviour 
{
    private string itemName;
    private float radius;
    private float strength;

    [SerializeField]
    BoxCollider hitbox;

    Rigidbody mosquitoRb;

    public void Start()
    {
        itemName = "fan";
        radius = 15.0f;
        strength = 1f;

        //Change length of fan hitbox to radius size
        hitbox.size = new Vector3(radius, 1,1);
        hitbox.center = new Vector3(radius/2 ,0,0);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mosquito"));
        {
            mosquitoRb = other.GetComponent<Rigidbody>();
            Push(mosquitoRb);

        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Mosquito"));
        {
            mosquitoRb.linearVelocity = Vector3.zero;
        }
    }
    /// <summary>
    /// Pushes the given rigid body directly away from the fan
    /// </summary>
    /// <param name="rb"></param>
    public void Push(Rigidbody rb)
    {
        //Get the correct push direction away from the fan
        Vector3 direction = Vector3.forward;
        direction = Vector3.Normalize(direction);

        rb.AddForce(direction*strength, ForceMode.Impulse);
    }
}
