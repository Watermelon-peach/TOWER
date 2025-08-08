using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Tower.Game
{
    public class StageTimer : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] private float timeLimit = 180f; // 3분 (180초)

        [Header("UI References")]
        [SerializeField] private GameObject timerPanel;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFillBar; // 시간 바 (옵션)

        [Header("Game Over UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton; // 메인 메뉴로

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color dangerColor = Color.red;
        [SerializeField] private float warningTime = 30f; // 30초 남으면 경고

        private Coroutine timerCoroutine;

        // 상태 변수
        private float currentTime;
        private int currentStageID;
        private bool isTimerActive = false;
        private bool isGameOver = false;

        // 기록용
        private float bestTime = 0f;
        private int bestStage = 0;

        // 싱글톤
        private static StageTimer instance;
        public static StageTimer Instance => instance;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // UI 초기 설정
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            // 저장된 기록 불러오기
            LoadBestRecord();
        }

        // 타이머 시작 (메인 메서드)
        public void StartStageTimer(int stageID)
        {
            if (isGameOver) return;

            currentStageID = stageID;
            currentTime = timeLimit;
            isTimerActive = true;

            Debug.Log($"[Timer] Stage {stageID} started - Timer: {timeLimit}s");

            // 기존 코루틴 중지
            if (timerCoroutine != null)
                StopCoroutine(timerCoroutine);

            // 새 코루틴 시작
            timerCoroutine = StartCoroutine(TimerCountdown());

            // UI 활성화
            if (timerPanel != null)
                timerPanel.SetActive(true);
        }

        // 타이머 카운트다운 코루틴
        IEnumerator TimerCountdown()
        {
            while (currentTime > 0 && !isGameOver)
            {
                if (isTimerActive)
                {
                    currentTime -= Time.deltaTime;
                    UpdateTimerUI();

                    if (currentTime <= 0)
                    {
                        currentTime = 0;
                        OnTimeOver();
                    }
                }

                yield return null; // 다음 프레임까지 대기
            }
        }

        // 타이머 정지 (스테이지 클리어 시)
        public void StopTimer()
        {
            isTimerActive = false;

            // 코루틴 중지
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            // 기록 갱신 체크
            float clearTime = timeLimit - currentTime;
            UpdateBestRecord(currentStageID, clearTime);

            Debug.Log($"[Timer] Stage {currentStageID} cleared in {clearTime:F1}s");
        }

        // 타이머 일시정지 (SafetyZone 등)
        public void PauseTimer()
        {
            isTimerActive = false;
            Debug.Log($"[Timer] Paused at Stage {currentStageID}");
        }

        // 타이머 재개
        public void ResumeTimer()
        {
            if (!isGameOver)
            {
                isTimerActive = true;
                Debug.Log($"[Timer] Resumed at Stage {currentStageID}");
            }
        }

        // UI 업데이트
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
                    // 10초 이하 - 빨간색 깜빡임
                    timerText.color = Color.Lerp(dangerColor, Color.white,
                        Mathf.PingPong(Time.time * 3, 1));
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

            // 타이머 바 업데이트
            if (timerFillBar != null)
            {
                timerFillBar.fillAmount = currentTime / timeLimit;

                // 바 색상
                if (currentTime <= 10f)
                    timerFillBar.color = dangerColor;
                else if (currentTime <= warningTime)
                    timerFillBar.color = warningColor;
                else
                    timerFillBar.color = normalColor;
            }
        }

        // 시간 초과
        void OnTimeOver()
        {
            isGameOver = true;
            isTimerActive = false;

            Debug.Log($"[Timer] TIME OVER at Stage {currentStageID}!");

            // 게임 정지
            Time.timeScale = 0f;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 게임오버 UI 표시
            ShowGameOverUI();
        }

        // 게임오버 UI 표시
        void ShowGameOverUI()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                if (gameOverText != null)
                {
                    gameOverText.text = $"시간 초과!\n\n" +
                                       $"도달 스테이지: {currentStageID + 1}층\n" +
                                       $"최고 기록: {bestStage + 1}층";
                }
            }
        }

        // 재시도 버튼
        void OnRetryClicked()
        {
            Time.timeScale = 1f;
            isGameOver = false;

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // 게임 리셋 (MapSpawnManager가 처리)
            if (MapSpawnManager.Instance != null)
            {
                MapSpawnManager.Instance.ResetGame();
            }

            // 1층부터 다시 시작
            StartStageTimer(0);
        }

        // 메인 메뉴로
        void OnMainMenuClicked()
        {

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 1f;
            // 메인 메뉴 씬으로 이동
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        // 다음 맵으로 이동 시
        public void OnNextMap(int newStageID)
        {
            StopTimer();
            StartStageTimer(newStageID);
        }

        // 기록 갱신
        void UpdateBestRecord(int stage, float clearTime)
        {
            if (stage > bestStage)
            {
                bestStage = stage;
                bestTime = clearTime;
                SaveBestRecord();

                Debug.Log($"[Timer] New best record! Stage: {bestStage + 1}, Time: {bestTime:F1}s");
            }
        }

        // 기록 저장
        void SaveBestRecord()
        {
            PlayerPrefs.SetInt("BestStage", bestStage);
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();
        }

        // 기록 불러오기
        void LoadBestRecord()
        {
            bestStage = PlayerPrefs.GetInt("BestStage", 0);
            bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        }

        // 현재 남은 시간 반환
        public float GetRemainingTime()
        {
            return currentTime;
        }

        // 현재 스테이지 반환
        public int GetCurrentStage()
        {
            return currentStageID;
        }

        // 게임오버 상태 확인
        public bool IsGameOver()
        {
            return isGameOver;
        }
    }
}