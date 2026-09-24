using UnityEngine;

public class Fan : Traps
{
    [SerializeField]
    BoxCollider hitbox;
    
    void Start()
    {
        name = "fan";
        radius = 15.0f;
        strength = 1f;
        //Change length fan can hit to radius size
        hitbox.size = new Vector3(radius, 1,1);
        hitbox.center = new Vector3(radius/2 ,0,0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mosquito"));
        {
            Rigidbody mosquitoRb = other.GetComponent<Rigidbody>();
            Push(mosquitoRb);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Mosquito"));
        {
            other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }
    }
    /// <summary>
    /// Pushes the given rigid body directly away from the fan
    /// </summary>
    /// <param name="rb"></param>
    public void Push(Rigidbody rb)
    {
        //Get the correct push direction away from the fan
        Vector3 direction = transform.right;
        direction = Vector3.Normalize(direction);

        rb.AddForce(direction*strength, ForceMode.Impulse);
    }
}
