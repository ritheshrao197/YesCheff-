// Table.cs
// Chops vegetables. Only one vegetable can be on the table at a time.
// Takes 2 seconds; shows progress via event to UI.

using System;
using System.Collections;
using UnityEngine;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Player;

namespace YesChef.Stations
{
    public class Table : BaseStation
    {
        private static readonly Vector3 IngredientPlacementOffset = Vector3.up * 0.5f;
        private const string PickupPrompt = "[E] Pick up chopped vegetable";
        private const string ChoppingPrompt = "Chopping...";
        private const string PlacePrompt = "[E] Place vegetable to chop";

        // ── Events ────────────────────────────────────────────────────────
        public static event Action<float> OnChopProgress;   // 0..1
        public static event Action         OnChopComplete;
        public static event Action         OnChopStarted;

        // ── Inspector ─────────────────────────────────────────────────────
        [SerializeField] private float chopTime = 2f;

        // ── State ─────────────────────────────────────────────────────────
        private Ingredient _currentIngredient;
        private bool _isChopping = false;

        protected override void Awake()
        {
            base.Awake();
            stationName = "Chopping Table";
        }

        public override void Interact(PlayerController player)
        {
            // Case 1: Table has a finished vegetable — player picks it up
            if (_currentIngredient != null && !_isChopping && _currentIngredient.State == IngredientState.Prepared)
            {
                if (player.HeldIngredient == null)
                {
                    player.PickUp(_currentIngredient);
                    LogInfo($"Player collected {GameLogger.DescribeIngredient(_currentIngredient)} from table.");
                    _currentIngredient = null;
                }
                else
                {
                    LogVerbose("Prepared ingredient is ready on table, but player hands are full.");
                }
                return;
            }

            // Case 2: Player has a raw vegetable and table is free
            if (player.HeldIngredient != null && !_isChopping && _currentIngredient == null)
            {
                Ingredient ing = player.HeldIngredient;

                if (ing.Data.type != IngredientType.Vegetable)
                {
                    LogWarning($"Rejected {GameLogger.DescribeIngredient(ing)} because only vegetables can be chopped.");
                    return;
                }

                if (ing.State != IngredientState.Raw)
                {
                    LogWarning($"Rejected {GameLogger.DescribeIngredient(ing)} because it is not raw.");
                    return;
                }

                player.Drop();
                _currentIngredient = ing;
                _currentIngredient.transform.position = transform.position + IngredientPlacementOffset;
                _currentIngredient.gameObject.SetActive(true);
                LogInfo($"Started chopping {GameLogger.DescribeIngredient(_currentIngredient)}.");

                StartCoroutine(ChopRoutine());
            }
        }

        private IEnumerator ChopRoutine()
        {
            _isChopping = true;
            _currentIngredient.SetState(IngredientState.Processing);
            OnChopStarted?.Invoke();

            float elapsed = 0f;
            while (elapsed < chopTime)
            {
                elapsed += Time.deltaTime;
                OnChopProgress?.Invoke(elapsed / chopTime);
                yield return null;
            }

            _currentIngredient.SetState(IngredientState.Prepared);
            _isChopping = false;
            OnChopProgress?.Invoke(1f);
            OnChopComplete?.Invoke();
            LogInfo($"Finished chopping {GameLogger.DescribeIngredient(_currentIngredient)}.");
        }

        public override string GetInteractionPrompt()
        {
            if (_currentIngredient != null && !_isChopping && _currentIngredient.State == IngredientState.Prepared)
                return PickupPrompt;
            if (_isChopping)
                return ChoppingPrompt;
            return PlacePrompt;
        }
    }
}
