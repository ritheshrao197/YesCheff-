// Stove.cs
// Processes ingredients that are prepared at the stove.

using System;
using System.Collections;
using UnityEngine;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Player;

namespace YesChef.Stations
{
    public class Stove : BaseStation
    {
        private const string PickupPrompt = "[E] Pick up prepared ingredient";
        private const string PlacePrompt = "[E] Place ingredient to cook";
        private const string FullPrompt = "Stove full";

        [SerializeField] private float defaultCookTime = 6f;
        [SerializeField] private int slotCount = 2;
        [SerializeField] private Transform[] slotOffsets;

        private Ingredient[] _slots;
        private bool[] _isCooking;

        protected override void Awake()
        {
            base.Awake();
            stationName = "Stove";
            _slots = new Ingredient[slotCount];
            _isCooking = new bool[slotCount];
        }

        public override void Interact(PlayerController player)
        {
            LogVerbose("Player interacted with stove.");
            for (int i = 0; i < slotCount; i++)
            {

                if (_slots[i] == null || _isCooking[i] || _slots[i].State != IngredientState.Prepared)
                {
                LogVerbose($"Slot {i} is not ready for pickup. Cooking: {_isCooking[i]}, Ingredient: {GameLogger.DescribeIngredient(_slots[i])}");
                    continue;
                }

                if (player.HeldIngredient != null)
                {
                    LogVerbose($"Prepared item ready in slot {i}, but player hands are full.");
                    continue;
                }

                player.PickUp(_slots[i]);
                LogVerbose($"Player collected {GameLogger.DescribeIngredient(_slots[i])} from slot {i}.");
                _slots[i] = null;
                return;
            }

            if (player.HeldIngredient == null)
            {
                LogInfo("Player has no ingredient to place.");

                return;
            }

            Ingredient ingredient = player.HeldIngredient;
            if (!ingredient.Data.CanBePreparedAt(PreparationStationType.Stove))
            {
                LogWarning($"Rejected {GameLogger.DescribeIngredient(ingredient)} because it cannot be cooked at the stove.");
                return;
            }

            if (ingredient.State != IngredientState.Raw)
            {
                LogWarning($"Rejected {GameLogger.DescribeIngredient(ingredient)} because it is not raw.");
                return;
            }

            for (int i = 0; i < slotCount; i++)
            {
                if (_slots[i] != null)
                {
                    continue;
                }

                player.Drop();
                _slots[i] = ingredient;
                _slots[i].transform.position = slotOffsets[i].position;
                _slots[i].gameObject.SetActive(true);
                LogInfo($"Started cooking {GameLogger.DescribeIngredient(ingredient)} in slot {i}.");
                StartCoroutine(CookRoutine(i));
                return;
            }

            LogWarning("Player tried to place an ingredient, but all stove slots are occupied.");
        }

        public override string GetInteractionPrompt()
        {
            for (int i = 0; i < slotCount; i++)
            {
                if (_slots[i] != null && !_isCooking[i] && _slots[i].State == IngredientState.Prepared)
                {
                    return PickupPrompt;
                }
            }

            for (int i = 0; i < slotCount; i++)
            {
                if (_slots[i] == null)
                {
                    return PlacePrompt;
                }
            }

            return FullPrompt;
        }

        private IEnumerator CookRoutine(int slotIndex)
        {
            _isCooking[slotIndex] = true;
            _slots[slotIndex].SetState(IngredientState.Processing);
            GameEvents.RaiseStoveSlotStarted(this, slotIndex);
            LogVerbose($"Cooking started in slot {slotIndex}.");

            float duration = Mathf.Max(0.01f, _slots[slotIndex].Data != null ? _slots[slotIndex].Data.preparationTime : defaultCookTime);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                GameEvents.RaiseStoveSlotProgressChanged(this, slotIndex, elapsed / duration);
                yield return null;
            }

            _slots[slotIndex].SetState(IngredientState.Prepared);
            _isCooking[slotIndex] = false;
            GameEvents.RaiseStoveSlotProgressChanged(this, slotIndex, 1f);
            GameEvents.RaiseStoveSlotCompleted(this, slotIndex);
            LogInfo($"Cooking finished in slot {slotIndex}: {GameLogger.DescribeIngredient(_slots[slotIndex])}.");
        }

        public void ResetStation()
        {
            for (int i = 0; i < slotCount; i++)
            {
                if (_slots[i] != null)
                {
                    Destroy(_slots[i].gameObject);
                    _slots[i] = null;
                    _isCooking[i] = false;
                }
            }
             LogInfo("Stove station reset.");   
        }

    }
}
