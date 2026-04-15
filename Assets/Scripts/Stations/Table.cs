// Table.cs
// Processes ingredients that are prepared at the chopping table.

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
        private const string PickupPrompt = "[E] Pick up prepared ingredient";
        private const string ChoppingPrompt = "Preparing...";
        private const string PlacePrompt = "[E] Place ingredient to prep";

        [SerializeField] private float defaultChopTime = 2f;

        private Ingredient _currentIngredient;
        private bool _isChopping;

        protected override void Awake()
        {
            base.Awake();
            stationName = "Chopping Table";
        }

        public override void Interact(PlayerController player)
        {
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

            if (player.HeldIngredient == null || _isChopping || _currentIngredient != null)
            {
                return;
            }

            Ingredient ingredient = player.HeldIngredient;
            if (!ingredient.Data.CanBePreparedAt(PreparationStationType.ChoppingTable))
            {
                LogWarning($"Rejected {GameLogger.DescribeIngredient(ingredient)} because it cannot be prepared at the chopping table.");
                return;
            }

            if (ingredient.State != IngredientState.Raw)
            {
                LogWarning($"Rejected {GameLogger.DescribeIngredient(ingredient)} because it is not raw.");
                return;
            }

            player.Drop();
            _currentIngredient = ingredient;
            _currentIngredient.transform.position = transform.position + IngredientPlacementOffset;
            _currentIngredient.gameObject.SetActive(true);
            LogInfo($"Started preparing {GameLogger.DescribeIngredient(_currentIngredient)}.");

            StartCoroutine(ChopRoutine());
        }

        public override string GetInteractionPrompt()
        {
            if (_currentIngredient != null && !_isChopping && _currentIngredient.State == IngredientState.Prepared)
            {
                return PickupPrompt;
            }

            if (_isChopping)
            {
                return ChoppingPrompt;
            }

            return PlacePrompt;
        }

        private IEnumerator ChopRoutine()
        {
            _isChopping = true;
            _currentIngredient.SetState(IngredientState.Processing);
            GameEvents.RaiseChopStarted(this);

            float duration = Mathf.Max(0.01f, _currentIngredient.Data != null ? _currentIngredient.Data.preparationTime : defaultChopTime);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                GameEvents.RaiseChopProgressChanged(this, elapsed / duration);
                yield return null;
            }

            _currentIngredient.SetState(IngredientState.Prepared);
            _isChopping = false;
            GameEvents.RaiseChopProgressChanged(this, 1f);
            GameEvents.RaiseChopCompleted(this);
            LogInfo($"Finished preparing {GameLogger.DescribeIngredient(_currentIngredient)}.");
        }
        public override void ResetStation()
        {
            if (_currentIngredient != null)
            {
                Destroy(_currentIngredient.gameObject);
                _currentIngredient = null;
            }
            _isChopping = false;
        }
    }
}
