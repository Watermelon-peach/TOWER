using System.Collections;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.AI;
using EnemyClass = Tower.Enemy.Enemy;

namespace Tower.Player
{
    public class Tanker : Character
    {
        [SerializeField] private EnemyDetector detector;
        [SerializeField] private int normalGroggyAmount = 3;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float gravity = -9.81f;

        private float skillCoolRemain;
        private float normalAttackRatio = 1f;
        private bool isAttacking = false;

        private EnemyClass currentTarget;
        private Vector3 currentTargetDirection = Vector3.forward;


        private CharacterController controller;
        private float verticalVelocity;

        public float SkillCoolRemain => skillCoolRemain;

        [Header("넉백")]
        //[SerializeField] private float power = 5;
        [SerializeField] private float duration = 1f;
        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<CharacterController>();
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

        public void OnAttack()
        {
            isAttacking = detector.detectedEnemies.Count > 0;

            if (isAttacking)
            {
                foreach (var enemy in detector.detectedEnemies)
                {
                    if (enemy == null) continue;
                    enemy.TakeDamage(Atk * normalAttackRatio * AtkBuff, normalGroggyAmount);
                }
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

            strongAtkVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            strongAtkVfx.Play();

            Collider[] enemies = Physics.OverlapSphere(strongAtkVfx.transform.position, 5f, enemyLayer);
            Debug.Log("피격된 적 수: " + enemies.Length);
            foreach (var enemy in enemies)
            {
                //enemy.GetComponent<EnemyClass>().TakeDamage(Atk * AtkBuff * strongAtackMultiplier, normalGroggyAmount * 3);
                EnemyClass _enemy = enemy.GetComponent<EnemyClass>();
                Vector3 dir = (enemy.transform.position - strongAtkVfx.transform.position).normalized;
                _enemy.TakeDamage(Atk * AtkBuff * strongAtackMultiplier, normalGroggyAmount * 3);
                StartCoroutine(_enemy.Knockback(dir, duration));
            }
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
    }

}
