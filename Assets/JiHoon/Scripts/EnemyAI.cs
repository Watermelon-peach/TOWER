using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;
using Tower.Game;

namespace Tower.Enemy
{
    public class EnemyAI : MonoBehaviour
    {
        [Header("References")]
        private NavMeshAgent agent;
        private Animator animator;
        private Transform target;
        private Rigidbody rb;
        private SphereCollider detectionTrigger;

        [Header("Settings")]
        public float attackRange = 2f;
        public float detectionRange = 10f;
        public float attackCooldown = 2f;

        private float lastAttackTime;
        private bool isAttacking = false;

        private Vector3 attackPosition;
        private bool isPositionLocked = false;

        //private bool canAct = true;  // 행동 가능 여부

        // 스폰 시스템 참조
        private MapSpawnArea spawnArea;

        // Behaviour Graph 에이전트 참조
        private BehaviorGraphAgent behaviorAgent;

        public bool CanBehave { get; set; }
        void Start()
        {
            CanBehave = true;
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            behaviorAgent = GetComponent<BehaviorGraphAgent>();

            // 플레이어 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;

                // Behaviour Graph의 Blackboard에 Target 설정
                SetTargetInBehaviourGraph(player);
            }
            else
            {
                Debug.LogError($"태그 못찾음");
            }
        }




        void Update()
        {
            if (!CanBehave) return;

            if (target == null) return;

            // 위치가 고정되어 있으면 강제로 위치 유지
            if (isPositionLocked)
            {
                transform.position = attackPosition;
                return;
            }

            // 애니메이션만 업데이트
            UpdateAnimations();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                 Debug.Log("스페이스바 눌림!");
                 StartCoroutine(RestartBehaviorGraph());
            }

        }

        

IEnumerator RestartBehaviorGraph()
        {
            if (!CanBehave) yield break;

            // Behaviour Graph 끄기  
            if (behaviorAgent != null)
            {
                behaviorAgent.enabled = false;
            }

            // NavMeshAgent도 잠시 정지  
            if (agent != null)
            {
                agent.isStopped = true;
            }

            // 한 프레임 대기  
            yield return null;

            // 새 플레이어 찾기  
            FindPlayerTag();

            // 한 프레임 대기  
            yield return new WaitForSeconds(0.2f);

            // Behaviour Graph 켜기  
            if (behaviorAgent != null)
            {
                behaviorAgent.enabled = true;
            }

            // NavMeshAgent 재개  
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }


        public void StopMoving()
        {
            if (!CanBehave) return;
            // NavMeshAgent 완전 정지
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
            agent.updatePosition = false;
            agent.updateRotation = false;
            isAttacking = true;

            // 타겟을 즉시 바라보기
            if (target != null)
            {
                Vector3 lookDirection = (target.position - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }

            // 현재 위치 저장하고 고정
            attackPosition = transform.position;
            isPositionLocked = true;
        }

        public void StartMoving()
        {
            if (!CanBehave) return;
            // 위치 고정 해제
            isPositionLocked = false;

            // NavMeshAgent를 현재 위치에서 다시 시작하도록 설정
            agent.Warp(transform.position);

            // NavMeshAgent 다시 활성화
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.isStopped = false;
            isAttacking = false;

            // 목표 위치 재설정
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }

        public void OnAttackStart()
        {
            isAttacking = true;
            StartCoroutine(LookAtTargetDuringAnimation());
        }

        public void OnAttackEnd()
        {
            isAttacking = false;
        }

        IEnumerator LookAtTargetDuringAnimation()
        {
            while (isAttacking)
            {
                LookAtTarget();
                yield return null;  // 매 프레임 실행
            }
        }

        public void LookAtTarget()
        {
            if (!CanBehave) return;
            StartCoroutine(RotateToTarget());
        }

        private IEnumerator RotateToTarget()
        {
            if (target == null) yield break;

            float elapsed = 0f;
            float duration = 2f;
            float rotationSpeed = 720f; // 초당 720도

            while (elapsed < duration)
            {
                Vector3 lookDirection = (target.position - transform.position).normalized;
                lookDirection.y = 0;

                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // 공격 상태 확인 메서드 추가
        public bool IsAttacking()
        {
            return isAttacking;
        }

        // Behaviour Graph의 Blackboard에 타겟 설정
        void SetTargetInBehaviourGraph(GameObject playerObject)
        {
            var behaviorAgent = GetComponent<BehaviorGraphAgent>();
            if (behaviorAgent != null && behaviorAgent.BlackboardReference != null)
            {
                // Blackboard의 "Target" 변수에 플레이어 GameObject 설정
                bool success = behaviorAgent.BlackboardReference.SetVariableValue("Target", playerObject);

            }
        }

        // MapSpawnArea에서 호출
        public void SetSpawnArea(MapSpawnArea area)
        {
            spawnArea = area;
        }

        void UpdateAnimations()
        {
            // NavMeshAgent의 속도를 기반으로 이동 애니메이션 제어
            float speed = agent.velocity.magnitude;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // 공격 범위 안에 있으면 Walk 애니메이션 유지 (Idle 방지)
            if (distanceToTarget <= attackRange && distanceToTarget <= detectionRange)
            {
                {
                    animator.SetBool("IsMoving", true);  // Idle 대신 Walk 유지
                    return;
                }
            }

            // 일반적인 경우
            animator.SetBool("IsMoving", speed > 0.1f);
        }

        public void FindPlayerTag()
        {
            // 플레이어 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;

                // Behaviour Graph의 Blackboard에 Target 설정
                SetTargetInBehaviourGraph(player);
            }
            else
            {
                Debug.LogError($"태그 못찾음");
            }
        }

        public void LookAtTargetDuringAttack()
        {
            if (target == null || !isPositionLocked) return;

            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                // 즉시 플레이어를 향해 회전
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        public void OnStunnedAnimationStart()
        {
            // 모든 입력 무시
            //canAct = false;
            agent.isStopped = true;

            // Behavior Graph 일시 정지
            var behaviorAgent = GetComponent<BehaviorGraphAgent>();
            behaviorAgent.enabled = false;
        }

        public void OnStunnedAnimationEnd()
        {
            // 입력 재개
            //canAct = true;
            agent.isStopped = false;

            // Behavior Graph 재개
            var behaviorAgent = GetComponent<BehaviorGraphAgent>();
            behaviorAgent.enabled = true;
        }

        public void StartGroggyState()
        {
            StartCoroutine(DisableInputForSeconds());
            
        }

        private IEnumerator DisableInputForSeconds()
        {
            // 입력 차단
            //canAct = false;

            // Behavior Graph 비활성화
            var behaviorAgent = GetComponent<BehaviorGraphAgent>();
            if (behaviorAgent != null)
            {
                behaviorAgent.enabled = false;
            }
            

            // 10초 대기
            yield return new WaitForSeconds(10f);

            // 입력 재개
            //canAct = true;

            // Behavior Graph 재활성화
            if (behaviorAgent != null)
            {
                behaviorAgent.enabled = true;
            }
        }
        // 디버그용 - Scene 뷰에서 범위 표시
        void OnDrawGizmosSelected()
        {
            // 공격 범위
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 감지 범위
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);


        }
        public void StageClearCheck()
        {

            // 먼저 MapSpawnArea에 죽음 알림 (리스트에서 제거)
            if (spawnArea != null)  // 이미 SetSpawnArea로 설정되어 있음
            {
                Debug.Log($"SpawnArea에 죽음 알림");
                spawnArea.OnMonsterDeath(gameObject);
                // OnMonsterDeath 내부에서 자동으로 MapExit.ActivateExit() 호출됨!
            }
            else
            {
                Debug.LogWarning("SpawnArea가 null입니다!");

                // Fallback: spawnArea가 없으면 직접 찾기
                MapSpawnArea[] areas = FindObjectsByType<MapSpawnArea>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var area in areas)
                {
                    if (area.ContainsMonster(gameObject))
                    {
                        area.OnMonsterDeath(gameObject);
                        break;
                    }
                }
            }
        }
    }
}