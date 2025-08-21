using Tower.Game;
using Tower.UI;
using UnityEngine;
using System.Collections;
using Tower.Player;

public class MapExit : MonoBehaviour
{
    public enum ExitType
    {
        Normal,           // 일반 층 → 층 (1→2, 2→3 등)
        ToSafetyZone,    // 마스트 마지막 층 → SafetyZone
    }


    [Header("Map Settings")]
    [SerializeField] private int currentMapID = 0;
    [SerializeField] private ExitType exitType = ExitType.Normal;
    [SerializeField] private Transform nextMapStartTransform;

    [Header("State")]
    private bool isTransitioning = false;
    private bool isActive = false;
    private bool canCheck = false;
    private MapSpawnArea mapArea;
    private GameObject currentPlayer;

    public int CurrentMapID => currentMapID;

    // ==================== 초기화 ====================

    void Awake()
    {
        SetTriggerActive(false);
    }

    void Start()
    {
        if (exitType == ExitType.ToSafetyZone && nextMapStartTransform == null)
        {
            FindSafetyZoneStartPoint();
        }

        FindMapSpawnArea();
        StartCoroutine(DelayedStart());
    }


    void FindSafetyZoneStartPoint()
    {
        // Scene에서 SafetyZone 찾기
        GameObject safetyZone = GameObject.Find("SafetyZone");
        if (safetyZone != null)
        {
            // StartPoint 찾기
            Transform startPoint = safetyZone.transform.Find("SafetyStartPoint");
            if (startPoint == null)
            {
                // StartPoint가 없으면 SafetyZone 자체 위치 사용
                startPoint = safetyZone.transform;
            }

            nextMapStartTransform = startPoint;
            Debug.Log($"[MapExit] Auto-connected to SafetyZone StartPoint for map {currentMapID}");
        }
        else
        {
            Debug.LogWarning($"[MapExit] SafetyZone not found in scene for map {currentMapID}!");
        }
    }

    void FindMapSpawnArea()
    {
        MapSpawnArea[] areas = FindObjectsByType<MapSpawnArea>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var area in areas)
        {
            if (area.mapID == currentMapID)
            {
                mapArea = area;
                Debug.Log($"[MapExit] Found MapSpawnArea for map {currentMapID}");
                return;
            }
        }
        Debug.LogWarning($"[MapExit] No MapSpawnArea found for map {currentMapID}!");
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2f);
        canCheck = true;
        Debug.Log($"[MapExit] Map {currentMapID} exit check enabled");
    }

    // ==================== 출구 활성화 ====================

    public void ActivateExit()
    {
        if (!isActive && mapArea != null && canCheck && !isTransitioning)
        {
            // SafetyZone이거나 몬스터 모두 처치 시 활성화
            if (mapArea.spawnConfig == null ||
                (mapArea.GetActiveMonsterCount() == 0 && mapArea.IsAllMonstersSpawned()))
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
        Debug.Log($"[MapExit] Map {currentMapID} exit is now {(active ? "ACTIVE" : "INACTIVE")}!");
    }

    // ==================== 플레이어 진입 처리 ====================

    void OnTriggerEnter(Collider other)
    {
        if (!isActive || isTransitioning) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log($"[MapExit] Player entered exit for map {currentMapID} (Type: {exitType})");
            currentPlayer = other.gameObject;

            // SafetyZone이나 FromSafetyZone은 보상 없음
            if (mapArea?.spawnConfig == null)
            {
                HandleDirectTransition();
            }
            else
            {
                StartCoroutine(HandleRewardAndTransition());
            }
        }
    }

    // ==================== 전환 처리 ====================

    // 보상 없이 바로 이동
    void HandleDirectTransition()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        isActive = false;

        MoveToNextArea();

        isTransitioning = false;
    }

    // 보상 후 이동
    IEnumerator HandleRewardAndTransition()
    {
        isTransitioning = true;
        isActive = false;
        GetComponent<Collider>().enabled = false;

        // 타이머 정지
        StageTimer.Instance?.StopTimer();

        // 플레이어 이동 정지
        var playerMovement = currentPlayer?.GetComponent<Sample.PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;

        // 커서 표시
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 보상 UI 처리
        bool rewardCompleted = false;
        if (CardRewardUI.Instance != null)
        {
            Debug.Log("[MapExit] Showing reward UI");
            CardRewardUI.Instance.ShowReward(currentMapID, () => rewardCompleted = true);
        }
        else
        {
            rewardCompleted = true;
        }

        // 보상 완료 대기
        while (!rewardCompleted)
            yield return null;

        yield return new WaitForSeconds(2.5f);

        // 커서 숨김
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 이동 처리
        MoveToNextArea();
        isTransitioning = false;
    }

    // ==================== 실제 이동 처리 ====================

    void MoveToNextArea()
    {
        Debug.Log($"[MapExit] Moving from map {currentMapID} (Type: {exitType})");

        switch (exitType)
        {
            
            case ExitType.ToSafetyZone:
                // 마지막 층 → SafetyZone
                HandleSafetyZoneTransition();
                break;

            case ExitType.Normal:
            default:
                // 일반 층 이동
                HandleNormalTransition();
                break;
        }
    }

    // 다음 마스트로 전환
    void HandleNextMastTransition()
    {
        Debug.Log("[MapExit] Loading next mast!");
        MastManager.Instance?.LoadNextMast();
    }

    // SafetyZone으로 이동
    void HandleSafetyZoneTransition()
    {
        Debug.Log("[MapExit] Moving to SafetyZone");

        if (nextMapStartTransform != null && TeamManager.Instance != null)
        {
            TeamManager.Instance.MoveFormation(
                nextMapStartTransform.position,
                nextMapStartTransform.rotation
            );
        }

        // 플레이어 이동 재활성화
        EnablePlayerMovement();
    }

    // 일반 층 이동
    void HandleNormalTransition()
    {
        Debug.Log($"[MapExit] Normal transition to map {currentMapID + 1}");

        // 팀 이동
        if (nextMapStartTransform != null && TeamManager.Instance != null)
        {
            TeamManager.Instance.MoveFormation(
                nextMapStartTransform.position,
                nextMapStartTransform.rotation
            );
        }

        // 플레이어 이동 재활성화
        EnablePlayerMovement();

        // 다음 맵 시작
        int nextMapIndex = currentMapID + 1;

        // 타이머 시작
        StageTimer.Instance?.OnNextMap(nextMapIndex);

        // 시간 기록
        CardRewardUI.Instance?.OnMapStart(nextMapIndex);

        // 몬스터 스폰
        MapSpawnManager.Instance?.StartMap(nextMapIndex);
    }

    // 플레이어 이동 활성화
    void EnablePlayerMovement()
    {
        if (currentPlayer != null)
        {
            var playerMovement = currentPlayer.GetComponent<Sample.PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
        }
    }
}