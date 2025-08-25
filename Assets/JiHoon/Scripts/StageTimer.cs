using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Tower.Player;

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

        [SerializeField] private Transform spawnPoint;// 스폰 위치

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
                //DontDestroyOnLoad(gameObject);
            }
            else
            {
                //Destroy(gameObject);
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
        public void ShowGameOverUI()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                if (gameOverText != null)
                {
                    gameOverText.text = $"Time Over!\n\n" +
                                       $"Stage : {currentStageID + 1}\n" +
                                       $"Best Stage: {bestStage + 1}";
                }
            }
        }


        void OnRetryClicked()
        {
            Debug.Log("[StageTimer] Retry clicked - Recreating first mast");

            // 1. 기본 설정 복원
            Time.timeScale = 1f;
            isGameOver = false;
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // 2. 캐릭터 부활
            Tower.Player.Character[] allCharacters = FindObjectsByType<Tower.Player.Character>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var character in allCharacters)
            {
                if (character != null)
                    character.Revibe();
            }

            // 3. MastManager 있는지 확인
            if (MastManager.Instance != null)
            {
                // 현재 마스트 제거
                if (MastManager.Instance.currentMastInstance != null)
                {
                    Debug.Log("[StageTimer] Destroying current mast");
                    Destroy(MastManager.Instance.currentMastInstance);
                    MastManager.Instance.currentMastInstance = null;
                }

                // 잠시 대기 후 첫 마스트 다시 생성
                StartCoroutine(RecreateFirstMast());
                Tower.Player.TeamManager.Instance.SwitchToNextCharacter();
            }
            else
            {
                Debug.LogError("[StageTimer] MastManager not found!");
            }
        }

        // 첫 마스트 재생성 코루틴
        IEnumerator RecreateFirstMast()
        {
            // 오브젝트 완전 제거 대기
            yield return null;
            yield return new WaitForSeconds(0.2f);

            // 적 제거 (안전장치)
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                Destroy(enemy);
            }

            // SpawnEnemy 정리
            GameObject spawnParent = GameObject.Find("SpawnEnemy");
            if (spawnParent != null)
            {
                foreach (Transform child in spawnParent.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            yield return new WaitForSeconds(0.2f);

            // MastManager의 LoadMast(0) 호출 - 첫 마스트 새로 생성
            Debug.Log("[StageTimer] Creating new first mast");
            MastManager.Instance.currentMastIndex = 0;
            MastManager.Instance.LoadMast(0);

            // 마스트 생성 완료 대기
            yield return new WaitForSeconds(0.5f);

            // 타이머 재시작
            StartStageTimer(0);

            Debug.Log("[StageTimer] First mast recreation completed!");
        }

        /// <summary>
        /// MapExit 체크 재활성화 코루틴
        /// </summary>
        IEnumerator ReEnableCheck(MapExit exit)
        {
            yield return new WaitForSeconds(2f);

            System.Reflection.FieldInfo canCheckField = exit.GetType().GetField("canCheck",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (canCheckField != null)
            {
                canCheckField.SetValue(exit, true);
            }
        }

        /// <summary>
        /// 모든 캐릭터 부활
        /// </summary>
        void ReviveAllCharacters()
        {
            Tower.Player.Character[] allCharacters = FindObjectsByType<Tower.Player.Character>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            foreach (Tower.Player.Character character in allCharacters)
            {
                if (character != null)
                {
                    character.Revibe();
                    Debug.Log($"[StageTimer] Revived: {character.gameObject.name}");
                }
            }
        }

        /// <summary>
        /// 남아있는 몬스터 제거 (안전장치)
        /// </summary>
        void ClearAllRemainingMonsters()
        {
            GameObject spawnParent = GameObject.Find("SpawnEnemy");
            if (spawnParent != null)
            {
                // SpawnEnemy 하위의 모든 자식 제거
                foreach (Transform child in spawnParent.transform)
                {
                    Destroy(child.gameObject);
                }
                Debug.Log("[StageTimer] Cleared all remaining monsters from SpawnEnemy");
            }

            // Enemy 태그를 가진 모든 오브젝트 제거 (추가 안전장치)
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }

            if (enemies.Length > 0)
            {
                Debug.Log($"[StageTimer] Destroyed {enemies.Length} remaining enemies");
            }
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