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
