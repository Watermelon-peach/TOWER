using UnityEngine;
using Tower.Effects;

/// <summary>
/// AttackEffectData 설정에 따라 타겟을 추적하는 컨트롤러
/// AnimationStateEffect에서 자동 추가됨
/// </summary>
public class SimpleFollowController : MonoBehaviour
{
    private Transform target;
    private AttackEffectData effectData;
    private float elapsedTime = 0f;
    private float followDelayTimer = 0f;
    private bool canFollow = false;
    private bool isFollowing = false;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(Transform targetTransform, AttackEffectData data)
    {
        target = targetTransform;
        effectData = data;
        elapsedTime = 0f;
        followDelayTimer = 0f;
        canFollow = false;
        isFollowing = true;
    }

    void Update()
    {
        if (!isFollowing || target == null || effectData == null) return;

        // 전체 지속 시간 체크
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= effectData.duration)
        {
            isFollowing = false;
            return;
        }

        // 추적 시작 딜레이
        if (!canFollow)
        {
            followDelayTimer += Time.deltaTime;
            if (followDelayTimer >= effectData.followDelay)
            {
                canFollow = true;
            }
            else
            {
                return; // 딜레이 중에는 추적 안 함
            }
        }

        // 거리 체크
        float distance = Vector3.Distance(transform.position, target.position);

        // 최대 거리 체크
        if (distance > effectData.maxFollowDistance)
        {
            return; // 너무 멀면 추적 안 함
        }

        // 정지 거리 체크
        if (distance <= effectData.stopDistance)
        {
            return; // 너무 가까우면 멈춤
        }

        // 타겟 위치
        Vector3 targetPosition = target.position + effectData.positionOffset;

        // 이동
        if (effectData.smoothFollow)
        {
            // 부드러운 이동 (Lerp)
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                effectData.followSpeed * Time.deltaTime
            );
        }
        else
        {
            // 일정 속도 이동 (MoveTowards)
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                effectData.followSpeed * Time.deltaTime
            );
        }

        // Look At Target 처리
        if (effectData.lookAtTarget)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    void OnDisable()
    {
        // 비활성화될 때 정리
        isFollowing = false;
        target = null;

        // 컴포넌트 자동 제거
        Destroy(this);
    }
}