using System.Collections.Generic;
using Tower.Effects;
using Tower.Enemy;
using UnityEngine;

/// <summary>
/// Animator State에 직접 붙여서 사용하는 이펙트 시스템
/// 매번 실행 시 현재 활성화된 Player를 찾아서 타겟팅
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
    private Transform firePoint;
    private AudioSource audioSource;
    private Transform effectPoolParent;
    private Dictionary<string, GameObject> effectPool = new Dictionary<string, GameObject>();

    // EnemyAI 컴포넌트 참조 (타겟 찾기용)
    private EnemyAI enemyAI;

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
    /// 현재 활성화된 플레이어 찾기 - EnemyAI의 타겟 찾기 로직 활용
    /// </summary>
    private Transform GetCurrentPlayerTarget()
    {
        // 1. EnemyAI가 있으면 FindPlayerTag() 호출해서 최신 타겟 갱신
        if (enemyAI != null)
        {
            enemyAI.FindPlayerTag();

            // EnemyAI의 target 사용
            if (enemyAI.Target!= null && enemyAI.Target.gameObject.activeInHierarchy)
            {
                return enemyAI.Target;
            }
        }

        // 2. EnemyAI가 없으면 직접 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.activeInHierarchy)
        {
            return player.transform;
        }

        return null;
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
        // 현재 플레이어 타겟 찾기
        Transform currentPlayer = GetCurrentPlayerTarget();

        if (currentPlayer == null)
        {
            Debug.LogWarning($"[AnimationStateEffect] No valid player target found!");
            return;
        }

        // EnemyEffectSpawnManager를 사용하는 경우
        if (effectManager != null && primaryEffect != null)
        {
            effectManager.SpawnEffect(primaryEffect, currentPlayer);
            PlaySound();
        }
        // 직접 Effect Pool에서 오브젝트를 활성화하는 경우
        else if (!string.IsNullOrEmpty(effectObjectName))
        {
            SpawnEffectDirectly(animator, effectObjectName, primaryEffect, currentPlayer);
            PlaySound();
        }
        else if (primaryEffect != null)
        {
            // effectManager가 없어도 primaryEffect의 이름으로 시도
            SpawnEffectDirectly(animator, primaryEffect.effectName, primaryEffect, currentPlayer);
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
                        effectManager.SpawnEffect(effect, currentPlayer);
                    }
                    else
                    {
                        SpawnEffectDirectly(animator, effect.effectName, effect, currentPlayer);
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
        Transform currentPlayer = GetCurrentPlayerTarget();

        if (currentPlayer == null) return;

        if (primaryEffect != null)
        {
            if (effectManager != null)
            {
                effectManager.SpawnEffect(primaryEffect, currentPlayer);
            }
            else
            {
                SpawnEffectDirectly(animator, primaryEffect.effectName, primaryEffect, currentPlayer);
            }
        }
    }

    /// <summary>
    /// Effect Pool에서 직접 이펙트 스폰
    /// </summary>
    private void SpawnEffectDirectly(Animator animator, string effectName, AttackEffectData effectData, Transform targetPlayer)
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
            Vector3 spawnPosition = CalculateSpawnPosition(animator, effectData, targetPlayer);
            effectObj.transform.position = spawnPosition;

            // 회전 설정 (Look At Target이 true일 때만)
            if (effectData != null && effectData.lookAtTarget && targetPlayer != null)
            {
                Vector3 direction = (targetPlayer.position - spawnPosition).normalized;
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

            // SimpleDamageHandler가 있으면 초기화
            var damageHandler = effectObj.GetComponent<SimpleDamageHandler>();
            if (damageHandler != null && effectData != null)
            {
                damageHandler.Initialize(effectData);
            }


            // 활성화
            effectObj.SetActive(true);

            // 자동 비활성화
            if (effectData != null && effectData.autoDeactivate)
            {
                var deactivator = effectObj.GetComponent<EffectAutoDeactivator>();
                if (deactivator == null)
                {
                    deactivator = effectObj.AddComponent<EffectAutoDeactivator>();
                }
                deactivator.DeactivateAfter(effectData.duration);
            }

            Debug.Log($"[Effect Spawned] {effectName} targeting {targetPlayer.name} at {spawnPosition}");
        }
        else
        {
            Debug.LogWarning($"Effect '{effectName}' not found in pool!");
        }
    }

    /// <summary>
    /// 스폰 위치 계산
    /// </summary>
    private Vector3 CalculateSpawnPosition(Animator animator, AttackEffectData effectData, Transform targetPlayer)
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
            case EffectSpawnType.AtTarget:
                if (targetPlayer != null)
                    position = targetPlayer.position;
                break;

            case EffectSpawnType.InFrontOfEnemy:
                position = animator.transform.position + animator.transform.forward * 2f;
                break;

            case EffectSpawnType.BetweenEnemyAndPlayer:
                if (targetPlayer != null)
                    position = Vector3.Lerp(animator.transform.position, targetPlayer.position, 0.5f);
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
        // EnemyAI 컴포넌트 찾기
        if (enemyAI == null)
        {
            enemyAI = animator.GetComponent<EnemyAI>();
            if (enemyAI == null)
            {
                enemyAI = animator.GetComponentInParent<EnemyAI>();
            }
            if (enemyAI == null)
            {
                enemyAI = animator.GetComponentInChildren<EnemyAI>();
            }
        }

        // EnemyEffectSpawnManager 찾기
        if (effectManager == null)
        {
            effectManager = animator.GetComponent<EnemyEffectSpawnManager>();
            if (effectManager == null && animator.transform.parent != null)
            {
                effectManager = animator.transform.parent.GetComponentInChildren<EnemyEffectSpawnManager>();
            }
            if (effectManager == null)
            {
                effectManager = animator.GetComponentInChildren<EnemyEffectSpawnManager>();
            }
        }

        // FirePoint 찾기
        if (firePoint == null)
        {
            firePoint = animator.transform.Find("FirePoint");
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
            effectPoolParent = animator.transform.Find("Effect");
            if (effectPoolParent == null)
            {
                Transform manager = animator.transform.Find("WizardEffectManager");
                if (manager != null)
                {
                    effectPoolParent = manager.Find("Effect");
                }
            }
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
                    effect.gameObject.SetActive(false);
                }
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
        Debug.Log($"[Cache Complete] EnemyAI: {enemyAI != null}, EffectManager: {effectManager != null}, " +
                  $"FirePoint: {firePoint != null}, EffectPool: {effectPoolParent != null}");
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