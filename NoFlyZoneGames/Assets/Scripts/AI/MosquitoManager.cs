using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// I may get rid of this class in the future in favor for a more data driven approach 
/// but this one is easier to set up
/// </summary>

public class MosquitoManager : MonoBehaviour
{
    [SerializeField] private Transform pointB;

    private List<MosquitoAgent> activeMosquitoes = new();
    private Stack<MosquitoAgent> mosquitoPool = new();

    private void Start()
    {
        activeMosquitoes.AddRange(
            FindObjectsByType<MosquitoAgent>(
                FindObjectsSortMode.None));

        MoveAllTo(pointB.position);
    }

    private void InstantiateMosquitos()
    {
        activeMosquitoes.Clear();
    }

    #region Public acess point controls
    public void SpawnMosquito()
    {
        // TODO: If there is a mosquito in the pool then use that, otherwise add a new one to the pool and sapwn it.
    }

    public void DespawnMosquito()
    {
        // TODO: Deactivate mosquito and sent it bcak to the pool
    }

    public void SetAllTask(MosquitoAgent.Task task)
    {
        foreach (MosquitoAgent mosquito in activeMosquitoes)
        {
            mosquito.SetTask(task);
        }
    }

    public void MoveAllTo(Vector3 target)
    {
        foreach (MosquitoAgent mosquito in activeMosquitoes)
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
        foreach (MosquitoAgent mosquito in activeMosquitoes)
        {
            mosquito.FleeFrom(target);
        }
    }
    #endregion
}