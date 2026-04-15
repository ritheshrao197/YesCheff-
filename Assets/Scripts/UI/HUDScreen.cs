// HUDScreen.cs
// Manages only the player HUD: score, timer, held item, interaction prompt, pause.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Player;
using YesChef.Systems;

namespace YesChef.UI
{
    public class HUDScreen : BaseScreen
    {
        [Header("HUD Elements")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text interactionPromptText;
        [SerializeField] private TMP_Text heldItemText;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;

        private void OnEnable()
        {
            if (pauseButton) pauseButton.onClick.AddListener(OnPauseClicked);
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            if (pauseButton) pauseButton.onClick.RemoveListener(OnPauseClicked);
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            TimerSystem.OnTimerTick += UpdateTimer;
            ScoreSystem.OnScoreChanged += UpdateScore;
            ScoreSystem.OnHighScoreChanged += UpdateHighScore;
            PlayerController.OnNearInteractable += ShowPrompt;
            PlayerController.OnLeftInteractable += HidePrompt;
            PlayerController.OnPickedUp += ShowHeldItem;
            PlayerController.OnDropped += HideHeldItem;
        }

        private void UnsubscribeFromEvents()
        {
            TimerSystem.OnTimerTick -= UpdateTimer;
            ScoreSystem.OnScoreChanged -= UpdateScore;
            ScoreSystem.OnHighScoreChanged -= UpdateHighScore;
            PlayerController.OnNearInteractable -= ShowPrompt;
            PlayerController.OnLeftInteractable -= HidePrompt;
            PlayerController.OnPickedUp -= ShowHeldItem;
            PlayerController.OnDropped -= HideHeldItem;
        }

        private void OnPauseClicked()
        {
            GameManager.Instance.PauseGame();
            UIManager.Instance?.ShowScreen<PauseScreen>();
        }

        private void UpdateTimer(float remaining)
        {
            if (timerText == null) return;
            int mins = Mathf.FloorToInt(remaining / 60f);
            int secs = Mathf.FloorToInt(remaining % 60f);
            timerText.text = $"{mins}:{secs:D2}";
            timerText.color = remaining <= 30f ? Color.red : Color.white;
        }

        private void UpdateScore(int score) => SetText(scoreText, $"Score: {score}");
        private void UpdateHighScore(int hs) => SetText(highScoreText, $"Best: {hs}");

        private void ShowPrompt(string prompt)
        {
            if (interactionPromptText)
            {
                interactionPromptText.text = prompt;
                interactionPromptText.gameObject.SetActive(true);
            }
        }

        private void HidePrompt()
        {
            if (interactionPromptText) interactionPromptText.gameObject.SetActive(false);
        }

        private void ShowHeldItem(Ingredient ing)
        {
            if (heldItemText)
            {
                heldItemText.text = $"Holding: {ing.Data.displayName} ({ing.State})";
                heldItemText.gameObject.SetActive(true);
            }
        }

        private void HideHeldItem()
        {
            if (heldItemText) heldItemText.gameObject.SetActive(false);
        }

        private void SetText(TMP_Text text, string value)
        {
            if (text) text.text = value;
        }
    }
}