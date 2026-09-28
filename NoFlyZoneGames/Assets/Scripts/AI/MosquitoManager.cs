using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// I may get rid of this class in the future in favor for a more data driven approach 
/// but this one is easier to set up
/// </summary>
public class MosquitoManager : MonoBehaviour
{
    [SerializeField] private Transform pointB;

    private readonly List<MosquitoAgent> mosquitoes = new();

    private void Start()
    {
        mosquitoes.AddRange(
            FindObjectsByType<MosquitoAgent>(
                FindObjectsSortMode.None));

        foreach (MosquitoAgent mosquito in mosquitoes)
            mosquito.MoveTo(pointB.position);
    }
}
