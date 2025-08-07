using UnityEngine;

namespace Tower.Game
{
    public class SafetyZone : MonoBehaviour
    {
        [Header("Safety Zone Settings")]
        [SerializeField] public int zoneNumber = 1; // 몇 번째 Safety Zone (1, 2, 3...)
        [SerializeField] private int stageNumber = 4; 
        [SerializeField] private Transform checkpointPosition; // 리스폰 위치 (없으면 자기 위치)

        private bool checkpointSaved = false;

        void Start()
        {
            // 체크포인트 위치 없으면 현재 위치 사용
            if (checkpointPosition == null)
            {
                checkpointPosition = transform;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !checkpointSaved)
            {
                Debug.Log($"Safety Zone {zoneNumber} 도달! (Stage {stageNumber})");

                // CheckpointManager에 저장
                if (CheckpointManager.Instance != null)
                {
                    CheckpointManager.Instance.OnStageEnter(stageNumber);
                    CheckpointManager.Instance.UpdateCheckpoint(checkpointPosition.position);
                    checkpointSaved = true;
                }
            }
        }

        // 디버그용 - Scene 뷰에서 영역 표시
        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2);

            if (checkpointPosition != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(checkpointPosition.position, 0.5f);
            }
        }
    }
}