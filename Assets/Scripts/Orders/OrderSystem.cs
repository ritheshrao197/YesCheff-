// OrderSystem.cs
// Contains the Order data class and the OrderManager that drives order lifecycle.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YesChef.Core;
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

    // ══════════════════════════════════════════════════════════════════════
    // OrderManager — creates, tracks and expires orders
    // ══════════════════════════════════════════════════════════════════════
    public class OrderManager : MonoBehaviour
    {
        // ── Events ────────────────────────────────────────────────────────
        public static event Action<Order, int> OnOrderCompleted;   // order, score delta
        public static event Action<Order, int> OnOrderAssigned;    // order, window index
        public static event Action<int>        OnWindowCleared;    // window index

        // ── Inspector ─────────────────────────────────────────────────────
        [Tooltip("All ingredient definitions used to generate orders.")]
        public IngredientRegistry ingredientRegistry;

        [SerializeField] private int maxWindows = 4;
        [SerializeField] private float respawnDelay = 5f;

        // ── State ─────────────────────────────────────────────────────────
        private Order[] _windowOrders;   // index = window slot
        private int _nextOrderId = 0;
        private bool _isRunning = false;

        // ── Unity lifecycle ───────────────────────────────────────────────
        private void Awake()
        {
            _windowOrders = new Order[maxWindows];
        }

        // ── Public API ────────────────────────────────────────────────────
        public void StartOrders()
        {
            _isRunning = true;
            GameLogger.Info(GameLogCategory.Orders, $"Starting order flow for {maxWindows} windows.", this);
            // Spawn all 4 orders immediately
            for (int i = 0; i < maxWindows; i++)
                SpawnOrder(i);
        }

        public void StopOrders()
        {
            _isRunning = false;
            StopAllCoroutines();
            GameLogger.Info(GameLogCategory.Orders, "Stopped order flow and cleared pending respawns.", this);
        }

        public Order GetOrderAtWindow(int windowIndex)
        {
            if (windowIndex < 0 || windowIndex >= maxWindows) return null;
            return _windowOrders[windowIndex];
        }

        /// <summary>
        /// Called by CustomerWindow after it confirms the order is complete.
        /// Clears the slot, fires events, and schedules the next order.
        /// </summary>
        public void CompleteOrderAtWindow(int windowIndex)
        {
            if (windowIndex < 0 || windowIndex >= maxWindows) return;
            Order completed = _windowOrders[windowIndex];
            if (completed == null) return;

            int score = completed.CalculateScore();
            _windowOrders[windowIndex] = null;
            OnOrderCompleted?.Invoke(completed, score);
            OnWindowCleared?.Invoke(windowIndex);
            GameLogger.Info(GameLogCategory.Orders, $"Completed {GameLogger.DescribeOrder(completed)} at window {windowIndex} for {score} points.", this);

            if (_isRunning)
                StartCoroutine(RespawnAfterDelay(windowIndex));
        }

        // ── Private helpers ───────────────────────────────────────────────
        private void SpawnOrder(int windowIndex)
        {
            if (!_isRunning) return;

            int count = UnityEngine.Random.value < 0.5f ? 2 : 3;
            var ingredients = new List<IngredientData>();
            for (int i = 0; i < count; i++)
                ingredients.Add(ingredientRegistry.GetRandom());

            var order = new Order(_nextOrderId++, ingredients);
            _windowOrders[windowIndex] = order;
            OnOrderAssigned?.Invoke(order, windowIndex);
            GameLogger.Info(GameLogCategory.Orders, $"Spawned {GameLogger.DescribeOrder(order)} at window {windowIndex}.", this);
        }

        private IEnumerator RespawnAfterDelay(int windowIndex)
        {
            GameLogger.Verbose(GameLogCategory.Orders, $"Respawning order at window {windowIndex} in {respawnDelay:0.0}s.", this);
            yield return new WaitForSeconds(respawnDelay);
            if (_isRunning)
                SpawnOrder(windowIndex);
        }
    }
}
