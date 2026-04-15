// Order.cs
// Contains the Order data class and the OrderManager that drives order lifecycle.

using System.Collections.Generic;
using UnityEngine;
using YesChef.Ingredients;

namespace YesChef.Orders
{
    // ══════════════════════════════════════════════════════════════════════
    // Order — pure data class, no MonoBehaviour overhead
    // ══════════════════════════════════════════════════════════════════════
    public class Order
    {
        public int Id { get; }
        public List<IngredientData> RequiredIngredients { get; }
        public List<IngredientData> FulfilledIngredients { get; } = new List<IngredientData>();
        public float StartTime { get; }

        public bool IsComplete => FulfilledIngredients.Count >= RequiredIngredients.Count;

        public Order(int id, List<IngredientData> ingredients)
        {
            Id = id;
            RequiredIngredients = new List<IngredientData>(ingredients);
            StartTime = Time.time;
        }

        /// <summary>
        /// Try to fulfil one slot with the given ingredient.
        /// Returns true if the ingredient was accepted.
        /// </summary>
        public bool TryFulfil(IngredientData ingredient)
        {
            // Count how many of this type are still needed
            int needed = 0;
            int filled = 0;
            foreach (var req in RequiredIngredients)
                if (req.type == ingredient.type) needed++;
            foreach (var fil in FulfilledIngredients)
                if (fil.type == ingredient.type) filled++;

            if (needed - filled <= 0) return false;

            FulfilledIngredients.Add(ingredient);
            return true;
        }

        /// <summary>Calculate score: sum of ingredient values minus whole seconds elapsed.</summary>
        public int CalculateScore()
        {
            int baseScore = 0;
            foreach (var ing in RequiredIngredients)
                baseScore += ing.scoreValue;

            int elapsed = Mathf.FloorToInt(Time.time - StartTime);
            return baseScore - elapsed;
        }
    }
}
