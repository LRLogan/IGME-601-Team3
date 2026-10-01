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
    public void OnCollisionEnter(Collision collision)
    {
        GameObject collisionObj = collision.gameObject;
        if(collisionObj.CompareTag("Mosquito"))
        {
            //very hard coded for now will have functionality for multiple mosquitos
            Destroy(collisionObj);
            NextLvlButton.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
