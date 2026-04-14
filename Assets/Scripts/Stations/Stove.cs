// Stove.cs
// Has 2 independent cooking slots. Meat takes 6 seconds per slot.
// Player does NOT need to stay near the stove while cooking.

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
        // ── Events ────────────────────────────────────────────────────────
        // Per-slot progress (slotIndex, 0..1)
        public static event Action<int, float> OnSlotProgress;
        public static event Action<int>        OnSlotComplete;
        public static event Action<int>        OnSlotStarted;


        // ── Inspector ─────────────────────────────────────────────────────
        [SerializeField] private float cookTime = 6f;
        [SerializeField] private int   slotCount = 2;
        [SerializeField]private Transform[] SlotOffsets;


        // ── State ─────────────────────────────────────────────────────────
        private Ingredient[] _slots;
        private bool[]       _isCooking;


        protected override void Awake()
        {
            base.Awake();
            stationName = "Stove";
            _slots     = new Ingredient[slotCount];
            _isCooking = new bool[slotCount];
        }

        public override void Interact(PlayerController player)
        {
            // Priority: let player collect finished meat first
            for (int i = 0; i < slotCount; i++)
            {
                if (_slots[i] != null && !_isCooking[i] && _slots[i].State == IngredientState.Prepared)
                {
                    if (player.HeldIngredient == null)
                    {
                        player.PickUp(_slots[i]);
                        LogInfo($"Player collected {GameLogger.DescribeIngredient(_slots[i])} from slot {i}.");
                        _slots[i] = null;
                        return;
                    }

                    LogVerbose($"Cooked item ready in slot {i}, but player hands are full.");
                }
            }

            // Place raw meat into the first available free slot
            if (player.HeldIngredient != null)
            {
                Ingredient ing = player.HeldIngredient;

                if (ing.Data.type != IngredientType.Meat)
                {
                    LogWarning($"Rejected {GameLogger.DescribeIngredient(ing)} because only meat can be cooked.");
                    return;
                }

                if (ing.State != IngredientState.Raw)
                {
                    LogWarning($"Rejected {GameLogger.DescribeIngredient(ing)} because it is not raw.");
                    return;
                }

                for (int i = 0; i < slotCount; i++)
                {
                    if (_slots[i] == null)
                    {
                        player.Drop();
                        _slots[i] = ing;
                        _slots[i].transform.position = SlotOffsets[i].position;
                        _slots[i].gameObject.SetActive(true);
                        LogInfo($"Started cooking {GameLogger.DescribeIngredient(ing)} in slot {i}.");
                        StartCoroutine(CookRoutine(i));
                        return;
                    }
                }

                LogWarning("Player tried to place meat, but both stove slots are occupied.");
            }
        }

        private IEnumerator CookRoutine(int slotIndex)
        {
            _isCooking[slotIndex] = true;
            _slots[slotIndex].SetState(IngredientState.Processing);
            OnSlotStarted?.Invoke(slotIndex);
            LogVerbose($"Cooking started in slot {slotIndex}.");

            float elapsed = 0f;
            while (elapsed < cookTime)
            {
                elapsed += Time.deltaTime;
                OnSlotProgress?.Invoke(slotIndex, elapsed / cookTime);
                yield return null;
            }

            _slots[slotIndex].SetState(IngredientState.Prepared);
            _isCooking[slotIndex] = false;
            OnSlotProgress?.Invoke(slotIndex, 1f);
            OnSlotComplete?.Invoke(slotIndex);
            LogInfo($"Cooking finished in slot {slotIndex}: {GameLogger.DescribeIngredient(_slots[slotIndex])}.");
        }

        public override string GetInteractionPrompt()
        {
            for (int i = 0; i < slotCount; i++)
            {
                if (_slots[i] != null && !_isCooking[i] && _slots[i].State == IngredientState.Prepared)
                    return "[E] Pick up cooked meat";
            }
            for (int i = 0; i < slotCount; i++)
            {
                if (_slots[i] == null) return "[E] Place meat to cook";
            }
            return "Stove full";
        }
    }
}
