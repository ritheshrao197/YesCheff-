// ScoreSystem.cs
// Manages current score, high score, and persistence via PlayerPrefs.

using UnityEngine;
using YesChef.Core;

namespace YesChef.Systems
{
   
    public class ScoreSystem : MonoBehaviour
    {

        public int CurrentScore { get; private set; }
        public int HighScore { get; private set; }
        [Header("Encryption Settings")]
        public EncryptionConfig encryptionConfig;

        private void Awake()
        {
            SaveSystem.Initialise(encryptionConfig);

            HighScore = SaveSystem.Load<SaveContainer>().Highscore;
            GameEvents.RaiseHighScoreChanged(HighScore);
            GameLogger.Info(GameLogCategory.Score, $"Loaded high score: {HighScore}.", this);
        }
        public void ResetScore()
        {
            CurrentScore = 0;
            GameEvents.RaiseScoreChanged(CurrentScore);
            GameEvents.RaiseHighScoreChanged(HighScore);
            GameLogger.Info(GameLogCategory.Score, "Score reset to 0.", this);
        }

        public void AddScore(int delta)
        {
            CurrentScore += delta;
            GameEvents.RaiseScoreChanged(CurrentScore);
            GameLogger.Info(GameLogCategory.Score, $"Score changed by {delta}. New total: {CurrentScore}.", this);
        }

        public void FinaliseGame()
        {
            bool isNewHighScore = CurrentScore > HighScore;
            if (isNewHighScore)
            {
                HighScore = CurrentScore;
                var saveContainer = new SaveContainer { Highscore = HighScore };
                SaveSystem.Save(saveContainer);
                GameEvents.RaiseHighScoreChanged(HighScore);
                GameLogger.Info(GameLogCategory.Score, $"New high score saved: {HighScore}.", this);
            }

            GameLogger.Info(GameLogCategory.Score, $"Game finalised with score {CurrentScore}. New high score: {isNewHighScore}.", this);
            GameEvents.RaiseGameEnded(isNewHighScore, CurrentScore);
        }
    }
}
