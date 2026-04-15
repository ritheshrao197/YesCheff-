// GameOverScreen.cs
// Displays the final score and post-game actions.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;

namespace YesChef.UI
{
    public class GameOverScreen : BaseScreen
    {
        private const string FinalScoreFormat = "Final Score: {0}";

        [Header("Game Over Screen")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private GameObject newHighScoreLabel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
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
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void OnDisable()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
            }
        }

        public void ShowFinalScore(int score, bool isNewHighScore)
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = string.Format(FinalScoreFormat, score);
            }

            if (newHighScoreLabel != null)
            {
                newHighScoreLabel.SetActive(isNewHighScore);
            }
        }

        private void OnRestartClicked()
        {
            gameManager?.RestartGame();
        }

        private void OnQuitClicked()
        {
            gameManager?.QuitGame();
        }
    }
}
