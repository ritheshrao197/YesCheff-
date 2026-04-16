// Trash.cs
// Destroys whatever ingredient the player is holding.

using UnityEngine;
using YesChef.Ingredients;
using YesChef.Player;

namespace YesChef.Stations
{
    public class Trash : BaseStation
    {
        protected override void Awake()
        {
            base.Awake();
            stationName = "Trash";
        }

        public override void Interact(PlayerController player)
        {
            if (player.HeldIngredient == null)
            {
                LogVerbose("Player tried to discard an item, but hands were empty.");
                return;
            }

            Ingredient ingredient = player.HeldIngredient;
            player.Drop();
            Destroy(ingredient.gameObject);
            LogInfo($"Discarded {ingredient.Data.displayName}.");
        }

        public override string GetInteractionPrompt()
        {
            return "[E] / [Space] Throw away ingredient";
        }
    }
}
