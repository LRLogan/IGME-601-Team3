using UnityEngine;
using UnityEngine.InputSystem;

public class TitleSceneController : MonoBehaviour
{
    private bool openingMenu;

    private void Update()
    {
        if (openingMenu || Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return;
        }

        // Escape is the manager's current quit behavior
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("Start the title screen with a GameManager.", this);
            return;
        }

        openingMenu = true;
        GameManager.Instance.OpenMainMenu();
    }
}
