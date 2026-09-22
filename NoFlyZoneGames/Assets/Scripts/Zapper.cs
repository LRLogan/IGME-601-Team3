using UnityEngine;

public class Zapper : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mosquito"))
        {
            Debug.Log("Mosquito entered zapper range");
        }
    }
}
