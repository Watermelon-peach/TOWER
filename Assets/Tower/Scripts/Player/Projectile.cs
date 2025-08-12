using UnityEngine;
using EnemyClass = Tower.Enemy.Enemy;

namespace Tower.Player
{
    public class Projectile : MonoBehaviour
    {
        #region Variables
        [SerializeField] private string targetTag;
        private EnemyClass targetEnemy;
        private float damage;
        private int groggyAmount;
        #endregion

        #region Property
        
        #endregion

        #region Unity Event Method
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(targetTag))
            {
                EnemyClass enemy = other.GetComponent<EnemyClass>();
                if (enemy == targetEnemy && enemy != null)
                {
                    enemy.TakeDamage(damage, groggyAmount);
                    //이펙트 추가
                    //...
                    //TODO : 이펙트 생기면 물방울 오브젝트 분리해서 순차적으로 끄기 (이펙트 발생 > 물 끄기 > 게임오브젝트 끄기 순서대로)
                    //그러면 OnEnable에서 물방울 활성화시키는 방향으로 ㄱㄱ
                    gameObject.SetActive(false);
                }
            }
        }
        #endregion

        #region Custom Method
        public void SetTarget(EnemyClass target,float damageAmount, int gpAmount)
        {
            targetEnemy = target;
            damage = damageAmount;
            groggyAmount = gpAmount;
        }
        #endregion
    }

}
