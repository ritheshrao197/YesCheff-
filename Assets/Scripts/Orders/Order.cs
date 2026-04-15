// Order.cs
// Contains the Order data class and the OrderManager that drives order lifecycle.

using System.Collections.Generic;
using UnityEngine;
using YesChef.Ingredients;

namespace YesChef.Orders
{
 
 /// <summary>
 /// Represents a customer's order, which consists of a list of required ingredients and tracks which have been fulfilled. Provides methods to try fulfilling the order with a given ingredient and to calculate the score based on ingredient values and time elapsed.
 /// The OrderManager (not shown) is responsible for creating new orders, assigning them to customers, and removing them when completed or expired.
 /// Orders are created with a unique ID and a list of required ingredients. The TryFulfil method checks if a given ingredient can fulfil one of the remaining required slots, and if so, adds it to the fulfilled list. The CalculateScore method sums the score values of the required ingredients and subtracts points based on how long the order has been active.
 /// This class is a pure data model and does not contain any Unity-specific code or references to GameObjects. It is used by the OrderManager and UI components to manage and display orders.
 /// Note: The OrderManager class (not shown) is responsible for creating new orders, assigning them to customers, and removing them when completed or expired.
 /// Example usage:
 /// var order = new Order(1, new List<IngredientData> { tomatoData, cheeseData });
 /// bool fulfilled = order.TryFulfil(tomatoData); // returns true, adds tomato to fulfilled ingredients
 /// int score = order.CalculateScore(); // calculates score based on required ingredients and time elapsed
 /// </summary>
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
       /// Attempts to fulfil the order with the given ingredient. Checks if the ingredient type matches any of the remaining required ingredients and if there are still unfulfilled slots for that type. If so, adds the ingredient to the fulfilled list and returns true. Otherwise, returns false.
       /// </summary>
       /// <param name="ingredient"></param>
       /// <returns></returns>
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

/// <summary>
/// Calculates the score for this order based on the score values of the required ingredients and how long the order has been active. The base score is the sum of the score values of the required ingredients. The final score is the base score minus points deducted for time elapsed since the order was created (e.g., 1 point per second). This encourages players to fulfil orders quickly for maximum points.
/// </summary>
/// <returns></returns>
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
