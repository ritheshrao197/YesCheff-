using System;
using UnityEngine;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Player;

namespace YesChef.Stations
{
    public class Refrigerator : BaseStation
    {
        private const string PickupPrompt = "[E] Pick up ingredient";
        private static readonly Vector3 SpawnOffset = Vector3.up * 0.5f;

        [Header("Refrigerator")]
        [SerializeField] private IngredientRegistry ingredientRegistry;
        [SerializeField] private IngredientSpawnCatalog spawnCatalog;

        protected override void Awake()
        {
            base.Awake();
            stationName = "Refrigerator";
        }

        public override void Interact(PlayerController player)
        {
            if (player.HeldIngredient != null)
            {
                LogWarning($"Player attempted pickup while already holding {GameLogger.DescribeIngredient(player.HeldIngredient)}.");
                return;
            }

            if (ingredientRegistry == null)
            {
                LogError("Ingredient registry is not assigned.");
                return;
            }

            if (spawnCatalog == null)
            {
                LogError("Ingredient spawn catalog is not assigned.");
                return;
            }

            IngredientData ingredientData = ingredientRegistry.GetRandom();
            if (ingredientData == null)
            {
                LogError("Ingredient registry returned null data.");
                return;
            }

            if (!spawnCatalog.TryGetPrefab(ingredientData, out GameObject ingredientPrefab) || ingredientPrefab == null)
            {
                LogError($"No prefab configured for ingredient '{ingredientData.displayName}'.");
                return;
            }

            GameObject ingredientObject = Instantiate(ingredientPrefab, transform.position + SpawnOffset, Quaternion.identity);
            if (!ingredientObject.TryGetComponent(out Ingredient ingredient))
            {
                LogError("Spawned ingredient prefab is missing an Ingredient component.");
                Destroy(ingredientObject);
                return;
            }

            ingredient.Initialise(ingredientData);
            player.PickUp(ingredient);
            LogInfo($"Dispensed {ingredientData.displayName} to player.");
        }

        public override string GetInteractionPrompt()
        {
            return PickupPrompt;
        }

    }
}
