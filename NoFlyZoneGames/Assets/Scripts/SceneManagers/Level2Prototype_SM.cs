using System;
using UnityEngine;

public class Level2Prototype_SM : MonoBehaviour
{
    [SerializeField] private MosquitoSpawner mosquitoSpawner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Spawn the mosquitos 
        mosquitoSpawner.SpawnMosquito();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
