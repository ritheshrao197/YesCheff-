// CustomerWindow.cs
// Accepts prepared ingredients, validates them against the active order,
// and reports score via event when an order completes.

using System;
using UnityEngine;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Orders;
using YesChef.Player;

namespace YesChef.Stations
{
    public class CustomerWindow : BaseStation
    {
        // ── Events ────────────────────────────────────────────────────────
        /// <summary>Fired when an order is fully completed. Args: windowIndex, score.</summary>
        public static event Action<int, int> OnDeliveryScored;

        // ── Inspector ─────────────────────────────────────────────────────
        [SerializeField] private int windowIndex;

        [SerializeField] private OrderManager _orderManager;

        public int WindowIndex => windowIndex;

        protected override void Awake()
        {
            base.Awake();
            stationName = "Customer Window";
        }

        private void Start()
        {
            if (_orderManager == null)
            {
                LogError("OrderManager reference not set in inspector.");
            }
        }

        public override void Interact(PlayerController player)
        {
            if (player.HeldIngredient == null)
            {
                LogVerbose($"Window {windowIndex} interaction ignored because player has empty hands.");
                return;
            }

            Ingredient ing = player.HeldIngredient;

            // 1. Ingredient must be ready
            if (!ing.IsReady)
            {
                LogWarning($"Window {windowIndex} rejected {GameLogger.DescribeIngredient(ing)} because it is not ready.");
                return;
            }

            // 2. There must be an active order
            Order order = _orderManager.GetOrderAtWindow(windowIndex);
            if (order == null)
            {
                LogWarning($"Window {windowIndex} rejected {GameLogger.DescribeIngredient(ing)} because there is no active order.");
                return;
            }

            // 3. Try to slot the ingredient into the order
            bool accepted = order.TryFulfil(ing.Data);
            if (!accepted)
            {
                LogWarning($"Window {windowIndex} rejected {ing.Data.displayName}; not required by {GameLogger.DescribeOrder(order)}.");
                return;
            }

            // Consume the ingredient
            player.Drop();
            Destroy(ing.gameObject);
            LogInfo($"Window {windowIndex} accepted {ing.Data.displayName} for {GameLogger.DescribeOrder(order)}.");

            // 4. Check for order completion
            if (order.IsComplete)
            {
                int score = order.CalculateScore();
                _orderManager.CompleteOrderAtWindow(windowIndex);
                OnDeliveryScored?.Invoke(windowIndex, score);
                LogInfo($"Window {windowIndex} completed {GameLogger.DescribeOrder(order)} for {score} points.");
            }
        }

        public override string GetInteractionPrompt()
        {
            Order order = _orderManager?.GetOrderAtWindow(windowIndex);
            return order == null ? "No order here" : "[E] Deliver ingredient";
        }
    }
}
