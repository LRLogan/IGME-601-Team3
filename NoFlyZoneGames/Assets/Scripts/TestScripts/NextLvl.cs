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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPress()
    {
        Debug.Log("yo momm");
        //very hard coded for now
        player.transform.position = spawn.transform.position;
    }
}
