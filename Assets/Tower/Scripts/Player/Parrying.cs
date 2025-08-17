using UnityEngine;
using Tower.Util;
using EnemyClass = Tower.Enemy.Enemy;
using System.Collections;


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

        [Header("강공")]
        private Character character;
        private Animator animator;
        private CharacterController controller;

        [SerializeField] private float strAtkMultiplier = 3f;
        [SerializeField] private int strAtkGP = 30;
        [SerializeField] private float strAtkDuration;
        //[SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float distance = 5f;
        #endregion

        #region Property
        public bool IsParrying { get; private set; }
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            character = GetComponent<Character>();
            animator = GetComponent<Animator>();
            controller = GetComponent<CharacterController>();
        }
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
            
            //Debug.Log("패리중!");    
            float timeLeft = parryDuration;

            //슬로우모션, 흑백 연출
            Effects(true);
            animator.SetBool(AnimHash.isParrying, true);

            while (timeLeft > 0)
            {
                timeLeft -= Time.unscaledDeltaTime;
                if (InputManager.Instance.AttackPressed)
                    StartCoroutine(StrongAttack(enemy));
                yield return null;
            }

            Effects(false);

            IsParrying = false;
            animator.SetBool(AnimHash.isParrying, false);
            //Debug.Log("패리 끝!");
        }

        private IEnumerator StrongAttack(EnemyClass enemy)
        {
            Effects(false);
            //테스트용 대미지
            enemy.TakeDamage(character.Atk * character.AtkBuff * strAtkMultiplier, strAtkGP); //Animator쪽으로 옮겨야 할듯
            Vector3 direction = (enemy.transform.position - transform.position).normalized;
            transform.forward = direction;
            //이펙트는 그로기로
            //enemy.animator.SetTrigger(AnimHash.groggy); 버그많아서 취소
            float count = strAtkDuration;


            Vector3 targetPos = enemy.transform.position + enemy.transform.forward * distance;
            Vector3 totalDisplacement = targetPos - transform.position;
            Vector3 velocity = totalDisplacement / strAtkDuration;

            while (count >= 0)
            {
                //Debug.Log("응아");
                //캐릭터가 strAtkDuration 동안 지정된 transform.position (offset) 쪽으로 돌진
                Vector3 deltaMove = velocity * Time.deltaTime;
                
                controller.Move(deltaMove);
                //transform.Translate(deltaMove);
                count -= Time.deltaTime;
                yield return null;
            }
            //enemy.animator.SetTrigger(AnimHash.endGroggy);
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
