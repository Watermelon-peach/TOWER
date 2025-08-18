using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "VFX/Attack VFX Table", fileName = "AttackVFXTable")]
public class AttackVFXTable : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        [Tooltip("Animator 상태 이름 (예: Attack1, Attack2, AreaAttack1 등)")]
        public string stateName;

        [Header("VFX 프리팹 (둘 중 하나 또는 둘 다)")]
        public VisualEffect vfxPrefab;
        public ParticleSystem psPrefab;

        [Header("스폰 옵션")]
        [Tooltip("Animator 오브젝트 기준 자식 경로 (예: Armature/RightHand/WeaponTip)")]
        public string spawnPath = "";
        public Vector3 localPositionOffset;
        public Vector3 localEulerOffset;
        public bool parentToSpawn = true;

        [Header("정리 옵션")]
        [Tooltip("상태 종료 후 파티클 잔상 유지 시간")]
        public float destroyDelay = 0.15f;
    }

    public Entry[] entries;
}
