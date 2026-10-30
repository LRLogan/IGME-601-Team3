using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUDController : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private Image[] strikeIcons;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite brokenHeart;

    [Header("Countdown")]
    [SerializeField] private GameObject countdownGroup;
    [SerializeField] private TMP_Text countdownLabel;
    [SerializeField] private TMP_Text countdownText;

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

    public void ShowCountdown(bool visible)
    {
        if (countdownGroup == null) return;

        countdownGroup.SetActive(visible);
    }

    public void SetCountdown(float seconds)
    {
        if (countdownText == null) return;

        int time = Mathf.CeilToInt(Mathf.Max(0f, seconds));

        int minutes = time / 60;
        int remainingSeconds = time % 60;

        countdownText.text = $"{minutes:00}:{remainingSeconds:00}";
    }
    
    public void SetCountdownLabel(string label)
    {
        if (countdownLabel == null) return;

        countdownLabel.text = label;
    }

    // Update strike icons.
    public void SetStrikes(int remaining, int maximum)
    {
        remaining = Mathf.Clamp(remaining, 0, maximum);

        for (int i = 0; i < strikeIcons.Length; i++)
        {
            strikeIcons[i].gameObject.SetActive(i < maximum);

            if (i < maximum)
            {
                strikeIcons[i].sprite =
                    i < remaining ? fullHeart : brokenHeart;
            }
        }
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
