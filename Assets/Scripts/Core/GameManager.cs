// GameManager.cs
// Central orchestrator. Drives the high-level game state machine:
// MainMenu → Playing → Paused → GameOver → (restart)

using UnityEngine;
using YesChef.Orders;
using YesChef.Player;
using YesChef.Stations;
using YesChef.Systems;

namespace YesChef.Core
{
    public enum GameState { MainMenu, Playing, Paused, GameOver }

    public class GameManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── Inspector refs ────────────────────────────────────────────────
        [Header("Systems (assign in Inspector)")]
        [SerializeField] private TimerSystem  timerSystem;
        [SerializeField] private ScoreSystem  scoreSystem;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private PlayerController playerController;

        // ── State ─────────────────────────────────────────────────────────
        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        // ── Unity lifecycle ───────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            GameLogger.Info(GameLogCategory.Game, "GameManager initialised.", this);
        }

        private void OnEnable()
        {
            TimerSystem.OnTimerExpired      += HandleTimerExpired;
            OrderManager.OnOrderCompleted   += HandleOrderCompleted;
            CustomerWindow.OnDeliveryScored += HandleDeliveryScored;
        }

        private void OnDisable()
        {
            TimerSystem.OnTimerExpired      -= HandleTimerExpired;
            OrderManager.OnOrderCompleted   -= HandleOrderCompleted;
            CustomerWindow.OnDeliveryScored -= HandleDeliveryScored;
        }

        // ── Public API ────────────────────────────────────────────────────
        public void StartGame()
        {
            if (CurrentState == GameState.Playing) return;
            GameLogger.Info(GameLogCategory.Game, "StartGame requested.", this);
            TransitionTo(GameState.Playing);
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;
            GameLogger.Info(GameLogCategory.Game, "PauseGame requested.", this);
            TransitionTo(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;
            GameLogger.Info(GameLogCategory.Game, "ResumeGame requested.", this);
            TransitionTo(GameState.Playing);
        }

        public void RestartGame()
        {
            GameLogger.Info(GameLogCategory.Game, "RestartGame requested.", this);
            TransitionTo(GameState.Playing);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ── Transitions ───────────────────────────────────────────────────
        private void TransitionTo(GameState newState)
        {
            GameLogger.Info(GameLogCategory.Game, $"State transition {CurrentState} -> {newState}.", this);
            CurrentState = newState;

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    scoreSystem.ResetScore();
                    timerSystem.StartTimer();
                    orderManager.StartOrders();
                    playerController.SetGameRunning(true);
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    timerSystem.SetPaused(true);
                    playerController.SetGameRunning(false);
                    break;

                case GameState.GameOver:
                    Time.timeScale = 1f;
                    timerSystem.StopTimer();
                    orderManager.StopOrders();
                    playerController.SetGameRunning(false);
                    scoreSystem.FinaliseGame();
                    break;

                case GameState.MainMenu:
                    Time.timeScale = 0f;
                    playerController.SetGameRunning(false);
                    break;
            }
        }

        private void HandleTimerExpired()
        {
            GameLogger.Info(GameLogCategory.Game, "Timer expired. Moving to GameOver.", this);
            TransitionTo(GameState.GameOver);
        }

        // OrderManager fires this — we don't add score here to avoid double-counting
        // (CustomerWindow.OnDeliveryScored is the canonical score source)
        private void HandleOrderCompleted(Order order, int score)
        {
            GameLogger.Info(GameLogCategory.Orders, $"{GameLogger.DescribeOrder(order)} completed for {score} points.", this);
        }

        private void HandleDeliveryScored(int windowIndex, int score)
        {
            GameLogger.Info(GameLogCategory.Score, $"Window {windowIndex} awarded {score} points.", this);
            scoreSystem.AddScore(score);
        }
    }

    public enum GameLogCategory
    {
        Game,
        Player,
        Stations,
        Orders,
        Timer,
        Score,
        Ingredients
    }

    public static class GameLogger
    {
        public static bool EnableInfoLogs = true;
        public static bool EnableWarningLogs = true;
        public static bool EnableVerboseLogs = false;

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Info(GameLogCategory category, string message, UnityEngine.Object context = null)
        {
            if (!EnableInfoLogs) return;
            Debug.Log(Format(category, message), context);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(GameLogCategory category, string message, UnityEngine.Object context = null)
        {
            if (!EnableWarningLogs) return;
            Debug.LogWarning(Format(category, message), context);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Verbose(GameLogCategory category, string message, UnityEngine.Object context = null)
        {
            if (!EnableVerboseLogs) return;
            Debug.Log(Format(category, message), context);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Error(GameLogCategory category, string message, UnityEngine.Object context = null)
        {
            Debug.LogError(Format(category, message), context);
        }

        public static string DescribeIngredient(Ingredients.Ingredient ingredient)
        {
            if (ingredient == null)
            {
                return "none";
            }

            string name = ingredient.Data != null ? ingredient.Data.displayName : ingredient.name;
            return $"{name} ({ingredient.State})";
        }

        public static string DescribeOrder(Orders.Order order)
        {
            if (order == null)
            {
                return "none";
            }

            return $"Order #{order.Id} [{order.FulfilledIngredients.Count}/{order.RequiredIngredients.Count}]";
        }

        private static string Format(GameLogCategory category, string message)
        {
            return $"[YesChef][{category}] {message}";
        }
    }
}
