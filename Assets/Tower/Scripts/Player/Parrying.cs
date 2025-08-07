using UnityEngine;
using Tower.Util;
using EnemyClass = Tower.Enemy.Enemy;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Tower.Player
{
    /// <summary>
    /// 패링처리를 하는 클래스
    /// </summary>
    public class Parrying : MonoBehaviour
    {
        #region Variables
        [Header("패링 범위")]
        [SerializeField] private float radius = 5f;

        [Header("패리모드 지속시간")]
        [SerializeField] private float parryDuration = 3f;

        [Header("적 레이어")]
        [SerializeField] private LayerMask targetLayer;
        
        [Header("이펙트")]
        [SerializeField] private GrayscaleEffect grayEffect;
        #endregion

        #region Property
        public bool IsParrying { get; private set; }
        #endregion

        #region Custom Method
        public void TryParry()
        {
            //범위 내 적 찾기
            Collider[] Enemies = Physics.OverlapSphere(transform.position, radius, targetLayer);

            foreach (Collider collider in Enemies)
            {
                EnemyClass enemy = collider.GetComponent<EnemyClass>();
                if (enemy != null)
                {
                    if (enemy.CanParry)
                    {
                        Debug.Log("패리!");
                        StartCoroutine(OnParryMode(enemy));
                    }
                }
            }
        }

        private IEnumerator OnParryMode(EnemyClass enemy)
        {
            IsParrying = true;
            
            Debug.Log("패리중!");
            float timeLeft = parryDuration;

            //슬로우모션, 흑백 연출
            Effects(true);

            while (timeLeft > 0)
            {
                timeLeft -= Time.unscaledDeltaTime;
                if (InputManager.Instance.AttackPressed)
                    StrongAttack(enemy);
                yield return null;
            }

            Effects(false);

            IsParrying = false;
            Debug.Log("패리 끝!");
        }

        private void StrongAttack(EnemyClass enemy)
        {
            Effects(false);
            //테스트용 대미지
            enemy.TakeDamage(10f, 100);
        }

        //연출
        private void Effects(bool enable)
        {
            grayEffect.SetGrayscale(enable);
            Time.timeScale = (enable) ? 0.3f : 1f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        #endregion
    }

}
