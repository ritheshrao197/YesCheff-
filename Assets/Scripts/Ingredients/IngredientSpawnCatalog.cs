using System;
using System.Collections.Generic;
using UnityEngine;

namespace YesChef.Ingredients
{
    [CreateAssetMenu(menuName = "YesChef/Ingredient Spawn Catalog", fileName = "IngredientSpawnCatalog")]
    public class IngredientSpawnCatalog : ScriptableObject
    {
        [SerializeField] private List<IngredientSpawnEntry> entries = new List<IngredientSpawnEntry>();

        public bool TryGetPrefab(IngredientData ingredientData, out GameObject prefab)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                IngredientSpawnEntry entry = entries[i];
                if (entry == null || entry.Data != ingredientData)
                {
                    continue;
                }

                prefab = entry.Prefab;
                return prefab != null;
            }

            prefab = null;
            return false;
        }
    }

    [Serializable]
    public class IngredientSpawnEntry
    {
        public IngredientData Data;
        public GameObject Prefab;
    }
}
