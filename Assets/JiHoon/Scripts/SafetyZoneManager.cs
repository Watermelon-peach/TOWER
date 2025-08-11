using Tower.Game;
using Tower.Player;
using UnityEngine;
using UnityEngine.UI;

public class SafetyZoneManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int nextMapIndex = 5; // 다음 맵 인덱스
    [SerializeField] private Transform exitPoint; // 나가기 위치

    [Header("UI")]
    [SerializeField] private GameObject menuUI;
    [SerializeField] private Button exitButton; // 나가기 버튼

    [Header("Zone Detection")]
    [SerializeField] private Collider safetyZoneTrigger; // SafetyZone 영역 콜라이더

    private bool isPlayerInZone = false; // 플레이어가 존에 있는지 체크

    void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitSafetyZone);
        }

        // Trigger 콜라이더 자동 찾기 (없으면)
        if (safetyZoneTrigger == null)
        {
            safetyZoneTrigger = GetComponent<Collider>();
            if (safetyZoneTrigger != null)
            {
                safetyZoneTrigger.isTrigger = true;
            }
        }

        // 시작 시 UI 숨기기
        if (menuUI != null)
        {
            menuUI.SetActive(false);
        }
    }

    void Update()
    {
        // SafetyZone에 있을 때만 ESC 메뉴 동작
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    // 플레이어가 SafetyZone에 들어왔을 때
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            Debug.Log("[SafetyZone] Player entered SafetyZone");
        }
    }

    // 플레이어가 SafetyZone을 나갔을 때
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;

            // 존을 나가면 메뉴 강제 닫기
            if (menuUI != null && menuUI.activeSelf)
            {
                menuUI.SetActive(false);
                Time.timeScale = 1f;
            }

            Debug.Log("[SafetyZone] Player left SafetyZone");
        }
    }

    void ToggleMenu()
    {
        if (menuUI != null)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            menuUI.SetActive(!menuUI.activeSelf);
            Time.timeScale = menuUI.activeSelf ? 0f : 1f;
        }
    }

    public void ExitSafetyZone()
    {
        Debug.Log("Exiting SafetyZone to next stage");

        // 타이머 시작
        if (StageTimer.Instance != null)
        {
            StageTimer.Instance.StartStageTimer(nextMapIndex);
            Debug.Log($"[SafetyZoneManager] Timer started for Stage {nextMapIndex}");
        }

        if (exitPoint == null)
        {
            Debug.LogError("Exit Point not set!");
            return;
        }

        // ⭐ TeamManager로 3명 모두 이동
        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.MoveFormation(exitPoint.position, exitPoint.rotation);
            Debug.Log("Moved all 3 characters using TeamManager");
        }
        else
        {
            // TeamManager 없으면 모든 플레이어 개별 이동
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

            foreach (var player in allPlayers)
            {
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }

                player.transform.position = exitPoint.position;
                player.transform.rotation = exitPoint.rotation;

                if (controller != null)
                {
                    controller.enabled = true;
                }

                Debug.Log($"Moved {player.name} to exit point");
            }

            // 첫 번째 플레이어의 Movement만 활성화
            if (allPlayers.Length > 0)
            {
                var playerMovement = allPlayers[0].GetComponent<Sample.PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.enabled = true;
                }
            }
        }

        // 다음 맵 시작
        if (MapSpawnManager.Instance != null)
        {
            MapSpawnManager.Instance.StartMap(nextMapIndex);
        }

        // CheckpointManager 업데이트
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.OnStageEnter(5);
        }

        // 메뉴 닫기
        if (menuUI != null)
        {
            menuUI.SetActive(false);
            Time.timeScale = 1f;
        }

        // ⭐ 마우스 커서 숨기기
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        this.enabled = false;

    }
}