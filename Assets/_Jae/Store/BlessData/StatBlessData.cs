using UnityEngine;

namespace Tower.Game.Data.Bless
{
    [CreateAssetMenu(fileName = "StatBlessData", menuName = "Scriptable Objects/StatBlessData")]
    public class StatblessData : ScriptableObject 
        {
        public string blessName;

        public GameObject blessEffect;

        public int Price(int Tier)
        {
            return Tier / 2 + 1;
        }
        public int nowTier;
        public int maxblessTier;

        public string TargetStatus;
        public float StatusIncreaserate;
    }
}