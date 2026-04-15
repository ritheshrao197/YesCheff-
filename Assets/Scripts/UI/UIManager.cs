// UIManager.cs
// Maps game state and gameplay events to presentation screens.

using UnityEngine;
using YesChef.Core;

namespace YesChef.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private StartScreen startScreen;
        [SerializeField] private HUDScreen hudScreen;
        [SerializeField] private PauseScreen pauseScreen;
        [SerializeField] private GameOverScreen gameOverScreen;

        private void OnEnable()
        {
            GameEvents.GameStateChanged += HandleGameStateChanged;
            GameEvents.GameEnded += HandleGameEnded;
        }

        private void OnDisable()
        {
            GameEvents.GameStateChanged -= HandleGameStateChanged;
            GameEvents.GameEnded -= HandleGameEnded;
        }

        private void Start()
        {
            ShowScreen<GameOverScreen>(false);
            ShowScreen<PauseScreen>(false);
            ShowScreen<HUDScreen>(false);
            ShowScreen<StartScreen>(true);
        }

        public void ShowScreen<T>(bool visible = true) where T : BaseScreen
        {
            ToggleScreen(startScreen, typeof(T) == typeof(StartScreen) && visible);
            ToggleScreen(hudScreen, typeof(T) == typeof(HUDScreen) && visible);
            ToggleScreen(pauseScreen, typeof(T) == typeof(PauseScreen) && visible);
            ToggleScreen(gameOverScreen, typeof(T) == typeof(GameOverScreen) && visible);
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Start:
                    ShowScreen<StartScreen>();
                    break;
                case GameState.Playing:
                    ShowScreen<HUDScreen>();
                    break;
                case GameState.Paused:
                    ShowScreen<PauseScreen>();
                    break;
                case GameState.End:
                    ShowScreen<GameOverScreen>();
                    break;
            }
        }

        private void HandleGameEnded(bool isNewHighScore, int finalScore)
        {
            if (gameOverScreen != null)
            {
                gameOverScreen.ShowFinalScore(finalScore, isNewHighScore);
            }
        }

        private static void ToggleScreen(BaseScreen screen, bool shouldShow)
        {
            if (screen == null)
            {
                return;
            }

            if (shouldShow)
            {
                screen.Show();
            }
            else
            {
                screen.Hide();
            }
        }
    }
}
