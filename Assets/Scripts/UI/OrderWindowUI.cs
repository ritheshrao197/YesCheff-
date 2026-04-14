// OrderWindowUI.cs
// Attached to each customer window UI panel.
// Shows required ingredients with check marks and an order timer.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YesChef.Orders;
using YesChef.Ingredients;

namespace YesChef.UI
{
    public class OrderWindowUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text   orderTimerText;
        [SerializeField] private TMP_Text   noOrderText;
        [SerializeField] private GameObject ingredientSlotContainer;
        [SerializeField] private GameObject ingredientSlotPrefab;

        private Order _currentOrder;
        private List<GameObject> _slots = new List<GameObject>();
        private Coroutine _timerRoutine;

        // ── Public API ────────────────────────────────────────────────────
        public void SetOrder(Order order)
        {
            _currentOrder = order;

            if (noOrderText)  noOrderText.gameObject.SetActive(false);
            if (ingredientSlotContainer) ingredientSlotContainer.SetActive(true);

            RebuildSlots();

            if (_timerRoutine != null) StopCoroutine(_timerRoutine);
            _timerRoutine = StartCoroutine(UpdateOrderTimer());
        }

        public void ClearOrder()
        {
            _currentOrder = null;
            if (_timerRoutine != null) { StopCoroutine(_timerRoutine); _timerRoutine = null; }

            foreach (var s in _slots) Destroy(s);
            _slots.Clear();

            if (noOrderText)  noOrderText.gameObject.SetActive(true);
            if (ingredientSlotContainer) ingredientSlotContainer.SetActive(false);
            if (orderTimerText) orderTimerText.text = "";
        }

        public void RefreshFulfilment()
        {
            if (_currentOrder == null) return;
            RebuildSlots();
        }

        // ── Private helpers ───────────────────────────────────────────────
        private void RebuildSlots()
        {
            foreach (var s in _slots) Destroy(s);
            _slots.Clear();

            if (ingredientSlotContainer == null || ingredientSlotPrefab == null) return;

            for (int i = 0; i < _currentOrder.RequiredIngredients.Count; i++)
            {
                var go     = Instantiate(ingredientSlotPrefab, ingredientSlotContainer.transform);
                var label  = go.GetComponentInChildren<TMP_Text>();
                var image  = go.GetComponent<Image>();

                IngredientData req  = _currentOrder.RequiredIngredients[i];
                bool fulfilled = i < _currentOrder.FulfilledIngredients.Count;

                if (label) label.text = fulfilled ? $"✓ {req.displayName}" : req.displayName;
                if (image) image.color = fulfilled ? Color.green * 0.7f : req.rawColor;

                _slots.Add(go);
            }
        }

        private IEnumerator UpdateOrderTimer()
        {
            while (_currentOrder != null)
            {
                if (orderTimerText)
                {
                    float elapsed = UnityEngine.Time.time - _currentOrder.StartTime;
                    orderTimerText.text = $"{Mathf.FloorToInt(elapsed)}s";

                    // Colour-code urgency
                    orderTimerText.color = elapsed > 30f ? Color.red :
                                           elapsed > 15f ? Color.yellow : Color.white;
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
