// CustomerWindow.cs
// Accepts prepared ingredients and delegates order validation to the order system.

using UnityEngine;
using YesChef.Core;
using YesChef.Orders;
using YesChef.Player;

namespace YesChef.Stations
{
    public class CustomerWindow : BaseStation
    {
        [SerializeField] private int windowIndex;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private GameObject customerAvatar;

        public int WindowIndex => windowIndex;

        protected override void Awake()
        {
            base.Awake();
            stationName = "Customer Window";
        }

        private void OnEnable()
        {
            GameEvents.OrderAssigned += HandleOrderAssigned;
            GameEvents.OrderWindowCleared += HandleOrderCleared;
        }

        private void OnDisable()
        {
            GameEvents.OrderAssigned -= HandleOrderAssigned;
            GameEvents.OrderWindowCleared -= HandleOrderCleared;
        }

        public override void Interact(PlayerController player)
        {
            if (player.HeldIngredient == null)
            {
                LogVerbose($"Window {windowIndex} interaction ignored because player has empty hands.");
                return;
            }

            if (orderManager == null)
            {
                LogError("OrderManager reference is missing.");
                return;
            }

            if (!orderManager.TryDeliverIngredient(windowIndex, player.HeldIngredient, out int score))
            {
                LogWarning($"Window {windowIndex} rejected {GameLogger.DescribeIngredient(player.HeldIngredient)}.");
                return;
            }

            string deliveredIngredientName = player.HeldIngredient.Data.displayName;
            Object deliveredObject = player.HeldIngredient.gameObject;
            player.Drop();
            Destroy(deliveredObject);
            LogInfo($"Window {windowIndex} accepted {deliveredIngredientName}.");

            if (score > 0)
            {
                GameEvents.RaiseDeliveryScored(windowIndex, score);
                LogInfo($"Window {windowIndex} completed the active order for {score} points.");
            }
        }

        public override string GetInteractionPrompt()
        {
            return orderManager != null && orderManager.GetOrderAtWindow(windowIndex) != null
                ? "[E] Deliver ingredient"
                : "No order here";
        }

        private void HandleOrderAssigned(Order order, int assignedWindowIndex)
        {
            if (assignedWindowIndex == windowIndex && customerAvatar != null)
            {
                customerAvatar.SetActive(true);
            }
        }

        private void HandleOrderCleared(int clearedWindowIndex)
        {
            if (clearedWindowIndex == windowIndex && customerAvatar != null)
            {
                customerAvatar.SetActive(false);
            }
        }
    }
}
