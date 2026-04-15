// OrderManager.cs
// Creates, tracks, fulfils, and respawns orders for customer windows.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YesChef.Core;
using YesChef.Ingredients;

namespace YesChef.Orders
{
    public class OrderManager : MonoBehaviour
    {
        private const int DefaultMaxWindows = 4;
        private const float DefaultRespawnDelay = 5f;
        private const float DefaultInitialSpawnDelayMin = 0.5f;
        private const float DefaultInitialSpawnDelayMax = 2f;
        private const int DefaultMinIngredientsPerOrder = 1;
        private const int DefaultMaxIngredientsPerOrder = 2;
        private const float DefaultMinIngredientCountChance = 0.5f;

        [Tooltip("All ingredient definitions used to generate orders.")]
        [SerializeField] private IngredientRegistry ingredientRegistry;
        [SerializeField] private GameConfig gameConfig;

        private Order[] _windowOrders;
        private int _nextOrderId;
        private bool _isRunning;

        private int MaxWindows => gameConfig != null ? gameConfig.systems.orders.maxWindows : DefaultMaxWindows;
        private float RespawnDelay => gameConfig != null ? gameConfig.systems.orders.respawnDelay : DefaultRespawnDelay;
        private float InitialSpawnDelayMin => gameConfig != null ? gameConfig.systems.orders.initialSpawnDelayMin : DefaultInitialSpawnDelayMin;
        private float InitialSpawnDelayMax => gameConfig != null ? gameConfig.systems.orders.initialSpawnDelayMax : DefaultInitialSpawnDelayMax;
        private int MinIngredientsPerOrder => gameConfig != null ? gameConfig.systems.orders.minIngredientsPerOrder : DefaultMinIngredientsPerOrder;
        private int MaxIngredientsPerOrder => gameConfig != null ? gameConfig.systems.orders.maxIngredientsPerOrder : DefaultMaxIngredientsPerOrder;
        private float MinIngredientCountChance => gameConfig != null ? gameConfig.systems.orders.minIngredientCountChance : DefaultMinIngredientCountChance;

        private void Awake()
        {
            if (gameConfig == null)
            {
                gameConfig = Resources.Load<GameConfig>("GameConfig");
            }

            _windowOrders = new Order[MaxWindows];
        }

        public void StartOrders()
        {
            ResetOrders();
            _isRunning = true;
            GameLogger.Info(GameLogCategory.Orders, $"Starting order flow for {MaxWindows} windows.", this);
            StartCoroutine(SpawnInitialOrders());
        }

        public void StopOrders()
        {
            _isRunning = false;
            StopAllCoroutines();
            GameLogger.Info(GameLogCategory.Orders, "Stopped order flow and cleared pending respawns.", this);
        }

        public void ResetOrders()
        {
            StopOrders();
            if (_windowOrders != null)
                for (int i = 0; i < _windowOrders.Length; i++)
                {
                    if (_windowOrders[i] == null)
                    {
                        continue;
                    }

                    _windowOrders[i] = null;
                    GameEvents.RaiseOrderWindowCleared(i);
                }

            _nextOrderId = 0;
        }

        public Order GetOrderAtWindow(int windowIndex)
        {
            return IsValidWindowIndex(windowIndex) ? _windowOrders[windowIndex] : null;
        }

        public bool TryDeliverIngredient(int windowIndex, Ingredient ingredient, out int awardedScore)
        {
            awardedScore = 0;
            if (!IsValidWindowIndex(windowIndex) || ingredient == null)
            {
                return false;
            }

            Order order = _windowOrders[windowIndex];
            if (order == null || !ingredient.IsReady)
            {
                return false;
            }

            if (!order.TryFulfil(ingredient.Data))
            {
                return false;
            }

            GameEvents.RaiseOrderUpdated(order, windowIndex);

            if (!order.IsComplete)
            {
                return true;
            }

            awardedScore = order.CalculateScore();
            CompleteOrderAtWindow(windowIndex, awardedScore);
            return true;
        }

        public void CompleteOrderAtWindow(int windowIndex, int scoreOverride = -1)
        {
            if (!IsValidWindowIndex(windowIndex))
            {
                return;
            }

            Order completed = _windowOrders[windowIndex];
            if (completed == null)
            {
                return;
            }

            int score = scoreOverride >= 0 ? scoreOverride : completed.CalculateScore();
            _windowOrders[windowIndex] = null;

            GameEvents.RaiseOrderCompleted(completed, score);
            GameEvents.RaiseOrderWindowCleared(windowIndex);
            GameLogger.Info(GameLogCategory.Orders, $"Completed {GameLogger.DescribeOrder(completed)} at window {windowIndex} for {score} points.", this);

            if (_isRunning)
            {
                StartCoroutine(RespawnAfterDelay(windowIndex));
            }
        }

        private IEnumerator SpawnInitialOrders()
        {
            for (int i = 0; i < _windowOrders.Length; i++)
            {
                SpawnOrder(i);
                yield return new WaitForSeconds(Random.Range(InitialSpawnDelayMin, InitialSpawnDelayMax));
            }
        }

        private void SpawnOrder(int windowIndex)
        {
            if (!_isRunning || ingredientRegistry == null || !IsValidWindowIndex(windowIndex))
            {
                return;
            }

            List<IngredientData> ingredients = new List<IngredientData>();
            int ingredientCount = GetIngredientCountForNextOrder();
            for (int i = 0; i < ingredientCount; i++)
            {
                IngredientData ingredientData = ingredientRegistry.GetRandom();
                if (ingredientData != null)
                {
                    ingredients.Add(ingredientData);
                }
            }

            if (ingredients.Count == 0)
            {
                GameLogger.Warning(GameLogCategory.Orders, "Unable to spawn order because the ingredient registry returned no ingredients.", this);
                return;
            }

            Order order = new Order(_nextOrderId++, ingredients, Time.time);
            _windowOrders[windowIndex] = order;
            GameEvents.RaiseOrderAssigned(order, windowIndex);
            GameLogger.Info(GameLogCategory.Orders, $"Spawned {GameLogger.DescribeOrder(order)} at window {windowIndex}.", this);
        }

        private IEnumerator RespawnAfterDelay(int windowIndex)
        {
            GameLogger.Verbose(GameLogCategory.Orders, $"Respawning order at window {windowIndex} in {RespawnDelay:0.0}s.", this);
            yield return new WaitForSeconds(RespawnDelay);

            if (_isRunning)
            {
                SpawnOrder(windowIndex);
            }
        }

        private int GetIngredientCountForNextOrder()
        {
            int minCount = Mathf.Max(1, MinIngredientsPerOrder);
            int maxCount = Mathf.Max(minCount, MaxIngredientsPerOrder);

            if (minCount == maxCount)
            {
                return minCount;
            }

            return Random.value < MinIngredientCountChance ? minCount : maxCount;
        }

        private bool IsValidWindowIndex(int windowIndex)
        {
            return windowIndex >= 0 && windowIndex < _windowOrders.Length;
        }
    }
}
