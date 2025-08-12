using UnityEngine;
using System.Collections.Generic;

using EnemyClass = Tower.Enemy.Enemy;

namespace Tower.Player
{
    public class EnemyDetector : MonoBehaviour
    {
        #region Variables
        public List<EnemyClass> detectedEnemies = new List<EnemyClass>();
        #endregion

        #region Unity Event Method
        private void Update()
        {
            // missing(=null) 적 제거
            detectedEnemies.RemoveAll(e => !e);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out EnemyClass enemy))
            {
                if (!detectedEnemies.Contains(enemy))
                    detectedEnemies.Add(enemy);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out EnemyClass enemy))
            {
                detectedEnemies.Remove(enemy);
            }
        }
        #endregion

        #region Custom Method
/*        public EnemyClass GetNearestEnemy()
        {
            if (detectedEnemies.Count <= 0)
            {
                Debug.Log("범위 내 적 없음");
                return null;
            }

            EnemyClass nearestEnemy = null;

            float minDistance = float.MaxValue;
            foreach (EnemyClass enemy in detectedEnemies)
            {
                if (enemy == null) continue; //파괴된 적 무시

                float distance = (enemy.transform.position - transform.position).magnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            return nearestEnemy;
        }*/
        #endregion
    }

}
