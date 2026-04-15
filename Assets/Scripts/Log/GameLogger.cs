using System;
using UnityEngine;
using YesChef.Systems;

namespace YesChef.Core
{
    /// <summary>
    /// Centralised logging utility for the YesChef! project. Provides methods for logging info, warnings, errors and verbose messages, with category prefixes and optional context objects. Log output can be filtered by category and log level via static flags. 
    /// Usage example: GameLogger.Info(GameLogCategory.Orders, $"New order received: {GameLogger.DescribeOrder(order)}", this);
    /// </summary>
    public static class GameLogger
    {
        public static bool EnableInfoLogs = false;
        public static bool EnableWarningLogs = true;
        public static bool EnableVerboseLogs = false;

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Info(GameLogCategory category, string message, UnityEngine.Object context = null)
        {
            if (!EnableInfoLogs) return;
            Debug.Log(Format(category, message), context);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(GameLogCategory category, string message, UnityEngine.Object context = null)
        {
            if (!EnableWarningLogs) return;
            Debug.LogWarning(Format(category, message), context);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Verbose(GameLogCategory category, string message, UnityEngine.Object context = null)
        {
            if (!EnableVerboseLogs) return;
            Debug.Log(Format(category, message), context);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Error(GameLogCategory category, string message, UnityEngine.Object context = null)
        {
            Debug.LogError(Format(category, message), context);
        }

        public static string DescribeIngredient(Ingredients.Ingredient ingredient)
        {
            if (ingredient == null)
            {
                return "none";
            }

            string name = ingredient.Data != null ? ingredient.Data.displayName : ingredient.name;
            return $"{name} ({ingredient.State})";
        }

        public static string DescribeOrder(Orders.Order order)
        {
            if (order == null)
            {
                return "none";
            }

            return $"Order #{order.Id} [{order.FulfilledIngredients.Count}/{order.RequiredIngredients.Count}]";
        }

        private static string Format(GameLogCategory category, string message)
        {
            return $"[YesChef][{category}] {message}";
        }

    }
}
