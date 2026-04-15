// OrderWindowUI.cs
// Renders a single customer order panel.

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Ingredients;
using YesChef.Orders;

namespace YesChef.UI
{
    public class OrderWindowUI : MonoBehaviour
    {
        private const float TimerRefreshIntervalSeconds = 0.5f;
        private const float HighUrgencyThresholdSeconds = 30f;
        private const float MediumUrgencyThresholdSeconds = 15f;
        private const string OrderTimerFormat = "{0}s";

        [Header("References")]
        [SerializeField] private TMP_Text orderTimerText;
        [SerializeField] private TMP_Text noOrderText;
        [SerializeField] private GameObject ingredientSlotContainer;
        [SerializeField] private GameObject ingredientSlotPrefab;

        private readonly List<GameObject> _slots = new List<GameObject>();

        private Order _currentOrder;
        private Coroutine _timerRoutine;

        private void OnEnable()
        {
            TryStartTimerRoutine();
        }

        private void OnDisable()
        {
            StopTimerRoutine();
        }

        public void SetOrder(Order order)
        {
            _currentOrder = order;

            if (noOrderText != null)
            {
                noOrderText.gameObject.SetActive(false);
            }

            if (ingredientSlotContainer != null)
            {
                ingredientSlotContainer.SetActive(true);
            }

            RebuildSlots();
            TryStartTimerRoutine();
        }

        public void ClearOrder()
        {
            _currentOrder = null;
            StopTimerRoutine();
            ClearSlots();

            if (noOrderText != null)
            {
                noOrderText.gameObject.SetActive(true);
            }

            if (ingredientSlotContainer != null)
            {
                ingredientSlotContainer.SetActive(false);
            }

            if (orderTimerText != null)
            {
                orderTimerText.text = string.Empty;
            }
        }

        public void RefreshFulfilment()
        {
            if (_currentOrder != null)
            {
                RebuildSlots();
            }
        }

        private void RebuildSlots()
        {
            ClearSlots();
            if (_currentOrder == null || ingredientSlotContainer == null || ingredientSlotPrefab == null)
            {
                return;
            }

            Dictionary<IngredientData, int> fulfilledCounts = BuildFulfilledCountMap(_currentOrder.FulfilledIngredients);
            for (int i = 0; i < _currentOrder.RequiredIngredients.Count; i++)
            {
                IngredientData requiredIngredient = _currentOrder.RequiredIngredients[i];
                bool isFulfilled = ConsumeFulfilledFlag(requiredIngredient, fulfilledCounts);

                GameObject slotObject = Instantiate(ingredientSlotPrefab, ingredientSlotContainer.transform);
                TMP_Text label = slotObject.GetComponentInChildren<TMP_Text>();
                Image image = slotObject.GetComponent<Image>();

                if (label != null)
                {
                    label.text = isFulfilled ? $"OK {requiredIngredient.displayName}" : requiredIngredient.displayName;
                }

                if (image != null)
                {
                    image.color = isFulfilled ? Color.green * 0.7f : requiredIngredient.preparedColor;
                }

                _slots.Add(slotObject);
            }
        }

        private IEnumerator UpdateOrderTimer()
        {
            while (_currentOrder != null)
            {
                if (orderTimerText != null)
                {
                    float elapsed = Time.time - _currentOrder.StartTime;
                    orderTimerText.text = string.Format(OrderTimerFormat, Mathf.FloorToInt(elapsed));
                    orderTimerText.color = elapsed > HighUrgencyThresholdSeconds
                        ? Color.red
                        : elapsed > MediumUrgencyThresholdSeconds
                            ? Color.yellow
                            : Color.white;
                }

                yield return new WaitForSeconds(TimerRefreshIntervalSeconds);
            }

            _timerRoutine = null;
        }

        private void TryStartTimerRoutine()
        {
            if (_currentOrder == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                return;
            }

            StopTimerRoutine();
            _timerRoutine = StartCoroutine(UpdateOrderTimer());
        }

        private void StopTimerRoutine()
        {
            if (_timerRoutine == null)
            {
                return;
            }

            StopCoroutine(_timerRoutine);
            _timerRoutine = null;
        }

        private void ClearSlots()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                Destroy(_slots[i]);
            }

            _slots.Clear();
        }

        private static Dictionary<IngredientData, int> BuildFulfilledCountMap(IReadOnlyList<IngredientData> ingredients)
        {
            Dictionary<IngredientData, int> counts = new Dictionary<IngredientData, int>();
            for (int i = 0; i < ingredients.Count; i++)
            {
                IngredientData ingredient = ingredients[i];
                counts.TryGetValue(ingredient, out int currentCount);
                counts[ingredient] = currentCount + 1;
            }

            return counts;
        }

        private static bool ConsumeFulfilledFlag(IngredientData ingredient, Dictionary<IngredientData, int> fulfilledCounts)
        {
            if (!fulfilledCounts.TryGetValue(ingredient, out int remaining) || remaining <= 0)
            {
                return false;
            }

            fulfilledCounts[ingredient] = remaining - 1;
            return true;
        }
    }
}
