using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// I may get rid of this class in the future in favor for a more data driven approach 
/// but this one is easier to set up
/// </summary>

public class MosquitoManager : MonoBehaviour
{
    [SerializeField] private Transform pointB;

    private List<MosquitoAgent> mosquitoes = new();

    private void Start()
    {
        mosquitoes.AddRange(
            FindObjectsByType<MosquitoAgent>(
                FindObjectsSortMode.None));

        //MoveAllTo(pointB.position);
    }

    #region Public acess point controls
    public void SetAllTask(MosquitoAgent.Task task)
    {
        foreach (MosquitoAgent mosquito in mosquitoes)
        {
            mosquito.SetTask(task);
        }
    }

    public void MoveAllTo(Vector3 target)
    {
        foreach (MosquitoAgent mosquito in mosquitoes)
        {
            mosquito.MoveTo(target);
        }
    }

    public void MoveCollectionTo(List<MosquitoAgent> agents, Vector3 target)
    {
        foreach (MosquitoAgent mosquito in agents)
        {
            mosquito.MoveTo(target);
        }
    }

    public void MoveOneTo(MosquitoAgent mosquito, Vector3 target)
    {
        mosquito.MoveTo(target);
    }

    public void FleeAllFrom(Vector3 target)
    {
        foreach (MosquitoAgent mosquito in mosquitoes)
        {
            mosquito.FleeFrom(target);
        }
    }

    public void RegisterMosquito(MosquitoAgent mosquito)
    {
        if (!mosquitoes.Contains(mosquito))
            mosquitoes.Add(mosquito);
    }

    public void UnregisterMosquito(MosquitoAgent mosquito)
    {
        mosquitoes.Remove(mosquito);
    }
    #endregion
}