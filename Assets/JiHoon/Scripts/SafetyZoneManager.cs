using Tower.Game;
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

    void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitSafetyZone);
        }
    }

    void Update()
    {
        // ESC 메뉴
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        if (menuUI != null)
        {
            menuUI.SetActive(!menuUI.activeSelf);
            Time.timeScale = menuUI.activeSelf ? 0f : 1f;
        }
    }

    public void ExitSafetyZone()
    {
        Debug.Log("Exiting SafetyZone to next stage");

        // 플레이어 찾기
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

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

        // CharacterController 처리 (중요!)
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;  // 비활성화
        }

        // 플레이어 이동
        player.transform.position = exitPoint.position;
        player.transform.rotation = exitPoint.rotation;
        Debug.Log($"Moved player to: {exitPoint.position}");

        // CharacterController 다시 활성화
        if (controller != null)
        {
            controller.enabled = true;
        }

        // PlayerMovement 활성화
        var playerMovement = player.GetComponent<Sample.PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
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
    }
}