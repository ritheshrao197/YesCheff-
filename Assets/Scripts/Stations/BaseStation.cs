// BaseStation.cs
// Abstract base for all interactable kitchen stations.
// Handles trigger-zone detection so derived classes just implement Interact().

using UnityEngine;
using YesChef.Core;
using YesChef.Interfaces;
using YesChef.Player;

namespace YesChef.Stations
{
   /// <summary>
   /// Abstract base class for all interactable kitchen stations. Implements IInteractable interface and handles trigger-zone detection for player proximity. Derived classes must implement the Interact() method to define specific station behavior. Also provides helper methods for logging station-related events with consistent formatting and categorization.
   /// Stations should have a collider set as trigger to detect player proximity. When the player is within the trigger zone, they can interact with the station by pressing the interaction key (e.g., 'E'), which will call the Interact() method defined in the derived class. The GetInteractionPrompt() method provides a default prompt that can be overridden for specific stations.
   /// </summary>
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
       /// <summary>
       /// Defines the interaction behavior when the player interacts with this station. The specific behavior will depend on the type of station (e.g., chopping, cooking, serving) and should be implemented in the derived class. The method receives a reference to the PlayerController, allowing it to check the player's held ingredient, modify it, or trigger other game events as needed. Interaction should only occur if the player is within the station's trigger zone and presses the interaction key.
       /// </summary>
       /// <param name="player"></param>
        public abstract void Interact(PlayerController player);

/// <summary>
/// Provides the interaction prompt to display to the player when they are near this station. By default, it returns a generic prompt based on the station name (e.g., "[E] Use Station"), but derived classes can override this method to provide more specific instructions (e.g., "[E] Chop Ingredient" for a chopping station). This prompt is used by the UI system to inform the player of possible interactions when they are within the station's trigger zone.
/// </summary>
/// <returns></returns>
        public virtual string GetInteractionPrompt()
        {
            return $"[E] {stationName}";
        }

/// <summary>
/// Helper method for logging informational messages related to this station. Prepends the station name to the message for clarity and categorizes it under GameLogCategory.Stations. The context parameter allows the log to be associated with this station object, enabling features like clicking the log message to highlight the station in the editor. Similar helper methods are provided for warnings, errors and verbose logs to maintain consistent formatting across all station-related logs.
/// </summary>
/// <param name="message"></param>
        protected void LogInfo(string message)
        {
            GameLogger.Info(GameLogCategory.Stations, $"{stationName}: {message}", this);
        }
/// <summary>
/// Helper method for logging warning messages related to this station. Prepends the station name to the message for clarity and categorizes it under GameLogCategory.Stations. The context parameter allows the log to be associated with this station object, enabling features like clicking the log message to highlight the station in the editor. Similar helper methods are provided for info, errors and verbose logs to maintain consistent formatting across all station-related logs.
/// </summary>
/// <param name="message"></param>
        protected void LogWarning(string message)
        {
            GameLogger.Warning(GameLogCategory.Stations, $"{stationName}: {message}", this);
        }
/// <summary>
/// Helper method for logging error messages related to this station. Prepends the station name to the message for clarity and categorizes it under GameLogCategory.Stations. The context parameter allows the log to be associated with this station object, enabling features like clicking the log message to highlight the station in the editor. Similar helper methods are provided for info, warnings and verbose logs to maintain consistent formatting across all station-related logs.
/// </summary>
/// <param name="message"></param>
        protected void LogError(string message)
        {
            GameLogger.Error(GameLogCategory.Stations, $"{stationName}: {message}", this);
        }
/// <summary>
/// Helper method for logging verbose messages related to this station. Prepends the station name to the message for clarity and categorizes it under GameLogCategory.Stations. The context parameter allows the log to be associated with this station object, enabling features like clicking the log message to highlight the station in the editor. Verbose logs are typically used for detailed debugging information that may not be necessary in regular playtesting, and can be enabled or disabled via the GameLogger settings. Similar helper methods are provided for info, warnings and errors to maintain consistent formatting across all station-related logs.
/// </summary>
/// <param name="message"></param>
        protected void LogVerbose(string message)
        {
            GameLogger.Verbose(GameLogCategory.Stations, $"{stationName}: {message}", this);
        }
    }
}
