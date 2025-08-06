using UnityEngine;
using System.Collections;
using Tower.Game;
using Tower.UI;

namespace Tower.Game
{
    // 맵 클리어 시 활성화되는 출구 트리거 (보상 표시 포함)
    public class MapExit : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private int currentMapID = 0;
        [SerializeField] private Transform nextMapStartTransform;

        private bool isTransitioning = false;
        private bool isActive = false;
        private MapSpawnArea mapArea;
        private GameObject currentPlayer; // 플레이어 참조 저장


        private void Awake()
        {
            // 첫 번째 맵인 경우 즉시 시간 기록
            if (currentMapID == 0 && CardRewardUI.Instance != null)
            {
                CardRewardUI.Instance.OnMapStart(0);
                Debug.Log($"[MapExit] First map (ID: 0) timer started in Awake");
            }

            // 초기에는 트리거 비활성화
            SetTriggerActive(false);
        }

        void Start()
        {
            // 이 맵의 시작 시간 즉시 기록 (MapSpawnManager를 기다리지 않음)
            if (CardRewardUI.Instance != null)
            {
                CardRewardUI.Instance.OnMapStart(currentMapID);
                Debug.Log($"[MapExit] Map {currentMapID} timer started directly in Start()");
            }

            // 이 맵의 MapSpawnArea 찾기
            MapSpawnArea[] areas = FindObjectsOfType<MapSpawnArea>();
            foreach (var area in areas)
            {
                if (area.mapID == currentMapID)
                {
                    mapArea = area;
                    break;
                }
            }
        }

        void Update()
        {
            // 맵이 클리어되었는지 체크
            if (!isActive && mapArea != null)
            {
                // 이미 사용된 출구는 다시 활성화하지 않음
                if (isTransitioning) return;

                if (mapArea.GetActiveMonsterCount() == 0 && mapArea.IsAllMonstersSpawned())
                {
                    SetTriggerActive(true);
                }
            }
        }

        void SetTriggerActive(bool active)
        {
            isActive = active;

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = active;
            }

            Debug.Log($"Map {currentMapID} exit trigger is now {(active ? "ACTIVE" : "INACTIVE")}!");
        }

        void OnTriggerEnter(Collider other)
        {
            if (!isActive || isTransitioning) return;

            if (other.CompareTag("Player"))
            {
                Debug.Log($"Player entered exit trigger for map {currentMapID}!");
                currentPlayer = other.gameObject;
                StartCoroutine(HandleRewardAndTransition());
            }
        }

        IEnumerator HandleRewardAndTransition()
        {
            // 중복 실행 방지
            isTransitioning = true;
            isActive = false;
            GetComponent<Collider>().enabled = false;

            // 플레이어 이동 정지
            var playerMovement = currentPlayer.GetComponent<Sample.PlayerMovement>();
            if (playerMovement != null)
                playerMovement.enabled = false;

            // 보상 UI 표시
            bool rewardCompleted = false;

            if (CardRewardUI.Instance != null)
            {
                Debug.Log("Showing stage reward UI");
                // 맵 ID를 전달하여 맵별로 다른 보상 가능
                CardRewardUI.Instance.ShowReward(currentMapID, () =>
                {
                    rewardCompleted = true;
                });
            }
            else
            {
                Debug.LogWarning("No reward UI found! Skipping reward.");
                rewardCompleted = true;
            }

            // 보상 UI가 닫힐 때까지 대기
            while (!rewardCompleted)
            {
                yield return null;
            }

            Debug.Log("Reward UI closed!");

            // 잠시 대기
            yield return new WaitForSeconds(0.5f);

            // 플레이어 이동 및 다음 맵 시작
            MovePlayerToNextMap();

            // 전환 완료
            isTransitioning = false;
        }

        void MovePlayerToNextMap()
        {
            Debug.Log($"MovePlayerToNextMap - Current MapID: {currentMapID}, Moving to next map");

            if (nextMapStartTransform != null && currentPlayer != null)
            {
                // CharacterController 처리
                CharacterController controller = currentPlayer.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }

                // 위치 이동
                currentPlayer.transform.position = nextMapStartTransform.position;
                currentPlayer.transform.rotation = nextMapStartTransform.rotation;

                // 컴포넌트 재활성화
                if (controller != null)
                {
                    controller.enabled = true;
                }

                var playerMovement = currentPlayer.GetComponent<Sample.PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.enabled = true;
                }

                Debug.Log($"Moved player to: {currentPlayer.transform.position}");
            }
            else
            {
                Debug.LogError($"Missing references - nextMapStartTransform: {nextMapStartTransform}, currentPlayer: {currentPlayer}");
            }

            // 다음 맵 시작 시간 기록
            if (CardRewardUI.Instance != null)
            {
                CardRewardUI.Instance.OnMapStart(currentMapID + 1);
            }

            // 다음 맵 몬스터 스폰 시작
            if (MapSpawnManager.Instance != null)
            {
                Debug.Log($"Starting next map (index: {currentMapID + 1})");
                MapSpawnManager.Instance.StartMap(currentMapID + 1);
            }
            else
            {
                Debug.LogError("MapSpawnManager.Instance is null!");
            }
        }
    }
}