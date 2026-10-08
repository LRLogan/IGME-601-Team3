using UnityEngine;
using TMPro;

public class PlayerHUDController : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text strikeText;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TMP_Text actionText;

    [Header("Player")]
    [SerializeField] private PlayerInteraction playerInteraction;

    private void Start()
    {
        if (playerInteraction == null)
        {
            playerInteraction =
                FindFirstObjectByType<PlayerInteraction>();
        }

        UpdateInteractionPrompt();
    }

    private void Update()
    {
        UpdateInteractionPrompt();
    }

    // Update the wave display.
    public void SetWave(int current, int total)
    {
        if (waveText == null) return;

        waveText.text = $"Wave {current}/{total}";
    }

    // Update strike icons.
    public void SetStrikes(int remaining, int maximum)
    {
        if (strikeText == null) return;

        maximum = Mathf.Max(0, maximum);
        remaining = Mathf.Clamp(remaining, 0, maximum);

        string icons = "";

        for (int i = 0; i < maximum; i++)
        {
            icons += i < remaining ? "●" : "○";

            if (i < maximum - 1)
                icons += " ";
        }

        strikeText.text = $"STRIKES: {icons}";
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionPrompt == null || actionText == null)
            return;

        if (playerInteraction == null)
        {
            ShowPrompt(null);
            return;
        }

        if (playerInteraction.IsHoldingItem)
        {
            ShowPrompt(
                playerInteraction.CanPlaceHeldItem
                    ? "Place"
                    : null
            );
        }
        else
        {
            ShowPrompt(
                playerInteraction.CanPickUpItem()
                    ? "Pick Up"
                    : null
            );
        }
    }

    private void ShowPrompt(string action)
    {
        bool visible = !string.IsNullOrEmpty(action);

        if (visible)
            actionText.text = action;

        if (interactionPrompt.activeSelf != visible)
            interactionPrompt.SetActive(visible);
    }
}
