using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Tower.Effects
{
    public enum EffectSpawnType
    {
        AtPlayer,           // 플레이어 위치에 생성
        AtFirePoint,        // FirePoint 위치에 생성
        InFrontOfEnemy,     // Enemy 앞쪽에 생성
        AtTarget,           // 타겟 위치에 생성
        BetweenEnemyAndPlayer, // Enemy와 Player 사이
        AtEnemyPosition     // Enemy 자체 위치
    }
}

namespace Tower.Effects
    {
        public class EnemyEffectSpawnManager : MonoBehaviour
        {
            [Header("References")]
            [SerializeField] private Transform firePoint;          // 발사 지점
            [SerializeField] private Transform effectPoolParent;   // Effect 폴더 (하이라키)

            [Header("Effect Pool")]
            [SerializeField] private List<GameObject> effectPool = new List<GameObject>();  // 미리 생성된 이펙트들

            private Transform playerTransform;
            private Dictionary<string, GameObject> effectDictionary = new Dictionary<string, GameObject>();

            void Start()
            {
                // Player 찾기
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;

                // 이펙트 풀 초기화 (하이라키에 있는 이펙트들을 Dictionary에 등록)
                InitializeEffectPool();
            }

            void InitializeEffectPool()
            {
                // effectPoolParent 아래의 모든 자식 오브젝트를 Dictionary에 등록
                if (effectPoolParent != null)
                {
                    foreach (Transform child in effectPoolParent)
                    {
                        effectDictionary[child.name] = child.gameObject;
                        child.gameObject.SetActive(false);  // 초기에는 비활성화
                    }
                }

                // Inspector에서 수동으로 할당한 이펙트들도 등록
                foreach (var effect in effectPool)
                {
                    if (effect != null && !effectDictionary.ContainsKey(effect.name))
                    {
                        effectDictionary[effect.name] = effect;
                        effect.SetActive(false);
                    }
                }
            }

            /// <summary>
            /// 이펙트 스폰 메인 메서드
            /// </summary>
            public void SpawnEffect(AttackEffectData effectData, Transform target = null)
            {
                if (effectData == null) return;

                // 타겟이 null이면 플레이어를 타겟으로
                if (target == null && playerTransform != null)
                    target = playerTransform;

                StartCoroutine(SpawnEffectCoroutine(effectData, target));
            }

            private IEnumerator SpawnEffectCoroutine(AttackEffectData effectData, Transform target)
            {
                // 스폰 전 딜레이
                if (effectData.delayBeforeSpawn > 0)
                    yield return new WaitForSeconds(effectData.delayBeforeSpawn);

                // 이펙트 가져오기
                GameObject effect = GetEffectFromPool(effectData.effectName);
                if (effect == null)
                {
                    Debug.LogWarning($"Effect '{effectData.effectName}' not found in pool!");
                    yield break;
                }

                // 위치 계산
                Vector3 spawnPosition = CalculateSpawnPosition(effectData, target);
                Quaternion spawnRotation = CalculateSpawnRotation(effectData, target, spawnPosition);

                // 이펙트 설정
                effect.transform.position = spawnPosition;
                effect.transform.rotation = spawnRotation;
                effect.transform.localScale = effectData.scale;

                // 이펙트 활성화
                effect.SetActive(true);

                // 타겟 추적이 필요한 경우
                if (effectData.followTarget && target != null)
                {
                    StartCoroutine(FollowTarget(effect, target, effectData.duration));
                }

                // 스케일 애니메이션이 있는 경우
                if (effectData.scaleCurve != null && effectData.scaleCurve.keys.Length > 0)
                {
                    StartCoroutine(AnimateScale(effect, effectData));
                }

                // 자동 비활성화
                if (effectData.autoDeactivate)
                {
                    yield return new WaitForSeconds(effectData.duration);
                    effect.SetActive(false);
                }
            }

            private Vector3 CalculateSpawnPosition(AttackEffectData effectData, Transform target)
            {
                Vector3 position = Vector3.zero;

                switch (effectData.spawnType)
                {
                    case EffectSpawnType.AtPlayer:
                        if (target != null)
                            position = target.position;
                        break;

                    case EffectSpawnType.AtFirePoint:
                        if (firePoint != null)
                            position = firePoint.position;
                        else
                            position = transform.position;
                        break;

                    case EffectSpawnType.InFrontOfEnemy:
                        Vector3 forward = transform.forward;
                        position = transform.position + forward * 2f;  // 2미터 앞
                        break;

                    case EffectSpawnType.AtTarget:
                        if (target != null)
                            position = target.position;
                        break;

                    case EffectSpawnType.BetweenEnemyAndPlayer:
                        if (target != null)
                            position = Vector3.Lerp(transform.position, target.position, 0.5f);
                        break;

                    case EffectSpawnType.AtEnemyPosition:
                        position = transform.position;
                        break;
                }

                // 오프셋 적용
                position += effectData.positionOffset;

                return position;
            }

            private Quaternion CalculateSpawnRotation(AttackEffectData effectData, Transform target, Vector3 spawnPos)
            {
                Quaternion rotation = Quaternion.identity;

                // 타겟을 바라보는 경우
                if (effectData.lookAtTarget && target != null)
                {
                    Vector3 direction = (target.position - spawnPos).normalized;
                    if (direction != Vector3.zero)
                        rotation = Quaternion.LookRotation(direction);
                }
                else
                {
                    // 기본 회전은 Enemy의 회전
                    rotation = transform.rotation;
                }

                // 오프셋 적용
                rotation *= Quaternion.Euler(effectData.rotationOffset);

                return rotation;
            }

            private GameObject GetEffectFromPool(string effectName)
            {
                if (effectDictionary.ContainsKey(effectName))
                    return effectDictionary[effectName];

                // 이름이 정확히 일치하지 않으면 포함된 이름으로 검색
                foreach (var kvp in effectDictionary)
                {
                    if (kvp.Key.Contains(effectName) || effectName.Contains(kvp.Key))
                        return kvp.Value;
                }

                return null;
            }

            private IEnumerator FollowTarget(GameObject effect, Transform target, float duration)
            {
                float elapsed = 0f;
                while (elapsed < duration && target != null && effect.activeSelf)
                {
                    effect.transform.position = target.position;
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            private IEnumerator AnimateScale(GameObject effect, AttackEffectData effectData)
            {
                float elapsed = 0f;
                Vector3 originalScale = effectData.scale;

                while (elapsed < effectData.duration && effect.activeSelf)
                {
                    float normalizedTime = elapsed / effectData.duration;
                    float scaleMultiplier = effectData.scaleCurve.Evaluate(normalizedTime);
                    effect.transform.localScale = originalScale * scaleMultiplier;

                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            /// <summary>
            /// 모든 이펙트 즉시 비활성화
            /// </summary>
            public void DeactivateAllEffects()
            {
                foreach (var effect in effectDictionary.Values)
                {
                    if (effect != null)
                        effect.SetActive(false);
                }
            }
        }
    }
