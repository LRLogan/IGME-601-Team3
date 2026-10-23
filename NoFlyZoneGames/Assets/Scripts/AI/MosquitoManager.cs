using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// I may get rid of this class in the future in favor for a more data driven approach 
/// but this one is easier to set up
/// </summary>

public class MosquitoManager : MonoBehaviour
{
    public Transform pointB;
    [SerializeField] private GameObject mosquitoPrefab;
    [SerializeField] private int initialPoolSize = 10;

    private List<MosquitoAgent> activeMosquitoes = new();
    private Stack<MosquitoAgent> mosquitoPool = new();

    private void Start()
    {
        /*
        activeMosquitoes.AddRange(
            FindObjectsByType<MosquitoAgent>(
                FindObjectsSortMode.None));

        MoveAllTo(pointB.position);
        */
        InstantiateMosquitos();
    }

    /// <summary>
    /// Called once at start to populate the agent object pool
    /// </summary>
    private void InstantiateMosquitos()
    {
        activeMosquitoes.Clear();
        mosquitoPool.Clear();

        if (mosquitoPrefab == null)
        {
            Debug.LogWarning("Mosquito prefab is not assigned.", this);
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject instance = Instantiate(
                mosquitoPrefab,
                transform.position,
                Quaternion.identity
            );

            MosquitoAgent mosquito = instance.GetComponent<MosquitoAgent>();

            if (mosquito == null)
            {
                Debug.LogError("Mosquito prefab is missing MosquitoAgent.", this);
                Destroy(instance);
                continue;
            }

            mosquito.ResetAgent();
            instance.SetActive(false);

            mosquitoPool.Push(mosquito);
        }
    }

    #region Public acess point controls
    public MosquitoAgent SpawnMosquito(Vector3 position, Quaternion rotation)
    {
        MosquitoAgent mosquito;

        if (mosquitoPool.Count > 0)
        {
            // Reuse an existing mosquito.
            mosquito = mosquitoPool.Pop();

            mosquito.transform.SetPositionAndRotation(position, rotation);
            mosquito.gameObject.SetActive(true);
        }
        else
        {
            // No pooled mosquitoes available. Create a new one.
            if (mosquitoPrefab == null)
            {
                Debug.LogWarning("Mosquito prefab is not assigned.", this);
                return null;
            }

            GameObject instance = Instantiate(
                mosquitoPrefab,
                position,
                rotation
            );

            mosquito = instance.GetComponent<MosquitoAgent>();

            if (mosquito == null)
            {
                Debug.LogError("Mosquito prefab has no MosquitoAgent.", this);
                Destroy(instance);
                return null;
            }
        }

        activeMosquitoes.Add(mosquito);

        return mosquito;
    }

    // Preserved for other scripts
    public void DespawnMosquito(MosquitoAgent mosquito)
    {
        if (mosquito == null || !activeMosquitoes.Remove(mosquito))
            return;

        // Complete value reset before returning to pool
        mosquito.ResetAgent();

        mosquito.gameObject.SetActive(false);
        mosquitoPool.Push(mosquito);
    }

    public bool TryDespawnMosquito(MosquitoAgent mosquito)
    {
        // Return true only when active mosquito was returned to pool
        if (mosquito == null || !activeMosquitoes.Remove(mosquito))
        {
            return false;
        }
        mosquito.ResetAgent();
        mosquito.gameObject.SetActive(false);
        mosquitoPool.Push(mosquito);
        return true;
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

    public void MoveOneTo(MosquitoAgent mosquito, Transform target)
    {
        mosquito.MoveTo(
            new Vector3(target.position.x,
            target.position.y,
            target.position.z));
    }

    public void MoveOneTo(MosquitoAgent mosquito, Transform target, float stopDistance)
    {
        // Add this overload
        mosquito.MoveTo(target.position, stopDistance);
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