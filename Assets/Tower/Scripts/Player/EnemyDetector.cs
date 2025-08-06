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
    }

}
