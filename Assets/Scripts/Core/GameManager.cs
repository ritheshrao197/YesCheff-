// GameManager.cs
// Coordinates high-level session state and system lifecycle.

using UnityEngine;
using YesChef.Orders;
using YesChef.Player;
using YesChef.Systems;

namespace YesChef.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private TimerSystem timerSystem;
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private StationSystem stationSystem;
        [SerializeField] private PlayerController playerController;



        public GameState CurrentState { get; private set; } = GameState.Start;

        private void Awake()
        {
            GameLogger.Info(GameLogCategory.Game, "GameManager initialised.", this);
            ApplyState(CurrentState, forceReset: true);
        }

        private void OnEnable()
        {
            GameEvents.TimerExpired += HandleTimerExpired;
            GameEvents.DeliveryScored += HandleDeliveryScored;
        }

        private void OnDisable()
        {
            GameEvents.TimerExpired -= HandleTimerExpired;
            GameEvents.DeliveryScored -= HandleDeliveryScored;
        }
        public void StartGame()
        {
            if (CurrentState == GameState.Playing)
            {
                return;
            }

            TransitionTo(GameState.Playing, forceReset: true);
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                TransitionTo(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                TransitionTo(GameState.Playing);
            }
        }

        public void RestartGame()
        {
            TransitionTo(GameState.Start, forceReset: true);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void TransitionTo(GameState newState, bool forceReset = false)
        {
            if (CurrentState == newState && !forceReset)
            {
                return;
            }

            GameLogger.Info(GameLogCategory.Game, $"State transition {CurrentState} -> {newState}.", this);
            CurrentState = newState;
            ApplyState(newState, forceReset);
            GameEvents.RaiseGameStateChanged(CurrentState);
        }

        private void ApplyState(GameState state, bool forceReset = false)
        {
            switch (state)
            {
                case GameState.Start:
                    Time.timeScale = 0f;
                    timerSystem.StopTimer();
                    orderManager.ResetOrders();
                    playerController.SetGameRunning(false);
                    scoreSystem.ResetScore();
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;

                    if (forceReset || timerSystem.Remaining <= 0f || !timerSystem.IsRunning)
                    {
                        scoreSystem.ResetScore();
                        orderManager.StartOrders();
                        timerSystem.StartTimer();
                        stationSystem.ResetStations();
                    }
                    else
                    {
                        timerSystem.SetPaused(false);
                    }

                    playerController.SetGameRunning(true);
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    timerSystem.SetPaused(true);
                    playerController.SetGameRunning(false);
                    break;

                case GameState.End:
                    Time.timeScale = 1f;
                    timerSystem.StopTimer();
                    orderManager.StopOrders();
                    playerController.SetGameRunning(false);
                    scoreSystem.FinaliseGame();
                    break;
            }
        }

        private void HandleTimerExpired()
        {
            TransitionTo(GameState.End);
        }

        private void HandleDeliveryScored(int windowIndex, int score)
        {
            GameLogger.Info(GameLogCategory.Score, $"Window {windowIndex} awarded {score} points.", this);
            scoreSystem.AddScore(score);
        }
    }
}
