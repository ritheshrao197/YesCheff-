// IngredientData.cs
// ScriptableObject that defines the properties of each ingredient type.
// Create assets via: Assets > Create > YesChef > IngredientData

using UnityEngine;

namespace YesChef.Ingredients
{
    public enum PreparationStationType
    {
        None,
        ChoppingTable,
        Stove
    }

    public enum IngredientType
    {
        Vegetable,
        Cheese,
        Meat
    }

    public enum IngredientState
    {
        Raw,
        Prepared,
        Processing
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
        public float preparationTime = 2f;
        public PreparationStationType preparationStation = PreparationStationType.None;

        [Header("Visuals")]
        public Color rawColor = Color.white;
        public Color preparedColor = Color.green;

        public bool NeedsPrep => requiresPreparation;

        public bool CanBePreparedAt(PreparationStationType stationType)
        {
            return requiresPreparation && preparationStation == stationType;
        }
    }
}
