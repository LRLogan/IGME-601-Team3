using UnityEngine;

public class HealthBarBillboard : MonoBehaviour
{
    private Camera playerCamera;

    private void LateUpdate()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        if (playerCamera == null)
        {
            return;
        }

        // Keep health bar parallel to camera's screen
        transform.rotation = playerCamera.transform.rotation;
    }
}
