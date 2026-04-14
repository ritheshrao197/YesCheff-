// ScoreSystem.cs
// Manages current score, high score, and persistence via PlayerPrefs.

using System;
using UnityEngine;
using YesChef.Core;

namespace YesChef.Systems
{
    public class ScoreSystem : MonoBehaviour
    {
        // ── Events ────────────────────────────────────────────────────────
        public static event Action<int> OnScoreChanged;
        public static event Action<int> OnHighScoreChanged;
        public static event Action<bool> OnGameEndedWithHighScore; // bool = isNewHighScore

        private const string HighScoreKey = "YesChef_HighScore";

        // ── State ─────────────────────────────────────────────────────────
        public int CurrentScore { get; private set; }
        public int HighScore    { get; private set; }

        // ── Unity lifecycle ───────────────────────────────────────────────
        private void Awake()
        {
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            GameLogger.Info(GameLogCategory.Score, $"Loaded high score: {HighScore}.", this);
        }

        // ── Public API ────────────────────────────────────────────────────
        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
            GameLogger.Info(GameLogCategory.Score, "Score reset to 0.", this);
        }

        public void AddScore(int delta)
        {
            CurrentScore += delta;
            OnScoreChanged?.Invoke(CurrentScore);
            GameLogger.Info(GameLogCategory.Score, $"Score changed by {delta}. New total: {CurrentScore}.", this);
        }

        public void FinaliseGame()
        {
            bool isNew = CurrentScore > HighScore;
            if (isNew)
            {
                HighScore = CurrentScore;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
                OnHighScoreChanged?.Invoke(HighScore);
                GameLogger.Info(GameLogCategory.Score, $"New high score saved: {HighScore}.", this);
            }

            GameLogger.Info(GameLogCategory.Score, $"Game finalised with score {CurrentScore}. New high score: {isNew}.", this);
            OnGameEndedWithHighScore?.Invoke(isNew);
        }
    }
}
