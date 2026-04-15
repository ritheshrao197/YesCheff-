// TimerSystem.cs
// Counts down from gameDuration seconds, firing events while running and when time expires.

using System;
using System.Collections;
using UnityEngine;
using YesChef.Core;

namespace YesChef.Systems
{
    public class TimerSystem : MonoBehaviour
    {
        private const float DefaultGameDuration = 180f;
        private const float TickIntervalSeconds = 0.5f;

        public static event Action<float> OnTimerTick;
        public static event Action OnTimerExpired;

        [SerializeField] private GameConfig gameConfig;

        public float Remaining { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }

        private Coroutine _timerRoutine;
        private float _timerEndTime;
        private float _pausedAt;

        private float GameDuration => gameConfig != null ? gameConfig.systems.timer.gameDuration : DefaultGameDuration;

        private void Awake()
        {
            if (gameConfig == null)
            {
                gameConfig = Resources.Load<GameConfig>("GameConfig");
            }
        }

        public void StartTimer()
        {
            StopTimerRoutine();

            Remaining = GameDuration;
            _pausedAt = 0f;
            IsRunning = true;
            IsPaused = false;
            _timerEndTime = Time.unscaledTime + GameDuration;
            _timerRoutine = StartCoroutine(RunTimer());
            OnTimerTick?.Invoke(Remaining);
            GameLogger.Info(GameLogCategory.Timer, $"Timer started at {GameDuration:0.0}s.", this);
        }

        public void StopTimer()
        {
            IsRunning = false;
            IsPaused = false;
            _pausedAt = 0f;
            StopTimerRoutine();
            GameLogger.Info(GameLogCategory.Timer, $"Timer stopped with {Remaining:0.0}s remaining.", this);
        }

        public void SetPaused(bool paused)
        {
            if (!IsRunning || IsPaused == paused)
            {
                return;
            }

            IsPaused = paused;

            if (paused)
            {
                _pausedAt = Time.unscaledTime;
            }
            else if (_pausedAt > 0f)
            {
                _timerEndTime += Time.unscaledTime - _pausedAt;
                _pausedAt = 0f;
                OnTimerTick?.Invoke(Remaining);
            }

            GameLogger.Info(GameLogCategory.Timer, paused ? "Timer paused." : "Timer resumed.", this);
            GameLogger.Info(GameLogCategory.Timer, $"Timer {(paused ? "paused" : "resumed")} with {Remaining:0.0}s remaining.", this);
        }

        private IEnumerator RunTimer()
        {
            while (IsRunning)
            {
                if (!IsPaused)
                {
                    Remaining = Mathf.Max(0f, _timerEndTime - Time.unscaledTime);
                    OnTimerTick?.Invoke(Remaining);

                    if (Remaining <= 0f)
                    {
                        IsRunning = false;
                        IsPaused = false;
                        _pausedAt = 0f;
                        GameLogger.Info(GameLogCategory.Timer, "Timer reached zero.", this);
                        OnTimerExpired?.Invoke();
                        _timerRoutine = null;
                        yield break;
                    }
                }

                yield return new WaitForSecondsRealtime(TickIntervalSeconds);
            }

            _timerRoutine = null;
        }

        private void StopTimerRoutine()
        {
            if (_timerRoutine == null)
            {
                return;
            }

            StopCoroutine(_timerRoutine);
            _timerRoutine = null;
        }
    }
}
