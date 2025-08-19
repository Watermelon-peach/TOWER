using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class PooledVfx : MonoBehaviour
{
    [Header("Optional Components")]
    public VisualEffect vfx;          // VFX Graph이면 여기에 연결
    public ParticleSystem ps;         // Particle System이면 여기에 연결

    private Transform followTarget;
    private bool followWorldSpace;
    private Vector3 followOffset;

    private System.Action<PooledVfx> onFinished;

    public void Play(
        Transform attachTo,
        bool worldSpaceFollow,
        Vector3 offset,
        float lifetime,
        System.Action<PooledVfx> onFinishedCallback)
    {
        followTarget = attachTo;
        followWorldSpace = worldSpaceFollow;
        followOffset = offset;
        onFinished = onFinishedCallback;

        // 위치 세팅
        if (followTarget != null)
        {
            if (followWorldSpace)
            {
                transform.SetPositionAndRotation(followTarget.position + offset, followTarget.rotation);
                transform.SetParent(null, true); // 월드 공간 유지
            }
            else
            {
                transform.SetParent(followTarget, false);
                transform.localPosition = offset;
                transform.localRotation = Quaternion.identity;
            }
        }

        // 재생
        if (vfx != null) vfx.Play();
        if (ps != null) ps.Play(true);

        StopAllCoroutines();
        StartCoroutine(Co_Watch(lifetime));
        gameObject.SetActive(true);
    }

    private IEnumerator Co_Watch(float lifetime)
    {
        if (lifetime > 0f)
        {
            yield return new WaitForSecondsRealtime(lifetime);
        }
        else
        {
            // lifetime이 0 이하이면 시스템이 끝날 때까지 대기
            // VFX Graph
            if (vfx != null)
            {
#if UNITY_2021_2_OR_NEWER
                while (vfx.aliveParticleCount > 0 || vfx.aliveParticleCount == -1) // 일부 버전 보호
                    yield return null;
#else
                // 구버전 대기(간단 폴백)
                yield return new WaitForSecondsRealtime(2f);
#endif
            }

            // Particle System
            if (ps != null)
            {
                while (ps.IsAlive(true))
                    yield return null;
            }
        }

        Stop();
        onFinished?.Invoke(this);
    }

    public void Stop()
    {
        if (vfx != null) vfx.Stop();          // 필요 시 vfx.Reinit() 고려
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        followTarget = null;
        transform.SetParent(null, true);
        gameObject.SetActive(false);
    }
}
