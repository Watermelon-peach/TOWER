using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Behavior Graph에서 설정한 Target을 찾거나, 태그로 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    void Update()
    {
        if (target == null) return;

        // 위치가 고정되어 있으면 강제로 위치 유지
        if (isPositionLocked)
        {
            transform.position = attackPosition;
            return;
        }

        // 애니메이션만 업데이트
        UpdateAnimations();
    }



    public void StopMoving()
    {
        // 현재 위치 즉시 저장
        attackPosition = transform.position;
        isPositionLocked = true;

        // NavMeshAgent 완전 비활성화
        if (agent != null)
        {
            // 먼저 정지시키고
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                agent.ResetPath();
                agent.updatePosition = false;
                agent.updateRotation = false;
            }

            // 완전히 비활성화
            agent.enabled = false;
        }

        // Rigidbody 완전 고정
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.isKinematic = true;
        }

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

        // 추가 보안 - 코루틴으로 지속적으로 위치 고정
        StartCoroutine(EnforcePositionLock());
    }

    // 위치 고정을 강제하는 코루틴
    IEnumerator EnforcePositionLock()
    {
        while (isPositionLocked)
        {
            // 매 프레임마다 위치 강제 고정
            transform.position = attackPosition;

            // NavMeshAgent가 다시 활성화되었다면 비활성화
            if (agent != null && agent.enabled)
            {
                agent.enabled = false;
            }

            // Rigidbody 속도도 계속 0으로
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            yield return null;
        }
    }

    public void StartMoving()
    {
        Debug.Log("StartMoving called!");

        // 일단 완전히 정지 상태 유지
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath(); // 경로 초기화
        }

        // 3초 후에 실제로 움직이기 시작
        StartCoroutine(DelayedStartMoving(2f));
    }

    IEnumerator DelayedStartMoving(float delay)
    {
        // 대기 중 위치 고정
        Vector3 waitPosition = transform.position;

        // Idle 애니메이션
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }

        float elapsed = 0f;
        while (elapsed < delay)
        {
            // 위치 강제 고정 (미끄러짐 방지)
            transform.position = waitPosition;

            // NavMeshAgent가 움직이려 하면 다시 정지
            if (agent != null && agent.enabled && !agent.isStopped)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 이제 실제로 움직이기 시작
        ActualStartMoving();
    }

    void ActualStartMoving()
    {
        // 1. 위치 고정 해제
        isPositionLocked = false;

        if (agent != null)
        {
            agent.enabled = true;  // 다시 활성화
            rb.isKinematic = false;  // Rigidbody도 원래대로
        }

        // 2. Rigidbody 제약 해제
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                            RigidbodyConstraints.FreezeRotationX |
                            RigidbodyConstraints.FreezeRotationZ;
        }

        // 3. 플레이어 향해 회전
        if (target != null)
        {
            Vector3 lookDirection = (target.position - transform.position).normalized;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        // 4. NavMeshAgent 재활성화
        if (agent != null && agent.enabled)
        {
            agent.updatePosition = true;
            agent.updateRotation = true;

            if (agent.isOnNavMesh)
            {
                agent.Warp(transform.position);
                agent.isStopped = false;

                if (target != null)
                {
                    agent.SetDestination(target.position);
                }
            }
        }

        // 5. 애니메이션 상태 업데이트
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }

    public void LookAtTarget()
    {
        if (target == null) return;

        Vector3 lookDirection = (target.position - transform.position).normalized;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }


    // 공격 상태 확인 메서드 추가
    public bool IsAttacking()
    {
        return isAttacking;
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
}