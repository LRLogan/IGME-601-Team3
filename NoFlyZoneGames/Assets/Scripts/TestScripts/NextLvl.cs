using UnityEngine;

public class NextLvl : MonoBehaviour
{
    [SerializeField]
    GameObject player;
    [SerializeField]
    GameObject spawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPress()
    {
        Debug.Log("teleport");
        //very hard coded for now
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = spawn.transform.position;
        player.GetComponent<CharacterController>().enabled = true;
        gameObject.SetActive(false);

    }
}
