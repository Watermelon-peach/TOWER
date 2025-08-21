using UnityEngine;
using System.Collections;

/// <summary>
/// 이펙트 자동 비활성화 헬퍼 컴포넌트
/// StateMachineBehaviour에서 코루틴을 실행할 수 없을 때 사용
/// </summary>
public class EffectAutoDeactivator : MonoBehaviour
{
    private Coroutine deactivateCoroutine;

    /// <summary>
    /// 지정된 시간 후 GameObject 비활성화
    /// </summary>
    public void DeactivateAfter(float delay)
    {
        // 이미 실행 중인 코루틴이 있으면 중지
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
        }

        // 새 코루틴 시작
        deactivateCoroutine = StartCoroutine(DeactivateCoroutine(delay));
    }

    private IEnumerator DeactivateCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
        deactivateCoroutine = null;
    }

    // GameObject가 비활성화될 때 코루틴 정리
    private void OnDisable()
    {
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = null;
        }
    }

    // 즉시 비활성화
    public void DeactivateNow()
    {
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = null;
        }
        gameObject.SetActive(false);
    }

    // 파티클 시스템이 있는 경우 파티클 재생 완료 후 비활성화
    public void DeactivateWhenParticleComplete()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            StartCoroutine(WaitForParticleEnd(ps));
        }
        else
        {
            // 파티클이 없으면 2초 후 비활성화
            DeactivateAfter(2f);
        }
    }

    private IEnumerator WaitForParticleEnd(ParticleSystem ps)
    {
        // 파티클이 재생 중인 동안 대기
        while (ps != null && ps.IsAlive())
        {
            yield return null;
        }

        // 추가로 0.5초 대기 (안전 마진)
        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
    }
}