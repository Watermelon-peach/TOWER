using UnityEngine;

namespace Tower.Game.Data.Bless 
{
    [CreateAssetMenu(fileName = "AbilityBlessData", menuName = "Scriptable Objects/AbilityBlessData")]

    public class AbilityBlessData : ScriptableObject
    {
        public string blessName;

        public GameObject blessEffect;

        public int Price;

        public int nowTier;
        public int maxTier;

        public string TargetStatus;
        public float StatusIncreaserate;
    }
}