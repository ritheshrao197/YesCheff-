// BaseStation.cs
// Shared base for all interactable kitchen stations.

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
            // Collider stationCollider = GetComponent<Collider>();
            // // stationCollider.isTrigger = true;
            LogVerbose("Initialised station trigger.");
        }

        public abstract void Interact(PlayerController player);

        public virtual string GetInteractionPrompt()
        {
            return $"[E] / [Space] {stationName}";
        }
        public virtual void ResetStation()
        {
            // Override in derived classes if needed to reset station state between rounds.
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
