using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Tower.Game
{
    public class StageTimer : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] private float timeLimit = 180f; // 3분 (180초)
        [SerializeField] private bool isTimerActive = false;

        [Header("UI References")]
        [SerializeField] private GameObject timerPanel;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFillBar; // 시간 바 (옵션)
        [SerializeField] private GameObject warningEffect; // 시간 부족 경고 효과

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private Button retryButton;

        [Header("Warning Settings")]
        [SerializeField] private float warningTime = 30f; // 30초 남으면 경고
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color dangerColor = Color.red;

        [Header("Checkpoint Settings")]
        [SerializeField] private int floorsPerMast = 4; // 한 마스트당 층 수
        [SerializeField] private Transform[] safetyZoneSpawnPoints; // 세이프티존 스폰 위치들

        private float currentTime;
        private int currentMapID;
        private bool isGameOver = false;
        private Coroutine timerCoroutine;
        private int lastCheckpointMap = 0; // 마지막 체크포인트 (세이프티존) 맵 ID

        // 싱글톤
        private static StageTimer instance;
        public static StageTimer Instance => instance;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 초기 설정
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);
        }

        public void StartStageTimer(int mapID)
        {
            if (isGameOver) return;

            currentMapID = mapID;
            currentTime = timeLimit;  // 항상 리셋

            // SafetyZone 체크 (MapSpawnManager로 확인)
            bool isSafetyZone = false;
            if (MapSpawnManager.Instance != null &&
                mapID < MapSpawnManager.Instance.mapSpawnAreas.Length)
            {
                var area = MapSpawnManager.Instance.mapSpawnAreas[mapID];
                if (area != null && area.spawnConfig == null)
                {
                    isSafetyZone = true;
                }
            }

            // SafetyZone이면 타이머 정지, 아니면 시작
            isTimerActive = !isSafetyZone;

            Debug.Log($"[StageTimer] Stage {mapID} timer - Time reset to {timeLimit}s, Active: {isTimerActive}");

            // UI는 항상 활성화 (시간은 보이되 안 감)
            if (timerPanel != null)
                timerPanel.SetActive(true);

            // 기존 코루틴 중지하고 재시작
            if (timerCoroutine != null)
                StopCoroutine(timerCoroutine);

            timerCoroutine = StartCoroutine(TimerCountdown());
        }

        // 세이프티존 도달 시 체크포인트 저장
        public void SetCheckpoint(int mapID)
        {
            lastCheckpointMap = mapID;
            Debug.Log($"[StageTimer] Checkpoint saved at map {mapID}");

            // 세이프티존에서는 타이머 정지 (선택사항)
            // PauseTimer();
        }

        // 타이머 일시정지
        public void PauseTimer()
        {
            isTimerActive = false;
        }

        // 타이머 재개
        public void ResumeTimer()
        {
            if (!isGameOver)
                isTimerActive = true;
        }

        // 스테이지 클리어 시 타이머 정지
        public void StopTimer()
        {
            isTimerActive = false;

            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            Debug.Log($"[StageTimer] Stage {currentMapID} timer stopped");
        }

        // 타이머 카운트다운
        IEnumerator TimerCountdown()
        {
            while (currentTime > 0 && !isGameOver)
            {
                if (isTimerActive)
                {
                    currentTime -= Time.deltaTime;
                    UpdateTimerUI();

                    // 경고 체크
                    CheckWarning();

                    if (currentTime <= 0)
                    {
                        currentTime = 0;
                        TimeUp();
                    }
                }

                yield return null;
            }
        }

        // 타이머 UI 업데이트
        void UpdateTimerUI()
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60);
                int seconds = Mathf.FloorToInt(currentTime % 60);
                timerText.text = $"{minutes:00}:{seconds:00}";

                // 색상 변경
                if (currentTime <= 10f)
                {
                    timerText.color = dangerColor;
                    // 깜빡임 효과
                    timerText.color = Color.Lerp(dangerColor, Color.white, Mathf.PingPong(Time.time * 2, 1));
                }
                else if (currentTime <= warningTime)
                {
                    timerText.color = warningColor;
                }
                else
                {
                    timerText.color = normalColor;
                }
            }

            // 타이머 바 업데이트 (있는 경우)
            if (timerFillBar != null)
            {
                timerFillBar.fillAmount = currentTime / timeLimit;

                // 바 색상도 변경
                if (currentTime <= 10f)
                    timerFillBar.color = dangerColor;
                else if (currentTime <= warningTime)
                    timerFillBar.color = warningColor;
                else
                    timerFillBar.color = normalColor;
            }
        }

        // 경고 체크
        void CheckWarning()
        {
            if (warningEffect != null)
            {
                if (currentTime <= warningTime && currentTime > 10f)
                {
                    if (!warningEffect.activeSelf)
                        warningEffect.SetActive(true);
                }
                else if (currentTime <= 10f)
                {
                    // 10초 이하일 때 더 강한 경고
                    warningEffect.SetActive(true);
                    // 깜빡임 효과 등 추가 가능
                }
            }
        }

        // 시간 초과
        void TimeUp()
        {
            isGameOver = true;
            isTimerActive = false;

            Debug.Log($"[StageTimer] TIME UP! Game Over at stage {currentMapID}");

            // 게임 정지
            Time.timeScale = 0f;

            // 게임오버 UI 표시
            ShowGameOverUI();
        }

        // 게임오버 UI 표시
        void ShowGameOverUI()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (gameOverText != null)
                {
                    // 체크포인트 계산
                    int checkpointMap = GetCheckpointForMap(currentMapID);
                    string respawnLocation = checkpointMap == 0 ? "1층" : $"세이프티존 (마스트 {checkpointMap / floorsPerMast} 클리어 후)";

                    gameOverText.text = $"시간 초과!\n{currentMapID + 1}층에서 실패\n\n{respawnLocation}으로 돌아갑니다";
                }
            }

            // 5초 후 자동으로 리셋 (또는 버튼 클릭)
            StartCoroutine(AutoResetGame());
        }

        // 자동 리셋
        IEnumerator AutoResetGame()
        {
            yield return new WaitForSecondsRealtime(5f);
            ResetToCheckpoint();
        }

        // 재시도 버튼 클릭
        void OnRetryClicked()
        {
            StopAllCoroutines();
            ResetToCheckpoint();
        }

        // 현재 맵에 대한 체크포인트 계산
        int GetCheckpointForMap(int mapID)
        {
            // mapID가 0-3이면 체크포인트는 0 (1층으로)
            // mapID가 4-7이면 체크포인트는 4 (첫 번째 세이프티존)
            // mapID가 8-11이면 체크포인트는 8 (두 번째 세이프티존)

            if (mapID < floorsPerMast)
            {
                return 0; // 첫 마스트는 1층으로
            }
            else
            {
                // 이전 마스트의 끝 (세이프티존)
                return ((mapID / floorsPerMast) * floorsPerMast);
            }
        }

        // 체크포인트로 리셋
        public void ResetToCheckpoint()
        {
            Debug.Log("[StageTimer] Resetting to checkpoint...");

            // 시간 복구
            Time.timeScale = 1f;

            // 체크포인트 계산
            int checkpointMap = GetCheckpointForMap(currentMapID);

            // 게임 상태 초기화
            isGameOver = false;
            currentTime = timeLimit;

            // UI 초기화
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            // 첫 마스트에서 실패한 경우 점수 초기화
            if (checkpointMap == 0)
            {
                // 점수 초기화
                if (Tower.UI.CardRewardUI.Instance != null)
                {
                    Tower.UI.CardRewardUI.Instance.ResetTotalScore();
                }

                ResetToFirstMap();
            }
            else
            {
                // 세이프티존으로 이동
                ResetToSafetyZone(checkpointMap);
            }
        }

        // 1층으로 맵 리셋
        void ResetToFirstMap()
        {
            Debug.Log("[StageTimer] Resetting to floor 1...");

            // MapSpawnManager 리셋
            if (MapSpawnManager.Instance != null)
            {
                MapSpawnManager.Instance.ResetGame();
            }

            // 플레이어 위치 리셋 (1층 시작 위치로)
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // 1층 시작 위치 찾기
                GameObject startPoint = GameObject.Find("StartPoint00");
                if (startPoint != null)
                {
                    MovePlayerToPosition(player, startPoint.transform);
                }
            }

            // 1층 타이머 재시작
            StartStageTimer(0);
        }

        // 세이프티존으로 리셋
        void ResetToSafetyZone(int checkpointMap)
        {
            Debug.Log($"[StageTimer] Resetting to safety zone after map {checkpointMap - 1}...");

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // 세이프티존 스폰 포인트 찾기
                int safetyZoneIndex = (checkpointMap / floorsPerMast) - 1;

                // 배열에서 세이프티존 스폰 포인트 사용
                if (safetyZoneSpawnPoints != null && safetyZoneIndex < safetyZoneSpawnPoints.Length)
                {
                    MovePlayerToPosition(player, safetyZoneSpawnPoints[safetyZoneIndex]);
                }
                else
                {
                    // 대체: 이름으로 찾기
                    GameObject safetyZonePoint = GameObject.Find($"SafetyZone_{safetyZoneIndex}_Spawn");
                    if (safetyZonePoint != null)
                    {
                        MovePlayerToPosition(player, safetyZonePoint.transform);
                    }
                }

                
            }

            
        }

        // 플레이어 위치 이동 헬퍼 메소드
        void MovePlayerToPosition(GameObject player, Transform targetTransform)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = false;

            player.transform.position = targetTransform.position;
            player.transform.rotation = targetTransform.rotation;

            if (controller != null)
                controller.enabled = true;

            // 플레이어 이동 컴포넌트 재활성화
            var playerMovement = player.GetComponent<Sample.PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }

            Debug.Log($"[StageTimer] Player moved to position: {targetTransform.position}");
        }

        // 다음 맵으로 이동 시 타이머 리셋
        public void OnNextMap(int newMapID)
        {
            StopTimer();

            // 세이프티존 체크 (4의 배수 = 세이프티존)
            if (newMapID % floorsPerMast == 0 && newMapID > 0)
            {
                SetCheckpoint(newMapID);
                // 세이프티존에서는 타이머를 시작하지 않음 (선택사항)
                // return;
            }

            StartStageTimer(newMapID);
        }

        // 남은 시간 반환
        public float GetRemainingTime()
        {
            return currentTime;
        }

        // 시간 추가 (아이템 등으로)
        public void AddTime(float bonusTime)
        {
            currentTime = Mathf.Min(currentTime + bonusTime, timeLimit);
            Debug.Log($"[StageTimer] Added {bonusTime} seconds. New time: {currentTime}");
        }
    }
}