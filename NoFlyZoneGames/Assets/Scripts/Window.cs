using UnityEngine;
using UnityEngine.UI;

public class Window : MonoBehaviour
{
    [SerializeField]
    private Button NextLvlButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NextLvlButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider collider)
    {
        GameObject collisionObj = collider.gameObject;
        if(collisionObj.CompareTag("Mosquito"))
        {
            Debug.Log("Huzzah");
            //Deletes the mosquito on contact 
            Destroy(collisionObj);

            //hard coded graybox level stuff to move to next section
            NextLvlButton.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
