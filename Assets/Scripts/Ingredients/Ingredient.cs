// Ingredient.cs
// Runtime component attached to every physical ingredient object in the scene.
// Tracks current state and updates visual colour accordingly.

using UnityEngine;

namespace YesChef.Ingredients
{
    [RequireComponent(typeof(Renderer))]
    public class Ingredient : MonoBehaviour
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        public IngredientData Data { get; private set; }
        public IngredientState State { get; private set; } = IngredientState.Raw;
        public bool IsReady => State == IngredientState.Prepared || (!Data.NeedsPrep && State == IngredientState.Raw);

        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;

        public void Initialise(IngredientData data)
        {
            Data = data;
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            SetState(IngredientState.Raw);
        }

        public void SetState(IngredientState newState)
        {
            State = newState;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_renderer == null || Data == null)
            {
                return;
            }

            Color target = State switch
            {
                IngredientState.Raw => Data.rawColor,
                IngredientState.Prepared => Data.preparedColor,
                IngredientState.Processing => Color.Lerp(Data.rawColor, Data.preparedColor, 0.5f),
                _ => Data.rawColor
            };

            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(ColorPropertyId, target);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
