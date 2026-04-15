// HUDScreen.cs
// Manages the gameplay HUD and listens to events only.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Ingredients;

namespace YesChef.UI
{
    public class HUDScreen : BaseScreen
    {
        private const float SecondsPerMinute = 60f;
        private const float LowTimeWarningSeconds = 30f;
        private const string TimerFormat = "{0}:{1:D2}";
        private const string ScoreFormat = "Score: {0}";
        private const string BestScoreFormat = "Best: {0}";
        private const string HeldItemFormat = "Holding: {0} ({1})";

        [Header("HUD Elements")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text interactionPromptText;
        [SerializeField] private TMP_Text heldItemText;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameManager gameManager;

        protected override void Awake()
        {
            base.Awake();

            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
        }

        private void OnEnable()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }

            GameEvents.TimerTicked += UpdateTimer;
            GameEvents.ScoreChanged += UpdateScore;
            GameEvents.HighScoreChanged += UpdateHighScore;
            GameEvents.InteractionPromptChanged += ShowPrompt;
            GameEvents.InteractionPromptCleared += HidePrompt;
            GameEvents.PlayerPickedUpIngredient += ShowHeldItem;
            GameEvents.PlayerDroppedIngredient += HideHeldItem;
        }

        private void OnDisable()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseClicked);
            }

            GameEvents.TimerTicked -= UpdateTimer;
            GameEvents.ScoreChanged -= UpdateScore;
            GameEvents.HighScoreChanged -= UpdateHighScore;
            GameEvents.InteractionPromptChanged -= ShowPrompt;
            GameEvents.InteractionPromptCleared -= HidePrompt;
            GameEvents.PlayerPickedUpIngredient -= ShowHeldItem;
            GameEvents.PlayerDroppedIngredient -= HideHeldItem;
        }

        private void OnPauseClicked()
        {
            gameManager?.PauseGame();
        }

        private void UpdateTimer(float remaining)
        {
            if (timerText == null)
            {
                return;
            }

            int mins = Mathf.FloorToInt(remaining / SecondsPerMinute);
            int secs = Mathf.FloorToInt(remaining % SecondsPerMinute);
            timerText.text = string.Format(TimerFormat, mins, secs);
            timerText.color = remaining <= LowTimeWarningSeconds ? Color.red : Color.white;
        }

        private void UpdateScore(int score) => SetText(scoreText, string.Format(ScoreFormat, score));
        private void UpdateHighScore(int highScore) => SetText(highScoreText, string.Format(BestScoreFormat, highScore));

        private void ShowPrompt(string prompt)
        {
            if (interactionPromptText == null)
            {
                return;
            }

            interactionPromptText.text = prompt;
            interactionPromptText.gameObject.SetActive(true);
        }

        private void HidePrompt()
        {
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }

        private void ShowHeldItem(Ingredient ingredient)
        {
            if (heldItemText == null || ingredient == null)
            {
                return;
            }

            heldItemText.text = string.Format(HeldItemFormat, ingredient.Data.displayName, ingredient.State);
            heldItemText.gameObject.SetActive(true);
        }

        private void HideHeldItem()
        {
            if (heldItemText != null)
            {
                heldItemText.gameObject.SetActive(false);
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
