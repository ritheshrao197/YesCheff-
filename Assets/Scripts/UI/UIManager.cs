// UIManager.cs
// Coordinates UI screens, progress bars, order windows, floating popups.
// No longer handles screen‑specific logic.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Orders;
using YesChef.Stations;
using YesChef.Systems;

namespace YesChef.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Screens")]
        [SerializeField] private StartScreen startScreen;
        [SerializeField] private HUDScreen hudScreen;
        [SerializeField] private PauseScreen pauseScreen;
        [SerializeField] private GameOverScreen gameOverScreen;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

        }

        private void OnEnable()
        {
            SubscribeToGlobalEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromGlobalEvents();
        }

        private void Start()
        {
            ShowScreen<StartScreen>();
        }

        // ─────────────────────────────────────────────────────────
        // Screen Management (Type‑safe)
        // ─────────────────────────────────────────────────────────
        public void ShowScreen<T>() where T : BaseScreen
        {
            // Hide all screens first
            startScreen?.Hide();
            hudScreen?.Hide();
            pauseScreen?.Hide();
            gameOverScreen?.Hide();

            // Show the requested one
            if (typeof(T) == typeof(StartScreen)) startScreen?.Show();
            else if (typeof(T) == typeof(HUDScreen)) hudScreen?.Show();
            else if (typeof(T) == typeof(PauseScreen)) pauseScreen?.Show();
            else if (typeof(T) == typeof(GameOverScreen)) gameOverScreen?.Show();
        }

        // ─────────────────────────────────────────────────────────
        // Global Event Subscription
        // ─────────────────────────────────────────────────────────
        private void SubscribeToGlobalEvents()
        {
            ScoreSystem.OnGameEndedWithHighScore += OnGameEnded;
        }
        private void UnsubscribeFromGlobalEvents()
        {
            ScoreSystem.OnGameEndedWithHighScore -= OnGameEnded;
        }

        public void OnGameEnded(bool isNewHighScore,int finalScore)
        {
            if ( gameOverScreen != null)
            {
                gameOverScreen.ShowFinalScore(finalScore, isNewHighScore);
            }
            ShowScreen<GameOverScreen>();
        }
    }
}