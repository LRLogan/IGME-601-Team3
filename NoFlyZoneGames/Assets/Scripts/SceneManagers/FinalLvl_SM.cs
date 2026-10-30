using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class FinalLvl_SM : MonoBehaviour
{
    [SerializeField] private MosquitoSpawner mosquitoSpawner;
    [SerializeField] private List<int> numMosquitos = new();
    

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
