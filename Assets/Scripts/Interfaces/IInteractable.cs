// IInteractable.cs
// Any object the player can interact with must implement this interface.
// Enables the InteractionSystem to work generically with all stations.

namespace YesChef.Interfaces
{
    public interface IInteractable
    {
        /// <summary>Called when the player presses the interact key while facing this object.</summary>
        void Interact(Player.PlayerController player);

        /// <summary>Human-readable label shown in the interaction prompt UI.</summary>
        string GetInteractionPrompt();
    }
}
