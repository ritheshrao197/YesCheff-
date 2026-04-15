// GameOverScreen.cs
// Manages the game over screen: final score, new high score label, restart/quit.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YesChef.Systems;
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

        private ScoreSystem scoreSystem;

        private void OnEnable()
        {
            if (restartButton) restartButton.onClick.AddListener(OnRestartClicked);
            if (quitButton) quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnDisable()
        {
            if (restartButton) restartButton.onClick.RemoveListener(OnRestartClicked);
            if (quitButton) quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        public void ShowFinalScore(int score, bool isNewHighScore)
        {
            if (finalScoreText) finalScoreText.text = string.Format(FinalScoreFormat, score);
            if (newHighScoreLabel) newHighScoreLabel.SetActive(isNewHighScore);
        }

        private void OnRestartClicked()
        {
            GameManager.Instance.RestartGame();
            // After restart, the game will show start screen again
            UIManager.Instance?.ShowScreen<StartScreen>();
        }

        private void OnQuitClicked()
        {
            GameManager.Instance.QuitGame();
        }
    }
}
