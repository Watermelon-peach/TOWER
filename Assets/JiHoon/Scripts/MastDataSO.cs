using UnityEngine;
using System.Collections.Generic;

namespace Tower.Game
{
    [CreateAssetMenu(fileName = "MastData", menuName = "Dungeon/Mast Data")]
    public class MastDataSO : ScriptableObject
    {
        [Header("Mast Info")]
        public int mastID;
        public string mastName = "Mast";

        [Header("Floor Configuration")]
        public MapSpawnConfig[] floorConfigs = new MapSpawnConfig[5];

        [Header("Mast Prefab")]
        public GameObject mastPrefab;

        [Header("Start Settings")]

        public Vector3 startPosition = new Vector3(-107.3f, 0, 0);
        public Vector3 startRotation = Vector3.zero;

        [Header("Mast Settings")]
        public bool hasSafetyZone = true;
        public int floorCount = 4;
    }
}