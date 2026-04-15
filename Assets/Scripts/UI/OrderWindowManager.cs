using UnityEngine;
using YesChef.Orders;

namespace YesChef.UI
{
    public class OrderWindowManager : MonoBehaviour
    {
        [Header("Order Windows")]
        [SerializeField] private OrderWindowUI[] orderWindowUIs;

        private void OnEnable()
        {
            OrderManager.OnOrderAssigned += OnOrderAssigned;
            OrderManager.OnWindowCleared += OnWindowCleared;
        }

        private void OnDisable()
        {
            OrderManager.OnOrderAssigned -= OnOrderAssigned;
            OrderManager.OnWindowCleared -= OnWindowCleared;
        }

        private void OnOrderAssigned(Order order, int windowIndex)
        {
            if (IsValidWindowIndex(windowIndex))
                orderWindowUIs[windowIndex].SetOrder(order);
        }

        private void OnWindowCleared(int windowIndex)
        {
            if (IsValidWindowIndex(windowIndex))
                orderWindowUIs[windowIndex].ClearOrder();
        }

        private bool IsValidWindowIndex(int idx) => idx >= 0 && idx < orderWindowUIs.Length;
    }
}