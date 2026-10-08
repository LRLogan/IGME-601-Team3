using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // This instance
    private GameObject managerObjInstance;
    private GameManager instance;

    [SerializeField] private string startingSceneName;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            managerObjInstance = gameObject;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// New game option from start menu
    /// </summary>
    public void NewGame()
    {
        SceneManager.LoadScene(startingSceneName);
    }
}
