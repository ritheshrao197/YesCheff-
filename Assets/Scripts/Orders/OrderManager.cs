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
        private const int DefaultMaxWindows = 4;
        private const float DefaultRespawnDelay = 5f;
        private const float DefaultInitialSpawnDelayMin = 0.5f;
        private const float DefaultInitialSpawnDelayMax = 2f;
        private const int DefaultMinIngredientsPerOrder = 1;
        private const int DefaultMaxIngredientsPerOrder = 2;
        private const float DefaultMinIngredientCountChance = 0.5f;

        // ── Events ────────────────────────────────────────────────────────
        public static event Action<Order, int> OnOrderCompleted;   // order, score delta
        public static event Action<Order, int> OnOrderAssigned;    // order, window index
        public static event Action<int> OnWindowCleared;    // window index

        // ── Inspector ─────────────────────────────────────────────────────
        [Tooltip("All ingredient definitions used to generate orders.")]
        public IngredientRegistry ingredientRegistry;

        [SerializeField] private GameConfig gameConfig;

        // ── State ─────────────────────────────────────────────────────────
        private Order[] _windowOrders;   // index = window slot
        private int _nextOrderId = 0;
        private bool _isRunning = false;

        private int MaxWindows => gameConfig != null ? gameConfig.systems.orders.maxWindows : DefaultMaxWindows;
        private float RespawnDelay => gameConfig != null ? gameConfig.systems.orders.respawnDelay : DefaultRespawnDelay;
        private float InitialSpawnDelayMin => gameConfig != null ? gameConfig.systems.orders.initialSpawnDelayMin : DefaultInitialSpawnDelayMin;
        private float InitialSpawnDelayMax => gameConfig != null ? gameConfig.systems.orders.initialSpawnDelayMax : DefaultInitialSpawnDelayMax;
        private int MinIngredientsPerOrder => gameConfig != null ? gameConfig.systems.orders.minIngredientsPerOrder : DefaultMinIngredientsPerOrder;
        private int MaxIngredientsPerOrder => gameConfig != null ? gameConfig.systems.orders.maxIngredientsPerOrder : DefaultMaxIngredientsPerOrder;
        private float MinIngredientCountChance => gameConfig != null ? gameConfig.systems.orders.minIngredientCountChance : DefaultMinIngredientCountChance;

        // ── Unity lifecycle ───────────────────────────────────────────────
        private void Awake()
        {
            if (gameConfig == null)
            {
                gameConfig = Resources.Load<GameConfig>("GameConfig");
            }

            _windowOrders = new Order[MaxWindows];
        }

        // ── Public API ────────────────────────────────────────────────────
        public void StartOrders()
        {
            _isRunning = true;
            GameLogger.Info(GameLogCategory.Orders, $"Starting order flow for {MaxWindows} windows.", this);
            // Spawn all 4 orders immediately
            // for (int i = 0; i < maxWindows; i++)
            // {
            //     SpawnOrder(i);
            // }
            StartCoroutine(StartOrdersWithDelay());
        }
        IEnumerator StartOrdersWithDelay()
        {
            for (int i = 0; i < _windowOrders.Length; i++)
            {
                SpawnOrder(i);
                yield return new WaitForSeconds(UnityEngine.Random.Range(InitialSpawnDelayMin, InitialSpawnDelayMax));
            }
        }
        /// <summary>
        /// Stops the order flow, preventing new orders from spawning and clearing any pending respawns. Existing orders will remain active until completed or expired, but no new orders will be generated. This is typically called when the game ends to freeze the order state.
        /// </summary>
        public void StopOrders()
        {
            _isRunning = false;
            StopAllCoroutines();
            GameLogger.Info(GameLogCategory.Orders, "Stopped order flow and cleared pending respawns.", this);
        }
        /// <summary>
        /// Returns the current order assigned to the specified window index, or null if the slot is empty or the index is out of range. This is used by CustomerWindow to display the order details and check for fulfilment. The Order object contains all the information about required and fulfilled ingredients, as well as timing for score calculation.
        /// </summary>
        /// <param name="windowIndex"></param>
        /// <returns></returns>
        public Order GetOrderAtWindow(int windowIndex)
        {
            if (windowIndex < 0 || windowIndex >= _windowOrders.Length) return null;
            return _windowOrders[windowIndex];
        }

        /// <summary>
        /// Called by CustomerWindow after it confirms the order is complete.
        /// Clears the slot, fires events, and schedules the next order.
        /// </summary>
        public void CompleteOrderAtWindow(int windowIndex)
        {
            if (windowIndex < 0 || windowIndex >= _windowOrders.Length) return;
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
    /// <summary>
    /// Spawns a new order at the specified window index. Generates a random list of required ingredients based on the configured min/max and chance for extra ingredients. Creates a new Order object with a unique ID and assigns it to the window slot. Fires the OnOrderAssigned event to notify UI and other systems of the new order. Logs the details of the spawned order for debugging.
    /// </summary>
    /// <param name="windowIndex"></param>
        private void SpawnOrder(int windowIndex)
        {
            if (!_isRunning) return;

            int count = GetIngredientCountForNextOrder();
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
            GameLogger.Verbose(GameLogCategory.Orders, $"Respawning order at window {windowIndex} in {RespawnDelay:0.0}s.", this);
            yield return new WaitForSeconds(RespawnDelay);
            if (_isRunning)
                SpawnOrder(windowIndex);
        }

        private int GetIngredientCountForNextOrder()
        {
            int minCount = Mathf.Max(1, MinIngredientsPerOrder);
            int maxCount = Mathf.Max(minCount, MaxIngredientsPerOrder);

            if (minCount == maxCount)
            {
                return minCount;
            }

            return UnityEngine.Random.value < MinIngredientCountChance ? minCount : maxCount;
        }
    }
}
