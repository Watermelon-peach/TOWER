using UnityEngine;

namespace Tower.Effects
{
    // 데미지 타입
    public enum DamageType
    {
        None,       // 데미지 없음
        Collision,  // 충돌 시 한 번
        Area        // 지속 데미지
    }

    [CreateAssetMenu(fileName = "AttackEffectData", menuName = "EnemySkillEffects/AttackEffectData")]
    public class AttackEffectData : ScriptableObject
    {
        [Header("Effect Settings")]
        public string effectName;                  // 이펙트 이름 (풀에서 찾을 때 사용)
        public EffectSpawnType spawnType;         // 스폰 위치 타입

        [Header("Transform Settings")]
        public Vector3 positionOffset = Vector3.zero;  // 위치 오프셋
        public Vector3 rotationOffset = Vector3.zero;  // 회전 오프셋
        public Vector3 scale = Vector3.one;            // 스케일

        [Header("Behavior Settings")]
        public bool lookAtTarget = false;          // 타겟을 바라볼지
        public bool followTarget = false;          // 타겟을 따라갈지
        public float duration = 2f;                // 지속 시간
        public float delayBeforeSpawn = 0f;        // 스폰 전 딜레이

        [Header("Follow Settings")]
        [Tooltip("타겟을 따라가는 속도 (followTarget이 true일 때만)")]
        public float followSpeed = 3f;             // 추적 속도

        [Tooltip("부드러운 이동 (Lerp) vs 일정 속도 (MoveTowards)")]
        public bool smoothFollow = true;           // 부드러운 추적 여부

        [Tooltip("추적 시작 전 대기 시간")]
        public float followDelay = 0f;             // 추적 시작 딜레이

        [Tooltip("최대 추적 거리")]
        public float maxFollowDistance = 30f;      // 최대 추적 거리

        [Tooltip("정지 거리 (이 거리 내에서는 멈춤)")]
        public float stopDistance = 1f;            // 정지 거리

        [Header("=== Damage Settings ===")]
        [Tooltip("데미지 타입")]
        public DamageType damageType = DamageType.None;

        [Tooltip("데미지 양")]
        public float damage = 10f;

        [Tooltip("데미지 간격 (Area 타입일 때)")]
        public float damageInterval = 1f;  // 1초마다


        [Header("Optional")]
        public AnimationCurve scaleCurve;          // 시간에 따른 스케일 변화
        public bool autoDeactivate = true;         // 자동으로 비활성화할지
    }
}