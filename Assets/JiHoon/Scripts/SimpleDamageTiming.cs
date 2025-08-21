using UnityEngine;
using System.Collections;

/// <summary>
/// 떨어지는 공격 타이밍 조절
/// DamageZone을 적절한 타이밍에 켜고 끔
/// </summary>
public class SimpleDamageTiming : MonoBehaviour
{
    [Header("타이밍 설정")]
    [SerializeField] private float fallTime = 1f;  // 돌이 떨어지는 시간
    [SerializeField] private float damageTime = 0.5f;  // 데미지 지속 시간

    [Header("데미지 영역")]
    [SerializeField] private GameObject damageZone;  // DamageZone GameObject

    void OnEnable()
    {
        // Attack5가 활성화되면 시작
        StartCoroutine(TimingSequence());
    }

    IEnumerator TimingSequence()
    {
        // 1. 처음엔 DamageZone 꺼둠
        if (damageZone != null)
            damageZone.SetActive(false);

        // 2. 돌이 떨어지는 시간만큼 대기
        yield return new WaitForSeconds(fallTime);

        // 3. 돌이 바닥에 닿으면 DamageZone 켬 (데미지 시작!)
        if (damageZone != null)
            damageZone.SetActive(true);

        // 4. 잠깐 데미지 주고
        yield return new WaitForSeconds(damageTime);

        // 5. DamageZone 다시 끔
        if (damageZone != null)
            damageZone.SetActive(false);
    }
}