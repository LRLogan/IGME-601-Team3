using UnityEngine;
using UnityEngine.UI;

public class Window : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider collider)
    {
        GameObject collisionObj = collider.gameObject;
        //Makes sure collision is with mosquito
        if(collisionObj.CompareTag("Mosquito"))
        {
            Debug.Log("Huzzah");
            //Deletes the mosquito on contact 
            Destroy(collisionObj);
        }
    }
}
