using UnityEngine;
using UnityEngine.InputSystem;

public class MosquitoSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject mosquitoPrefab;

    // temporary test hook
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SpawnMosquito();
        }
    }

    public void SpawnMosquito()
    {
        if (mosquitoPrefab == null)
        {
            Debug.LogWarning("Mosquito prefab is not assigned.", this);
            return;
        }

        // TODO: Spawn the mosquito in a set radius to the spawner to allow variation.
        // Be sure to check for collisions and not spawn it in that location if so

        Instantiate(
            mosquitoPrefab,
            transform.position,
            transform.rotation
        );

        // TODO: Add the mosquito to the active mosquitos list in masquito manager and remove it from the pool
    }
}