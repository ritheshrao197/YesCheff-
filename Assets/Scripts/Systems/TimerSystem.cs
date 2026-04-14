// TimerSystem.cs
// Counts down from gameDuration seconds, firing events each second and when time expires.

using System;
using UnityEngine;
using YesChef.Core;

namespace YesChef.Systems
{
    public class TimerSystem : MonoBehaviour
    {
        // ── Events ────────────────────────────────────────────────────────
        public static event Action<float> OnTimerTick;   // remaining seconds
        public static event Action         OnTimerExpired;

        // ── Inspector ─────────────────────────────────────────────────────
        [SerializeField] private float gameDuration = 180f;

        // ── State ─────────────────────────────────────────────────────────
        public float Remaining { get; private set; }
        public bool  IsRunning  { get; private set; }
        public bool  IsPaused   { get; private set; }

        // ── Public API ────────────────────────────────────────────────────
        public void StartTimer()
        {
            Remaining = gameDuration;
            IsRunning  = true;
            IsPaused   = false;
            GameLogger.Info(GameLogCategory.Timer, $"Timer started at {gameDuration:0.0}s.", this);
        }

        public void StopTimer()
        {
            IsRunning = false;
            GameLogger.Info(GameLogCategory.Timer, $"Timer stopped with {Remaining:0.0}s remaining.", this);
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            GameLogger.Info(GameLogCategory.Timer, paused ? "Timer paused." : "Timer resumed.", this);
        }

        // ── Unity lifecycle ───────────────────────────────────────────────
        private void Update()
        {
            if (!IsRunning || IsPaused) return;

            Remaining -= Time.deltaTime;
            OnTimerTick?.Invoke(Remaining);

            if (Remaining <= 0f)
            {
                Remaining = 0f;
                IsRunning  = false;
                GameLogger.Info(GameLogCategory.Timer, "Timer reached zero.", this);
                OnTimerExpired?.Invoke();
            }
        }
    }
}
