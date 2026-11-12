using UnityEngine;
using UnityEngine.SceneManagement;


public class Tutorial_SM : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    [SerializeField]
    GameObject spawn1;

    [SerializeField] 
    GameObject spawn2;

    [SerializeField]
    int MainLevelSceneIndex = 2;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       //Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToPartTwo()
    {
        //very hard coded for now
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = spawn1.transform.position;
        player.GetComponent<CharacterController>().enabled = true;
    }
    public void ToPartThree()
    {
        //very hard coded for now
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = spawn2.transform.position;
        player.GetComponent<CharacterController>().enabled = true;
    }


    public void ToMainLevel()
    {
        SceneManager.LoadScene(MainLevelSceneIndex);
    }
}
