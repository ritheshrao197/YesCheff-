using UnityEngine;
using YesChef.Core;
using YesChef.Orders;

namespace YesChef.UI
{
    public class OrderWindowManager : MonoBehaviour
    {
        [Header("Order Windows")]
        [SerializeField] private OrderWindowUI[] orderWindowUIs;

        private void OnEnable()
        {
            GameEvents.OrderAssigned += HandleOrderAssigned;
            GameEvents.OrderWindowCleared += HandleOrderCleared;
            GameEvents.OrderUpdated += HandleOrderUpdated;
        }

        private void OnDisable()
        {
            GameEvents.OrderAssigned -= HandleOrderAssigned;
            GameEvents.OrderWindowCleared -= HandleOrderCleared;
            GameEvents.OrderUpdated -= HandleOrderUpdated;
        }

        private void HandleOrderAssigned(Order order, int windowIndex)
        {
            if (IsValidWindowIndex(windowIndex))
            {
                orderWindowUIs[windowIndex].SetOrder(order);
            }
        }

        private void HandleOrderCleared(int windowIndex)
        {
            if (IsValidWindowIndex(windowIndex))
            {
                orderWindowUIs[windowIndex].ClearOrder();
            }
        }

        private void HandleOrderUpdated(Order order, int windowIndex)
        {
            if (IsValidWindowIndex(windowIndex))
            {
                orderWindowUIs[windowIndex].RefreshFulfilment();
            }
        }

        private bool IsValidWindowIndex(int index)
        {
            return index >= 0 && index < orderWindowUIs.Length;
        }
    }
}
