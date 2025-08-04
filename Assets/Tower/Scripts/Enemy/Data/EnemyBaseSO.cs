using UnityEngine;

namespace Tower.Enemy.Data
{
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy/EnemyData")]
    public class EnemyBaseSO : ScriptableObject
    {
        [Header("Basic Info")]
        public string enemyName;
        public float atk;
        public float def;

        public float maxHp;
        public float maxGp;
    }

}
