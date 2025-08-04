using UnityEngine;
using UnityEngine.AI;

public class GroggySystem : MonoBehaviour
{
    [Header("Groggy Settings")]
    public float maxGroggyGauge = 100f;
    public float groggyDuration = 5f;
    public float groggyDamageMultiplier = 1.5f; // 그로기 중 받는 데미지 증가


    private float currentGroggyGauge = 0f;
    private bool isGroggy = false;
    private NavMeshAgent agent;
    private Animator animator;
    

    public bool IsGroggy => isGroggy;
    public float GroggyGaugePercent => currentGroggyGauge / maxGroggyGauge;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // DamageHandler나 공격 받는 곳에서 호출
    public void AddGroggyDamage(float amount)
    {
        if (isGroggy) return;

        currentGroggyGauge += amount;

        if (currentGroggyGauge >= maxGroggyGauge)
        {
            StartGroggy();
        }
    }

    void StartGroggy()
    {
        isGroggy = true;
        currentGroggyGauge = 0f;

        // 1. NavMeshAgent 정지
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // 2. 그로기 애니메이션
        if (animator != null)
        {
            animator.SetBool("Groggy",true);
        }


        // 5초 후 회복
        Invoke(nameof(EndGroggy), groggyDuration);
    }

    void EndGroggy()
    {
        isGroggy = false;

        // 1. NavMeshAgent 재활성화
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        // 2. 애니메이션 종료
        if (animator != null)
        {
            animator.SetBool("Groggy",false);
        }

    }

    // 그로기 중 추가 데미지 계산용
    public float ModifyIncomingDamage(float damage)
    {
        return isGroggy ? damage * groggyDamageMultiplier : damage;
    }
}