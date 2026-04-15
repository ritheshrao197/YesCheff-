// Order.cs
// Domain model for customer orders.

using System.Collections.Generic;
using UnityEngine;
using YesChef.Ingredients;

namespace YesChef.Orders
{
    public class Order
    {
        public int Id { get; }
        public IReadOnlyList<IngredientData> RequiredIngredients => _requiredIngredients;
        public IReadOnlyList<IngredientData> FulfilledIngredients => _fulfilledIngredients;
        public float StartTime { get; }
        public bool IsComplete => _fulfilledIngredients.Count >= _requiredIngredients.Count;

        private readonly List<IngredientData> _requiredIngredients;
        private readonly List<IngredientData> _fulfilledIngredients = new List<IngredientData>();

        public Order(int id, IEnumerable<IngredientData> ingredients, float startTime)
        {
            Id = id;
            _requiredIngredients = new List<IngredientData>(ingredients);
            StartTime = startTime;
        }

        public bool TryFulfil(IngredientData ingredient)
        {
            if (ingredient == null)
            {
                return false;
            }

            int requiredCount = CountMatches(_requiredIngredients, ingredient);
            int fulfilledCount = CountMatches(_fulfilledIngredients, ingredient);
            if (fulfilledCount >= requiredCount)
            {
                return false;
            }

            _fulfilledIngredients.Add(ingredient);
            return true;
        }

        public int CalculateScore()
        {
            int baseScore = 0;
            for (int i = 0; i < _requiredIngredients.Count; i++)
            {
                baseScore += _requiredIngredients[i].scoreValue;
            }

            int elapsed = Mathf.FloorToInt(Time.time - StartTime);
            return Mathf.Max(0, baseScore - elapsed);
        }

        private static int CountMatches(IReadOnlyList<IngredientData> ingredients, IngredientData target)
        {
            int count = 0;
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (ingredients[i] == target)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
