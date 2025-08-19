using System.Collections;
using UnityEngine;
using EnemyClass = Tower.Enemy.Enemy;

namespace Tower.Player
{
    public class Healer : Character
    {

        #region Variables
        [SerializeField] private EnemyDetector detector;
        [SerializeField] private int normalGroggyAmount = 3;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float gravity = -9.81f;

        [Header("원거리 공격")]
        [SerializeField] private Transform pool;
        [SerializeField] private int poolCount;
        [SerializeField] private GameObject[] projectiles;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float normalAttackRatio = 1f;
        [SerializeField] private float projectileSpeed = 1f;
        [SerializeField] private float hitPointOffset = 1f;
        private float skillCoolRemain;
        private bool isAttacking = false;

        private EnemyClass currentTarget;
        private Vector3 currentTargetDirection = Vector3.forward;


        private CharacterController controller;
        private float verticalVelocity;

        public float SkillCoolRemain => skillCoolRemain;
        #endregion

        #region Unity Event Method
        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<CharacterController>();

            poolCount = pool.childCount;
            projectiles = new GameObject[poolCount];
            // 투사체 배열에 각 오브젝트 할당
            for (int i = 0; i < poolCount; i++)
            {
                projectiles[i] = pool.GetChild(i).gameObject;
            }

            //초기 셋팅 (비활성화)
            foreach (GameObject go in projectiles)
            {
                go.SetActive(false);
            }
        }

        private void Update()
        {
            // 공격 중인데 적이 아예 없으면 강제로 회전 해제
            if (isAttacking && detector.detectedEnemies.Count == 0)
            {
                EndAttack();
            }

            // 공격 중일 때만 타겟 방향 갱신
            if (isAttacking)
                UpdateTargetDirection();
        }

        private void OnAnimatorMove()
        {
            if (animator == null || controller == null) return;

            // 루트모션 이동 계산
            Vector3 motion = animator.deltaPosition;

            // 중력 보정
            if (controller.isGrounded)
                verticalVelocity = 0f;
            else
                verticalVelocity += gravity * Time.deltaTime;

            motion.y = verticalVelocity * Time.deltaTime;

            // 이동 적용
            controller.Move(motion);

            // 공격 중일 때만 회전
            if (isAttacking && currentTargetDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentTargetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
        #endregion

        #region Custom Method
        public void OnAttack()
        {
            isAttacking = detector.detectedEnemies.Count > 0;

            if (isAttacking)
            {
                //제일 가까운 적 타겟팅
                EnemyClass target = currentTarget;

                if (target == null || target.IsDead) return;

                //꺼져 있는 투사체 하나 갖고오기
                foreach (GameObject go in projectiles)
                {
                    if (!go.activeSelf)
                    {
                        //투사체 클래스 갖고오기
                        Projectile projectile = go.GetComponent<Projectile>();
                        if (projectile == null) continue;

                        //투사체 파이어포인트로 불러오기 (1)
                        //projectile.transform.position = firePoint.position;
                        StartCoroutine(LaunchProjectile(projectile, target));
                        break;
                    }
                }
            }
        }

        private IEnumerator LaunchProjectile(Projectile projectile, EnemyClass target)
        {
            //투사체 파이어포인트로 불러오기(2)
            //projectile.transform.position = firePoint.position;
            // 발사 위치 세팅 (y값 안전 범위 보정)
            Vector3 startPos = firePoint.position;
            if (startPos.y < 0.2f) // 너무 낮으면 살짝 올려줌
                startPos.y = 0.2f;

            projectile.transform.position = startPos;
            projectile.gameObject.SetActive(true);

            //투사체 셋팅
            projectile.SetTarget(target, normalAttackRatio * Atk * AtkBuff, normalGroggyAmount);

            //투사체 이동
            while(projectile.gameObject.activeSelf)
            {
                //날아가는 도중에 타겟이 죽으면 즉시 오브젝트 비활성화 후 반복문 종료
                if (target == null || target.IsDead)
                {
                    projectile.gameObject.SetActive(false);
                    yield break;
                }

                //타격 부위 보정
                //Vector3 targetPosition = new Vector3(target.transform.position.x, 1f, target.transform.position.z);
                Vector3 targetPosition = target.transform.position + Vector3.up * hitPointOffset;
                Vector3 dir = (targetPosition - projectile.transform.position).normalized;
                projectile.transform.Translate(dir * projectileSpeed * Time.deltaTime);
                
                yield return null;
            }
        }

        public void EndAttack()
        {
            isAttacking = false; // 공격 끝나면 회전 고정 해제
            currentTarget = null;
        }

        //강공격 구현
        public override void OnStrongAttack()
        {
            base.OnStrongAttack();
        }

        //교체공격 구현
        public override void SwitchCombo()
        {
            base.SwitchCombo();
        }

        private void UpdateTargetDirection()
        {
            currentTarget = GetClosestEnemy();
            if (currentTarget == null) return;

            Vector3 dir = currentTarget.transform.position - transform.position;
            dir.y = 0f;
            currentTargetDirection = dir.normalized;
        }

        private EnemyClass GetClosestEnemy()
        {
            EnemyClass closest = null;
            float closestDist = Mathf.Infinity;

            foreach (var enemy in detector.detectedEnemies)
            {
                if (enemy == null) continue;
                float dist = (transform.position - enemy.transform.position).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }
            return closest;
        }
        #endregion
    }

}
