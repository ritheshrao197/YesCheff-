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

        // Cached state to avoid redundant UI updates
        private int lastScore = -1;
        private int lastHighScore = -1;
        private string lastTimerString = "";
        private Color lastTimerColor = Color.white;
        private string lastHeldItemString = "";
        private bool isPromptActive = false;
        private bool isHeldItemActive = false;

        protected override void Awake()
        {
            base.Awake();

            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
        }

        private void OnEnable()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);

            // Subscribe to game events
            GameEvents.TimerTicked += UpdateTimer;
            GameEvents.ScoreChanged += UpdateScore;
            GameEvents.HighScoreChanged += UpdateHighScore;
            GameEvents.InteractionPromptChanged += ShowPrompt;
            GameEvents.InteractionPromptCleared += HidePrompt;
            GameEvents.PlayerPickedUpIngredient += ShowHeldItem;
            GameEvents.PlayerDroppedIngredient += HideHeldItem;

            // Reset visual state (but do NOT overwrite actual score/highscore values)
            ResetScreen();
        }

        private void OnDisable()
        {
            if (pauseButton != null)
                pauseButton.onClick.RemoveListener(OnPauseClicked);

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

       
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                OnPauseClicked();
        }

        private void UpdateTimer(float remaining)
        {
            if (timerText == null) return;

            int mins = Mathf.FloorToInt(remaining / SecondsPerMinute);
            int secs = Mathf.FloorToInt(remaining % SecondsPerMinute);
            string newTimerString = string.Format(TimerFormat, mins, secs);
            Color newColor = remaining <= LowTimeWarningSeconds ? Color.red : Color.white;

            // Only update text and color if they actually changed
            if (newTimerString != lastTimerString)
            {
                timerText.text = newTimerString;
                lastTimerString = newTimerString;
            }
            if (newColor != lastTimerColor)
            {
                timerText.color = newColor;
                lastTimerColor = newColor;
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText == null) return;
            if (score == lastScore) return; // No change, skip update

            scoreText.text = string.Format(ScoreFormat, score);
            lastScore = score;
        }

        private void UpdateHighScore(int highScore)
        {
            if (highScoreText == null) return;
            if (highScore == lastHighScore) return;

            highScoreText.text = string.Format(BestScoreFormat, highScore);
            lastHighScore = highScore;
        }

        private void ShowPrompt(string prompt)
        {
            if (interactionPromptText == null) return;

            interactionPromptText.text = prompt;
            if (!isPromptActive)
            {
                interactionPromptText.gameObject.SetActive(true);
                isPromptActive = true;
            }
        }

        private void HidePrompt()
        {
            if (interactionPromptText == null) return;
            if (!isPromptActive) return;

            interactionPromptText.gameObject.SetActive(false);
            isPromptActive = false;
        }

        private void ShowHeldItem(Ingredient ingredient)
        {
            if (heldItemText == null || ingredient == null) return;

            string newHeldString = string.Format(HeldItemFormat, ingredient.Data.displayName, ingredient.State);
            if (newHeldString != lastHeldItemString)
            {
                heldItemText.text = newHeldString;
                lastHeldItemString = newHeldString;
            }

            if (!isHeldItemActive)
            {
                heldItemText.gameObject.SetActive(true);
                isHeldItemActive = true;
            }
        }

        private void HideHeldItem()
        {
            if (heldItemText == null) return;
            if (!isHeldItemActive) return;

            heldItemText.gameObject.SetActive(false);
            isHeldItemActive = false;
            lastHeldItemString = ""; // Reset so next pickup won't compare stale value
        }

        public override void ResetScreen()
        {
            // Reset only the visual state – do NOT overwrite actual score/highscore values.
            // The real values will be pushed via ScoreChanged/HighScoreChanged events shortly.
            if (scoreText != null && lastScore != 0)
            {
                scoreText.text = string.Format(ScoreFormat, 0);
                lastScore = 0;
            }
            HidePrompt();
            HideHeldItem();

            // Timer and highscore will be updated by their respective events.
            // Optionally reset cached timer string to force first update.
            lastTimerString = "";
        }
    }
}