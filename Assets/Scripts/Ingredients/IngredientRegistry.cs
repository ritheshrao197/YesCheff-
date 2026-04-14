// IngredientRegistry.cs
// Single ScriptableObject that holds references to all IngredientData assets.
// Assign in the Inspector; used by Refrigerator and OrderManager.
// Create via: Assets > Create > YesChef > IngredientRegistry

using System.Collections.Generic;
using UnityEngine;
using YesChef.Core;

namespace YesChef.Ingredients
{
    [CreateAssetMenu(menuName = "YesChef/IngredientRegistry", fileName = "IngredientRegistry")]
    public class IngredientRegistry : ScriptableObject
    {
        [Tooltip("All ingredient definitions in the game. Must contain Vegetable, Cheese and Meat.")]
        public List<IngredientData> allIngredients = new List<IngredientData>();

        /// <summary>Returns a random IngredientData from the registry.</summary>
        public IngredientData GetRandom()
        {
            if (allIngredients == null || allIngredients.Count == 0)
            {
                GameLogger.Error(GameLogCategory.Ingredients, "IngredientRegistry is empty!");
                return null;
            }
            return allIngredients[Random.Range(0, allIngredients.Count)];
        }

        /// <summary>Returns the IngredientData for a given type.</summary>
        public IngredientData Get(IngredientType type)
        {
            return allIngredients.Find(d => d.type == type);
        }
    }
}
