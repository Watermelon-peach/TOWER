using System.Collections.Generic;
using Tower.Effects;

using UnityEngine;

/// <summary>
/// Animator State에 직접 붙여서 사용하는 이펙트 시스템
/// 각 공격 애니메이션 State에 이 Behaviour를 추가하면 자동으로 이펙트 실행
/// </summary>
public class AnimationStateEffect : StateMachineBehaviour
{
    [Header("=== Effect Timing ===")]
    [SerializeField, Range(0f, 1f)]
    [Tooltip("이펙트가 실행될 애니메이션 시점 (0 = 시작, 0.5 = 중간, 1 = 끝)")]
    private float effectTriggerTime = 0.4f;

    [SerializeField]
    [Tooltip("애니메이션이 루프할 때마다 이펙트 반복 실행")]
    private bool repeatOnLoop = false;

    [Header("=== Effect Data ===")]
    [SerializeField]
    [Tooltip("실행할 이펙트 데이터 (ScriptableObject)")]
    private AttackEffectData primaryEffect;

    [SerializeField]
    [Tooltip("Effect 풀에서 직접 오브젝트 이름으로 찾기 (ScriptableObject 없을 때)")]
    private string effectObjectName = "";

    [SerializeField]
    [Tooltip("추가 이펙트 사용 여부")]
    private bool useMultipleEffects = false;

    [SerializeField]
    [Tooltip("추가로 실행할 이펙트들")]
    private List<AttackEffectData> additionalEffects = new List<AttackEffectData>();

    [Header("=== Position Settings ===")]
    [SerializeField]
    [Tooltip("ScriptableObject 설정 무시하고 직접 위치 지정")]
    private bool overridePosition = false;

    [SerializeField]
    [Tooltip("직접 지정할 스폰 타입")]
    private EffectSpawnType overrideSpawnType = EffectSpawnType.AtFirePoint;

    [SerializeField]
    [Tooltip("위치 오프셋")]
    private Vector3 positionOffset = Vector3.zero;

    [Header("=== Scale Settings ===")]
    [SerializeField]
    [Tooltip("스케일 덮어쓰기")]
    private bool overrideScale = false;

    [SerializeField]
    [Tooltip("덮어쓸 스케일 값")]
    private Vector3 customScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("=== Advanced Settings ===")]
    [SerializeField]
    [Tooltip("여러 시점에서 이펙트 실행")]
    private bool useMultipleTriggerPoints = false;

    [SerializeField]
    [Tooltip("추가 실행 시점들")]
    private List<float> additionalTriggerTimes = new List<float> { 0.6f, 0.8f };

    [Header("=== Audio (Optional) ===")]
    [SerializeField]
    [Tooltip("이펙트와 함께 재생할 사운드")]
    private AudioClip effectSound;

    [SerializeField, Range(0f, 1f)]
    [Tooltip("사운드 볼륨")]
    private float soundVolume = 1f;

    // 내부 상태 관리
    private bool hasTriggeredPrimary = false;
    private Dictionary<float, bool> triggerStatus = new Dictionary<float, bool>();
    private EnemyEffectSpawnManager effectManager;
    private Transform playerTransform;
    private Transform firePoint;
    private AudioSource audioSource;
    private Transform effectPoolParent;
    private Dictionary<string, GameObject> effectPool = new Dictionary<string, GameObject>();

    // State 진입 시 (애니메이션 시작)
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 초기화
        ResetTriggerStatus();
        CacheComponents(animator);

#if UNITY_EDITOR
        if (animator.GetCurrentAnimatorClipInfo(layerIndex).Length > 0)
        {
            Debug.Log($"[Attack State] Entered: {animator.GetCurrentAnimatorClipInfo(layerIndex)[0].clip.name}");
        }
#endif
    }

    // State 업데이트 중 (애니메이션 재생 중)
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 메인 이펙트 실행 체크
        CheckAndTriggerEffect(animator, stateInfo.normalizedTime, effectTriggerTime, ref hasTriggeredPrimary, true);

        // 추가 시점 이펙트 실행 체크
        if (useMultipleTriggerPoints)
        {
            for (int i = 0; i < additionalTriggerTimes.Count; i++)
            {
                float triggerTime = additionalTriggerTimes[i];
                if (!triggerStatus.ContainsKey(triggerTime))
                    triggerStatus[triggerTime] = false;

                bool triggered = triggerStatus[triggerTime];
                CheckAndTriggerEffect(animator, stateInfo.normalizedTime, triggerTime, ref triggered, false);
                triggerStatus[triggerTime] = triggered;
            }
        }

        // 애니메이션 루프 처리
        if (repeatOnLoop && stateInfo.normalizedTime >= 1f)
        {
            ResetTriggerStatus();
        }
    }

    // State 종료 시 (애니메이션 끝)
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 상태 초기화
        ResetTriggerStatus();

#if UNITY_EDITOR
        Debug.Log($"[Attack State] Exited");
#endif
    }

    /// <summary>
    /// 이펙트 실행 체크 및 트리거
    /// </summary>
    private void CheckAndTriggerEffect(Animator animator, float normalizedTime, float triggerTime, ref bool hasTriggered, bool isPrimary)
    {
        // 이미 실행했으면 스킵
        if (hasTriggered && !repeatOnLoop) return;

        // 지정된 시간에 도달했는지 체크
        if (normalizedTime >= triggerTime && !hasTriggered)
        {
            if (isPrimary)
            {
                ExecutePrimaryEffect(animator);
            }
            else
            {
                ExecuteAdditionalEffects(animator);
            }

            hasTriggered = true;
        }
    }

    /// <summary>
    /// 메인 이펙트 실행
    /// </summary>
    private void ExecutePrimaryEffect(Animator animator)
    {
        // EnemyEffectSpawnManager를 사용하는 경우
        if (effectManager != null && primaryEffect != null)
        {
            effectManager.SpawnEffect(primaryEffect, playerTransform);
            PlaySound();
        }
        // 직접 Effect Pool에서 오브젝트를 활성화하는 경우
        else if (!string.IsNullOrEmpty(effectObjectName))
        {
            SpawnEffectDirectly(animator, effectObjectName, primaryEffect);
            PlaySound();
        }
        else if (primaryEffect != null)
        {
            // effectManager가 없어도 primaryEffect의 이름으로 시도
            SpawnEffectDirectly(animator, primaryEffect.effectName, primaryEffect);
            PlaySound();
        }

        // 추가 이펙트들 실행
        if (useMultipleEffects && additionalEffects != null)
        {
            foreach (var effect in additionalEffects)
            {
                if (effect != null)
                {
                    if (effectManager != null)
                    {
                        effectManager.SpawnEffect(effect, playerTransform);
                    }
                    else
                    {
                        SpawnEffectDirectly(animator, effect.effectName, effect);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 추가 시점 이펙트 실행
    /// </summary>
    private void ExecuteAdditionalEffects(Animator animator)
    {
        if (primaryEffect != null)
        {
            if (effectManager != null)
            {
                effectManager.SpawnEffect(primaryEffect, playerTransform);
            }
            else
            {
                SpawnEffectDirectly(animator, primaryEffect.effectName, primaryEffect);
            }
        }
    }

    /// <summary>
    /// Effect Pool에서 직접 이펙트 스폰
    /// </summary>
    private void SpawnEffectDirectly(Animator animator, string effectName, AttackEffectData effectData)
    {
        if (string.IsNullOrEmpty(effectName)) return;

        GameObject effectObj = null;

        // Effect Pool에서 찾기
        if (effectPool.ContainsKey(effectName))
        {
            effectObj = effectPool[effectName];
        }
        else if (effectPoolParent != null)
        {
            // Pool에서 이름으로 찾기
            Transform effect = effectPoolParent.Find(effectName);
            if (effect != null)
            {
                effectObj = effect.gameObject;
                effectPool[effectName] = effectObj;
            }
        }

        if (effectObj != null)
        {
            // 위치 계산
            Vector3 spawnPosition = CalculateSpawnPosition(animator, effectData);
            effectObj.transform.position = spawnPosition;

            // 회전 설정 (Look At Target이 true일 때만)
            if (effectData != null && effectData.lookAtTarget && playerTransform != null)
            {
                Vector3 direction = (playerTransform.position - spawnPosition).normalized;
                if (direction != Vector3.zero)
                    effectObj.transform.rotation = Quaternion.LookRotation(direction);
            }

            // 스케일 설정
            if (overrideScale)
            {
                effectObj.transform.localScale = customScale;
            }
            else if (effectData != null)
            {
                effectObj.transform.localScale = effectData.scale;
            }

            // 활성화
            effectObj.SetActive(true);

            // Follow Target 처리 (ScriptableObject 설정에 따라)
            if (effectData != null && effectData.followTarget && playerTransform != null)
            {
                // 기존 컨트롤러들 제거
                var oldFollower = effectObj.GetComponent<SimpleFollowController>();
                if (oldFollower != null)
                {
                    MonoBehaviour.Destroy(oldFollower);
                }


                var oldLocker = effectObj.GetComponent<EffectRotationLocker>();
                if (oldLocker != null)
                {
                    MonoBehaviour.Destroy(oldLocker);
                }

            }

            // 자동 비활성화
            if (effectData != null && effectData.autoDeactivate)
            {
                // EffectAutoDeactivator 컴포넌트 사용
                var deactivator = effectObj.GetComponent<EffectAutoDeactivator>();
                if (deactivator == null)
                {
                    deactivator = effectObj.AddComponent<EffectAutoDeactivator>();
                }
                deactivator.DeactivateAfter(effectData.duration);
            }

            Debug.Log($"[Effect Spawned] {effectName} at {spawnPosition}");
        }
        else
        {
            Debug.LogWarning($"Effect '{effectName}' not found in pool!");
        }
    }

    /// <summary>
    /// 스폰 위치 계산
    /// </summary>
    private Vector3 CalculateSpawnPosition(Animator animator, AttackEffectData effectData)
    {
        Vector3 position = animator.transform.position;
        EffectSpawnType spawnType = EffectSpawnType.AtEnemyPosition;

        // 스폰 타입 결정
        if (overridePosition)
        {
            spawnType = overrideSpawnType;
        }
        else if (effectData != null)
        {
            spawnType = effectData.spawnType;
        }

        // 위치 계산
        switch (spawnType)
        {
            case EffectSpawnType.AtFirePoint:
                if (firePoint != null)
                    position = firePoint.position;
                else
                    position = animator.transform.position + animator.transform.forward * 1f;
                break;

            case EffectSpawnType.AtPlayer:
                if (playerTransform != null)
                    position = playerTransform.position;
                break;

            case EffectSpawnType.InFrontOfEnemy:
                position = animator.transform.position + animator.transform.forward * 2f;
                break;

            case EffectSpawnType.AtTarget:
                if (playerTransform != null)
                    position = playerTransform.position;
                break;

            case EffectSpawnType.BetweenEnemyAndPlayer:
                if (playerTransform != null)
                    position = Vector3.Lerp(animator.transform.position, playerTransform.position, 0.5f);
                break;

            case EffectSpawnType.AtEnemyPosition:
            default:
                position = animator.transform.position;
                break;
        }

        // 오프셋 적용
        if (effectData != null)
        {
            position += effectData.positionOffset;
        }
        position += positionOffset;

        return position;
    }

    /// <summary>
    /// 컴포넌트 캐싱
    /// </summary>
    private void CacheComponents(Animator animator)
    {
        // EnemyEffectSpawnManager 찾기 (여러 방법 시도)
        if (effectManager == null)
        {
            // 1. 같은 GameObject에서
            effectManager = animator.GetComponent<EnemyEffectSpawnManager>();

            // 2. 부모에서
            if (effectManager == null && animator.transform.parent != null)
            {
                effectManager = animator.transform.parent.GetComponentInChildren<EnemyEffectSpawnManager>();
            }

            // 3. 자식에서
            if (effectManager == null)
            {
                effectManager = animator.GetComponentInChildren<EnemyEffectSpawnManager>();
            }
        }

        // FirePoint 찾기
        if (firePoint == null)
        {
            // 직접 찾기
            firePoint = animator.transform.Find("FirePoint");

            // 자식들 중에서 찾기
            if (firePoint == null)
            {
                Transform[] children = animator.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    if (child.name.Contains("FirePoint") || child.name.Contains("firepoint") ||
                        child.name.Contains("Firepoint") || child.name.Contains("ShootPoint"))
                    {
                        firePoint = child;
                        break;
                    }
                }
            }
        }

        // Effect Pool Parent 찾기
        if (effectPoolParent == null)
        {
            // 1. Effect 폴더 직접 찾기
            effectPoolParent = animator.transform.Find("Effect");

            // 2. WizardEffectManager에서 찾기
            if (effectPoolParent == null)
            {
                Transform manager = animator.transform.Find("WizardEffectManager");
                if (manager != null)
                {
                    effectPoolParent = manager.Find("Effect");
                }
            }

            // 3. 자식들 중에서 찾기
            if (effectPoolParent == null)
            {
                foreach (Transform child in animator.transform)
                {
                    if (child.name == "Effect" || child.name == "Effects")
                    {
                        effectPoolParent = child;
                        break;
                    }

                    Transform effectChild = child.Find("Effect");
                    if (effectChild != null)
                    {
                        effectPoolParent = effectChild;
                        break;
                    }
                }
            }

            // Effect Pool 초기화
            if (effectPoolParent != null)
            {
                foreach (Transform effect in effectPoolParent)
                {
                    effectPool[effect.name] = effect.gameObject;
                    effect.gameObject.SetActive(false); // 초기에는 비활성화
                }
            }
        }

        // 플레이어 찾기
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // 오디오 소스 찾기
        if (audioSource == null && effectSound != null)
        {
            audioSource = animator.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = animator.gameObject.AddComponent<AudioSource>();
            }
        }

        // 디버그 정보
        Debug.Log($"[Cache Complete] EffectManager: {effectManager != null}, FirePoint: {firePoint != null}, " +
                  $"EffectPool: {effectPoolParent != null}, Player: {playerTransform != null}");
    }

    /// <summary>
    /// 트리거 상태 초기화
    /// </summary>
    private void ResetTriggerStatus()
    {
        hasTriggeredPrimary = false;
        triggerStatus.Clear();

        if (useMultipleTriggerPoints)
        {
            foreach (float time in additionalTriggerTimes)
            {
                triggerStatus[time] = false;
            }
        }
    }

    /// <summary>
    /// 사운드 재생
    /// </summary>
    private void PlaySound()
    {
        if (audioSource != null && effectSound != null)
        {
            audioSource.PlayOneShot(effectSound, soundVolume);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Inspector에서 보기 좋게 표시
    /// </summary>
    private void OnValidate()
    {
        // 트리거 시간 유효성 검사
        effectTriggerTime = Mathf.Clamp01(effectTriggerTime);

        if (additionalTriggerTimes != null)
        {
            for (int i = 0; i < additionalTriggerTimes.Count; i++)
            {
                additionalTriggerTimes[i] = Mathf.Clamp01(additionalTriggerTimes[i]);
            }
        }
    }
#endif
}