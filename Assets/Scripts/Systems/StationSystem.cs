using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YesChef.Core;
using YesChef.Stations;
namespace YesChef.Systems
{

    public class StationSystem : MonoBehaviour
    {
        [SerializeField] private Stove stove;
        [SerializeField] private Refrigerator refrigerator;
        [SerializeField] private Table cuttingBoard;

        private void Start()
        {
            if (stove == null || refrigerator == null || cuttingBoard == null)
            {
                GameLogger.Error(GameLogCategory.Stations, "StationSystem is missing references to one or more stations.", this);
            }
        }

        public void ResetStations()
        {
            stove.ResetStation();
            refrigerator.ResetStation();
            cuttingBoard.ResetStation();
        }
    }
}