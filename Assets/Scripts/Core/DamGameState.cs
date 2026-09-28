using System;
using UnityEngine;

namespace Riverforge
{
    [Serializable]
    public class DamGameState
    {
        public int day = 1;
        public float hour = 8f;
        public float money = 120000f;
        public float waterLevel = 0.46f;
        public float safety = 100f;
        public float ecology = 100f;
        public float publicTrust = 85f;
        public float totalEnergyMWh = 0f;
        public float turbineLoad = 0f;
        public float spillway = 0.12f;
        public bool damBuilt = false;
        public float buildProgress = 0f;
        public int wallLevel = 0;
        public int turbineLevel = 0;
        public int spillwayLevel = 0;
        public bool won = false;
        public bool failed = false;
        public string weather = "Clear";
        public int seed = 1440;
    }
}
