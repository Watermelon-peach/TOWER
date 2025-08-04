using UnityEngine;

namespace Tower.Player
{
    public class Dealer : Character
    {
        #region Variables
        [SerializeField] private EnemyDetector detector;
        [SerializeField] private int normalGroggyAmount = 3;
        private float skillCoolRemain; //남은 쿨타임
        private float normalAttackRatio = 1f;
        #endregion

        #region Property
        public float SkillCoolRemain => skillCoolRemain;
        #endregion

        #region Custom Method
        public void OnAttack()
        {
            foreach (var enemy in detector.detectedEnemies)
            {
                enemy.TakeDamage(Atk * normalAttackRatio * AtkBuff, normalGroggyAmount);
            }

        }
        #endregion


    }

}
