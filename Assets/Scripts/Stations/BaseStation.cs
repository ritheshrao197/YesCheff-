// BaseStation.cs
// Abstract base for all interactable kitchen stations.
// Handles trigger-zone detection so derived classes just implement Interact().

using UnityEngine;
using YesChef.Core;
using YesChef.Interfaces;
using YesChef.Player;

namespace YesChef.Stations
{
    [RequireComponent(typeof(Collider))]
    public abstract class BaseStation : MonoBehaviour, IInteractable
    {
        [Header("Station Settings")]
        [SerializeField] protected string stationName = "Station";

        protected virtual void Awake()
        {
            // Ensure the collider is set as trigger for proximity detection
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            LogVerbose("Initialised station trigger.");
        }

        // ── IInteractable ─────────────────────────────────────────────────
        public abstract void Interact(PlayerController player);

        public virtual string GetInteractionPrompt()
        {
            return $"[E] {stationName}";
        }

        protected void LogInfo(string message)
        {
            GameLogger.Info(GameLogCategory.Stations, $"{stationName}: {message}", this);
        }

        protected void LogWarning(string message)
        {
            GameLogger.Warning(GameLogCategory.Stations, $"{stationName}: {message}", this);
        }

        protected void LogError(string message)
        {
            GameLogger.Error(GameLogCategory.Stations, $"{stationName}: {message}", this);
        }

        protected void LogVerbose(string message)
        {
            GameLogger.Verbose(GameLogCategory.Stations, $"{stationName}: {message}", this);
        }
    }
}
