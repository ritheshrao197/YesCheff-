// IngredientData.cs
// ScriptableObject that defines the properties of each ingredient type.
// Create assets via: Assets > Create > YesChef > IngredientData

using UnityEngine;

namespace YesChef.Ingredients
{
    public enum IngredientType
    {
        Vegetable,
        Cheese,
        Meat
    }

    public enum IngredientState
    {
        Raw,        // Just picked up from fridge
        Prepared,   // Chopped (veg) or Cooked (meat) — ready for orders
        Processing  // Currently being chopped/cooked
    }

    [CreateAssetMenu(menuName = "YesChef/IngredientData", fileName = "IngredientData")]
    public class IngredientData : ScriptableObject
    {
        [Header("Identity")]
        public IngredientType type;
        public string displayName;

        [Header("Scoring")]
        public int scoreValue = 10;

        [Header("Preparation")]
        public bool requiresPreparation = true;
        public float preparationTime = 2f;   // seconds

        [Header("Visuals")]
        public Color rawColor = Color.white;
        public Color preparedColor = Color.green;

        /// <summary>Returns true if this ingredient needs processing before it can fill an order.</summary>
        public bool NeedsPrep => requiresPreparation;
    }
}
