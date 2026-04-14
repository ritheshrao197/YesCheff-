// Trash.cs
// Destroys whatever ingredient the player is holding.

using UnityEngine;
using YesChef.Core;
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

            Ingredient ing = player.HeldIngredient;
            player.Drop();
            Destroy(ing.gameObject);
            LogInfo($"Discarded {GameLogger.DescribeIngredient(ing)}.");
        }

        public override string GetInteractionPrompt()
        {
            return "[E] Throw away ingredient";
        }
    }
}
