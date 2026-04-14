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

        protected override void Awake()
        {
            base.Awake();
            stationName = "Refrigerator";
        }

        public override void Interact(PlayerController player)
        {
            LogInfo("Player interacted with refrigerator.");  // Log before validation checks
            // Player must have empty hands
            if (player.HeldIngredient != null)
            {
                LogWarning($"Player attempted pickup while already holding {GameLogger.DescribeIngredient(player.HeldIngredient)}.");
                return;
            }

          
            // Pick a random ingredient type
            IngredientData data = ingredientRegistry.GetRandom();
            GameObject ingredientPrefab = null;
        switch (data.type)
            {
                case IngredientType.Cheese:
                    ingredientPrefab = cheesePrefab;
                    break;
                case IngredientType.Vegetable:
                    ingredientPrefab = vegetablePrefab;
                    break;
                case IngredientType.Meat:
                    ingredientPrefab = meatPrefab;
                    break;
                default:
                    LogError($"Unsupported ingredient type: {data.type}");
                    return;
            }
              // Spawn a new ingredient object and give it to the player
            GameObject go = Instantiate(ingredientPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            var ingredient = go.GetComponent<Ingredient>();

            if (ingredient == null)
            {
                LogError("Spawned ingredient prefab is missing an Ingredient component.");
                Destroy(go);
                return;
            }

            if (data == null)
            {
                LogError("Ingredient registry returned no ingredient data.");
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
