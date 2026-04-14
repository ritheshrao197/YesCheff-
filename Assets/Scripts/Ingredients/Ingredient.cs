// Ingredient.cs
// Runtime component attached to every physical ingredient object in the scene.
// Tracks current state and updates visual colour accordingly.

using UnityEngine;

namespace YesChef.Ingredients
{
    [RequireComponent(typeof(Renderer))]
    public class Ingredient : MonoBehaviour
    {
        // ── Public state ──────────────────────────────────────────────────
        public IngredientData Data { get; private set; }
        public IngredientState State { get; private set; } = IngredientState.Raw;

        // ── Private refs ──────────────────────────────────────────────────
        private Renderer _renderer;

        // ── Initialisation ────────────────────────────────────────────────
        public void Initialise(IngredientData data)
        {
            Data = data;
            _renderer = GetComponent<Renderer>();
            SetState(IngredientState.Raw);
        }

        // ── State management ──────────────────────────────────────────────
        public void SetState(IngredientState newState)
        {
            State = newState;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_renderer == null) return;

            Color target = State switch
            {
                IngredientState.Raw        => Data.rawColor,
                IngredientState.Prepared   => Data.preparedColor,
                IngredientState.Processing => Color.Lerp(Data.rawColor, Data.preparedColor, 0.5f),
                _                          => Data.rawColor
            };

            _renderer.material.color = target;
        }

        public bool IsReady => State == IngredientState.Prepared ||
                               (!Data.NeedsPrep && State == IngredientState.Raw);
    }
}
