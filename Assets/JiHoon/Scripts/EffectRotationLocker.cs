using UnityEngine;

/// <summary>
/// 이펙트의 월드 좌표와 회전을 완전히 고정
/// 부모가 움직여도 이펙트는 제자리에 유지
/// </summary>
public class EffectRotationLocker : MonoBehaviour
{
    [Header("Speed Settings")]
    [Range(0.01f, 1f)]
    [SerializeField] private float followSpeed = 0.1f;  // 0.1 = 10% 속도로 따라감

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void OnEnable()
    {
        // 현재 상태 저장
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // 현재 위치/회전 (부모 때문에 변한 상태)
        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;

        // 천천히 따라가기 (Lerp로 속도 조절)
        transform.position = Vector3.Lerp(lastPosition, currentPos, followSpeed);
        transform.rotation = Quaternion.Lerp(lastRotation, currentRot, followSpeed);

        // 다음 프레임을 위해 저장
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }
}