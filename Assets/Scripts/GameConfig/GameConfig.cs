using System;
using UnityEngine;

namespace YesChef.Core
{
    [CreateAssetMenu(menuName = "YesChef/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public SystemConfig systems = new SystemConfig();
    }

    [Serializable]
    public class SystemConfig
    {
        public TimerSystemConfig timer = new TimerSystemConfig();
        public OrderSystemConfig orders = new OrderSystemConfig();
    }

    [Serializable]
    public class TimerSystemConfig
    {
        [Min(30f)] public float gameDuration = 180f;
    }

    [Serializable]
    public class OrderSystemConfig
    {
        [Min(1)] public int maxWindows = 4;
        [Min(0f)] public float respawnDelay = 5f;
        [Min(0f)] public float initialSpawnDelayMin = 0.5f;
        [Min(0f)] public float initialSpawnDelayMax = 2f;
        [Min(1)] public int minIngredientsPerOrder = 2;
        [Min(1)] public int maxIngredientsPerOrder = 3;
        [Range(0f, 1f)] public float minIngredientCountChance = 0.5f;
    }
}