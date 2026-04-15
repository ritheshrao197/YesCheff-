// GameLogger.cs
// Central orchestrator. Drives the high-level game state machine:
// MainMenu → Playing → Paused → GameOver → (restart)

using UnityEngine;

namespace YesChef.Core
{
    public static class GameLogger
    {
        public static bool EnableInfoLogs = true;
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
