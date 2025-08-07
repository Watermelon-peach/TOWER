using Tower.Game;
using Tower.UI;
using UnityEngine;
using System.Collections;

public class MapExit : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int currentMapID = 0;
    [SerializeField] private Transform nextMapStartTransform;

    private bool isTransitioning = false;
    private bool isActive = false;
    private MapSpawnArea mapArea;
    private GameObject currentPlayer;
    private bool canCheck = false; // 체크 가능 여부

    private void Awake()
    {
        // 첫 번째 맵인 경우 타이머 시작
        if (currentMapID == 0)
        {
            if (CardRewardUI.Instance != null)
            {
                CardRewardUI.Instance.OnMapStart(0);
                Debug.Log($"[MapExit] First map (ID: 0) timer started in Awake");
            }

            if (StageTimer.Instance != null)
            {
                StageTimer.Instance.StartStageTimer(0);
                Debug.Log($"[MapExit] Stage timer started for first map");
            }
        }

        // 모든 Exit는 초기에 비활성화
        SetTriggerActive(false);
    }

    void Start()
    {
        // 이 맵의 MapSpawnArea 찾기
        MapSpawnArea[] areas = FindObjectsOfType<MapSpawnArea>();
        foreach (var area in areas)
        {
            if (area.mapID == currentMapID)
            {
                mapArea = area;
                Debug.Log($"[MapExit] Found MapSpawnArea for map {currentMapID}");
                break;
            }
        }

        // mapArea를 못 찾은 경우 경고
        if (mapArea == null)
        {
            Debug.LogWarning($"[MapExit] No MapSpawnArea found for map {currentMapID}!");
        }

        // 첫 프레임에 체크하지 않도록 딜레이 추가
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        // 2초 대기 (몬스터 스폰 시간 확보)
        yield return new WaitForSeconds(2f);
        canCheck = true;
        Debug.Log($"[MapExit] Map {currentMapID} exit check enabled");
    }

    void Update()
    {
        if (!isActive && mapArea != null)
        {
            if (isTransitioning) return;
            if (Time.time < 1f) return;

            // canCheck 추가!
            if (!canCheck) return;  // ← 이것만 추가하면 됨

            // SafetyZone 체크 (spawnConfig이 없으면 SafetyZone)
            if (mapArea.spawnConfig == null)
            {
                Debug.Log($"[MapExit] SafetyZone exit activated after delay");
            }
            
            else
            {
                // 일반 맵은 몬스터 체크
                if (mapArea.GetActiveMonsterCount() == 0 && mapArea.IsAllMonstersSpawned())
                {
                    SetTriggerActive(true);
                }
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

            // Config이 없는 맵은 보상 없이 바로 이동
            if (mapArea != null && mapArea.spawnConfig == null)
            {
                HandleDirectTransition();
            }
            else
            {
                // 일반 맵은 보상 후 이동
                StartCoroutine(HandleRewardAndTransition());
            }
        }
    }

    // 보상 없이 바로 이동 (SafetyZone 등)
    void HandleDirectTransition()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        isActive = false;

        Debug.Log($"[MapExit] Direct transition from map {currentMapID} to {currentMapID + 1}");

        // 다음 맵으로 이동
        MovePlayerToNextMap();

        isTransitioning = false;
    }

    // 일반 맵 전환 처리 (보상 포함)
    IEnumerator HandleRewardAndTransition()
    {
        isTransitioning = true;
        isActive = false;
        GetComponent<Collider>().enabled = false;

        // 타이머 정지
        if (StageTimer.Instance != null)
        {
            StageTimer.Instance.StopTimer();
            Debug.Log($"[MapExit] Timer stopped for map {currentMapID}");
        }

        // 플레이어 이동 정지
        var playerMovement = currentPlayer.GetComponent<Sample.PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;

        // 보상 UI 표시
        bool rewardCompleted = false;

        if (CardRewardUI.Instance != null)
        {
            Debug.Log("Showing stage reward UI");
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
        yield return new WaitForSeconds(0.5f);

        // 플레이어 이동 및 다음 맵 시작
        MovePlayerToNextMap();
        isTransitioning = false;
    }

    void MovePlayerToNextMap()
    {
        Debug.Log($"MovePlayerToNextMap - Current MapID: {currentMapID}, Moving to next map");

        // 플레이어 이동 (이건 항상 실행)
        if (nextMapStartTransform != null && currentPlayer != null)
        {
            CharacterController controller = currentPlayer.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            currentPlayer.transform.position = nextMapStartTransform.position;
            currentPlayer.transform.rotation = nextMapStartTransform.rotation;
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

        // SafetyZone 체크
        var nextIndex = currentMapID + 1;
        if (MapSpawnManager.Instance != null &&
            nextIndex < MapSpawnManager.Instance.mapSpawnAreas.Length)
        {
            var nextArea = MapSpawnManager.Instance.mapSpawnAreas[nextIndex];

            // SafetyZone이면 타이머, 스폰 등 스킵
            if (nextArea != null && nextArea.spawnConfig == null)
            {
                Debug.Log($"[MapExit] Entering SafetyZone - skipping map initialization");
                return; // 여기서 종료
            }
        }

        // === 아래는 일반 맵일 때만 실행 ===

        // 다음 맵 타이머 시작
        if (StageTimer.Instance != null)
        {
            StageTimer.Instance.OnNextMap(currentMapID + 1);
            Debug.Log($"[MapExit] Timer started for next map {currentMapID + 1}");
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
    }
}