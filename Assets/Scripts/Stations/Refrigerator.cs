// Refrigerator.cs
// Gives the player a raw ingredient when they interact.
// Never runs out of ingredients.

using UnityEngine;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Player;

namespace YesChef.Stations
{
    public class Refrigerator : BaseStation
    {
        [Header("Refrigerator")]
        [SerializeField] private IngredientRegistry ingredientRegistry;
        [SerializeField] private GameObject cheesePrefab;
        [SerializeField] private GameObject vegetablePrefab;
        [SerializeField] private GameObject meatPrefab;

        // Array indexed by IngredientType (must match enum order)
        private GameObject[] prefabByType;

        // Cached spawn offset
        private static readonly Vector3 SpawnOffset = Vector3.up * 0.5f;

        protected override void Awake()
        {
            base.Awake();
            stationName = "Refrigerator";

            // Build lookup table once
            prefabByType = new GameObject[3];
            prefabByType[(int)IngredientType.Cheese] = cheesePrefab;
            prefabByType[(int)IngredientType.Vegetable] = vegetablePrefab;
            prefabByType[(int)IngredientType.Meat] = meatPrefab;
        }

        public override void Interact(PlayerController player)
        {
            // Fail fast: player already holding something
            if (player.HeldIngredient != null)
            {
                LogWarning($"Player attempted pickup while already holding {GameLogger.DescribeIngredient(player.HeldIngredient)}.");
                return;
            }

            // Get random ingredient data
            IngredientData data = ingredientRegistry.GetRandom();
            if (data == null)
            {
                LogError("Ingredient registry returned null data.");
                return;
            }

            // Get the corresponding prefab using the lookup array
            int typeIndex = (int)data.type;
            if (typeIndex < 0 || typeIndex >= prefabByType.Length)
            {
                LogError($"Invalid ingredient type index: {typeIndex}");
                return;
            }

            GameObject selectedPrefab = prefabByType[typeIndex];
            if (selectedPrefab == null)
            {
                LogError($"No prefab assigned for ingredient type {data.type}");
                return;
            }

            // Spawn and initialise the ingredient
            GameObject go = Instantiate(selectedPrefab, transform.position + SpawnOffset, Quaternion.identity);
            if (!go.TryGetComponent(out Ingredient ingredient))
            {
                LogError("Spawned ingredient prefab is missing an Ingredient component.");
                Destroy(go);
                return;
            }

            ingredient.Initialise(data);
            player.PickUp(ingredient);
            LogInfo($"Dispensed {data.displayName} to player.");
        }

        public override string GetInteractionPrompt()
        {
            return "[E] Pick up ingredient";
        }
    }
}