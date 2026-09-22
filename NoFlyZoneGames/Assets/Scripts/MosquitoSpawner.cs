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

        Instantiate(
            mosquitoPrefab,
            transform.position,
            transform.rotation
        );
    }
}