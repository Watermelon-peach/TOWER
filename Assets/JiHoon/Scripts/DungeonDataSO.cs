using UnityEngine;
using System.Collections.Generic;

namespace Tower.Game
{
    [CreateAssetMenu(fileName = "DungeonData", menuName = "Dungeon/Dungeon Data")]
    public class DungeonDataSO : ScriptableObject
    {
        [Header("Dungeon Info")]
        public string dungeonName = "Tower Dungeon";
        public string dungeonDescription;

        [Header("Mast List")]
        public List<MastDataSO> mastList = new List<MastDataSO>();  

        [Header("Dungeon Settings")]
        public int totalFloors;
        public float difficultyMultiplier = 1.0f;

        // 자동으로 총 층수 계산하는 메서드 추가
        public void CalculateTotalFloors()
        {
            totalFloors = 0;
            foreach (var mast in mastList)
            {
                if (mast != null)
                    totalFloors += mast.floorCount;
            }
        }
    }
}