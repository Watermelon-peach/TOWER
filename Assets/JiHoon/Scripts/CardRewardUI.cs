using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using Tower.Enemy;

namespace Tower.UI
{
    public class CardRewardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject rewardPanel;           // 보상 표시 패널
        [SerializeField] private TextMeshProUGUI ScoreText;       // 점수 표시 텍스트
        [SerializeField] private TextMeshProUGUI totalScoreText;  // 총점수 표시 텍스트 (별도)
        [SerializeField] private TextMeshProUGUI clearTimeText;   // 클리어 시간 텍스트
        [SerializeField] private Button confirmButton;            // 확인 버튼
        [SerializeField] private Image cardImage;                 // 카드 이미지

        [Header("Grade Settings")]
        [SerializeField] private List<RewardCard> gradeRewards = new List<RewardCard>();  // ScriptableObject 리스트

        [Header("Settings")]
        [SerializeField] private float autoCloseTime = 10f;        // 자동 종료 시간 (10초)
        [SerializeField] private float scoreAnimationDuration = 1.5f; // 점수 애니메이션 시간
        [SerializeField] private float scoreAnimationDelay = 0.5f;    // 애니메이션 시작 전 대기 시간

        private System.Action onRewardClosed;                     // 보상 UI 닫힐 때 콜백
        private Coroutine autoCloseCoroutine;
        private Coroutine scoreAnimationCoroutine;                // 점수 애니메이션 코루틴
        private float stageStartTime;                             // 스테이지 시작 시간

        // 누적 점수 관리
        private int totalScoreEarned = 0;                         // 전체 게임에서 획득한 총 점수
        private int currentStageScore = 0;                        // 현재 스테이지에서 획득한 점수

        // 싱글톤
        private static CardRewardUI instance;
        public static CardRewardUI Instance => instance;

        void Awake()
        {
            // 싱글톤 설정
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                return;
            }

            // 시작 시 패널 숨기기
            if (rewardPanel != null)
                rewardPanel.SetActive(false);

            // 확인 버튼 이벤트 연결
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            // 등급 데이터를 시간 순으로 정렬
            SortGradeRewards();
        }

        // 등급 데이터 정렬
        void SortGradeRewards()
        {
            if (gradeRewards != null && gradeRewards.Count > 0)
            {
                gradeRewards = gradeRewards.OrderBy(r => r.timeThreshold).ToList();
            }
        }

        // 스테이지 시작 시 호출 (전체 게임 시작)
        public void OnStageStart()
        {
            stageStartTime = Time.time;
            totalScoreEarned = 0;  // 게임 시작 시 총 점수 초기화
            Debug.Log($"[CardRewardUI] Stage started at: {stageStartTime}");
        }

        // 각 맵 시작 시 호출 (개별 맵 시간 측정)
        public void OnMapStart(int mapID)
        {
            stageStartTime = Time.time;
            Debug.Log($"[CardRewardUI] Map {mapID} started at: {stageStartTime}");
        }

        // 보상 UI 표시
        public void ShowReward(int mapID, System.Action onComplete)
        {
            // 디버그: 현재 시간과 시작 시간 출력
            Debug.Log($"[CardRewardUI] Current Time: {Time.time}, Start Time: {stageStartTime}");

            // 클리어 시간 계산
            float clearTime;

            // 시작 시간이 설정되지 않은 경우 - mapID가 0이면 특별 처리
            if (stageStartTime <= 0.01f)
            {
                if (mapID == 0)
                {
                    // 첫 번째 맵은 게임 시작부터 지금까지의 시간 사용
                    clearTime = Time.time;
                    Debug.Log($"[CardRewardUI] Map 0 - Using Time.time as clear time: {clearTime}");
                }
                else
                {
                    Debug.LogWarning("[CardRewardUI] Stage start time not set, using default 120 seconds");
                    clearTime = 120f;
                }
            }
            else
            {
                clearTime = Time.time - stageStartTime;
            }

            Debug.Log($"[CardRewardUI] Calculated clear time: {clearTime:F1} seconds");

            // 콜백 저장
            onRewardClosed = onComplete;

            // 패널 활성화
            if (rewardPanel != null)
                rewardPanel.SetActive(true);

            // 시간 정지
            Time.timeScale = 0f;

            // 등급 계산
            RewardCard grade = GetGradeByTime(clearTime);

            if (grade == null)
            {
                Debug.LogError("[CardRewardUI] No grade found for clear time!");
                CloseRewardUI();
                return;
            }

            // UI 업데이트
            DisplayRewardUI(grade, clearTime);

            // 점수 지급
            GiveScoreReward(grade.Score);

            // 자동 종료 시작
            StartAutoClose();
        }

        // 시간에 따른 등급 계산
        RewardCard GetGradeByTime(float clearTime)
        {
            if (gradeRewards == null || gradeRewards.Count == 0)
            {
                Debug.LogError("[CardRewardUI] No grade rewards configured!");
                return null;
            }

            foreach (var grade in gradeRewards)
            {
                if (grade != null && clearTime <= grade.timeThreshold)
                {
                    Debug.Log($"[CardRewardUI] Clear time: {clearTime:F1}s → Score: {grade.Score}");
                    return grade;
                }
            }

            // 모든 시간을 초과한 경우 마지막 등급 반환
            return gradeRewards[gradeRewards.Count - 1];
        }

        // UI 표시 업데이트
        void DisplayRewardUI(RewardCard grade, float clearTime)
        {
            // 카드 이미지 설정
            if (cardImage != null && grade.gradeIcon != null)
            {
                cardImage.sprite = grade.gradeIcon;
                Debug.Log($"[CardRewardUI] Card image set to: {grade.gradeName}");
            }

            // 현재 스테이지 점수 저장
            currentStageScore = grade.Score;

            // 클리어 시간 (실제 시간 표시)
            if (clearTimeText != null)
            {
                int minutes = Mathf.FloorToInt(clearTime / 60);
                int seconds = Mathf.FloorToInt(clearTime % 60);
                clearTimeText.text = $"Clear Time : {minutes:00}:{seconds:00}";
            }

            // 점수 애니메이션 시작
            if (scoreAnimationCoroutine != null)
                StopCoroutine(scoreAnimationCoroutine);

            scoreAnimationCoroutine = StartCoroutine(AnimateScore());
        }

        // 점수 애니메이션 코루틴
        IEnumerator AnimateScore()
        {
            float elapsed = 0f;

            int startRewardScore = currentStageScore;
            int startTotalScore = totalScoreEarned;
            int targetTotalScore = totalScoreEarned + currentStageScore;

            // 초기 표시
            if (ScoreText != null)
                ScoreText.text = $"Score : {startRewardScore} ";

            if (totalScoreText != null)
                totalScoreText.text = $"Total : {startTotalScore} ";

            // 애니메이션 시작 전 잠시 대기 (보여주기 위해)
            yield return new WaitForSecondsRealtime(scoreAnimationDelay);

            // 점수 이동 애니메이션
            while (elapsed < scoreAnimationDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Time.timeScale이 0이어도 작동
                float progress = elapsed / scoreAnimationDuration;

                // 이징 함수 (부드러운 애니메이션)
                float easedProgress = Mathf.SmoothStep(0, 1, progress);

                // 보상 점수는 줄어들고
                int currentReward = Mathf.RoundToInt(Mathf.Lerp(startRewardScore, 0, easedProgress));

                // 총점수는 늘어남
                int currentTotal = Mathf.RoundToInt(Mathf.Lerp(startTotalScore, targetTotalScore, easedProgress));

                // UI 업데이트
                if (ScoreText != null)
                {
                    if (currentReward > 0)
                        ScoreText.text = $"Score {currentReward} ";
                    else
                        ScoreText.text = ""; // 0이 되면 숨김
                }

                if (totalScoreText != null)
                {
                    totalScoreText.text = $"Total : {currentTotal} ";

                    // 점수가 오를 때 색상 효과 (옵션)
                    if (currentTotal > startTotalScore)
                    {
                        totalScoreText.color = Color.Lerp(Color.white, Color.yellow, Mathf.PingPong(elapsed * 4, 1));
                    }
                }

                yield return null;
            }

            // 최종 값 설정
            if (ScoreText != null)
                ScoreText.text = ""; // 보상 점수는 비움

            if (totalScoreText != null)
            {
                totalScoreText.text = $"Total Score: {targetTotalScore}";
                totalScoreText.color = Color.white; // 색상 원래대로
            }
        }

        // 점수 지급
        void GiveScoreReward(int scoreAmount)
        {
            // 누적 점수 업데이트
            totalScoreEarned += scoreAmount;

            Debug.Log($"[CardRewardUI] 현재 스테이지 점수: {scoreAmount}, 총 누적 점수: {totalScoreEarned}");

            // TODO: 실제 플레이어 점수 증가 코드 연결
            // 예시:
            // PlayerData.Instance?.AddScore(scoreAmount);
            // GameManager.Instance?.AddPlayerScore(scoreAmount);
        }

        // 자동 종료 시작
        void StartAutoClose()
        {
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);

            autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
        }

        // 자동 종료 코루틴 (10초 후 자동 종료)
        IEnumerator AutoCloseCoroutine()
        {
            yield return new WaitForSecondsRealtime(autoCloseTime);
            CloseRewardUI();
        }

        // 확인 버튼 클릭
        void OnConfirmClicked()
        {
            // 자동 종료 중지
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }

            CloseRewardUI();
        }

        // UI 닫기
        void CloseRewardUI()
        {
            // 애니메이션 중단
            if (scoreAnimationCoroutine != null)
            {
                StopCoroutine(scoreAnimationCoroutine);
                scoreAnimationCoroutine = null;
            }

            // 패널 비활성화
            if (rewardPanel != null)
                rewardPanel.SetActive(false);

            // 시간 복구
            Time.timeScale = 1f;

            // 콜백 실행
            onRewardClosed?.Invoke();
            onRewardClosed = null;

            Debug.Log("[CardRewardUI] Reward UI closed");
        }

        // 강제 종료 (외부에서 호출 가능)
        public void ForceClose()
        {
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }

            CloseRewardUI();
        }

        // 현재까지 획득한 총 점수 반환
        public int GetTotalScoreEarned()
        {
            return totalScoreEarned;
        }

        // 게임 초기화 시 점수 리셋
        public void ResetTotalScore()
        {
            totalScoreEarned = 0;
            currentStageScore = 0;
        }

        //// ===== 테스트용 메서드들 =====
        //[ContextMenu("Test First Grade (30초)")]
        //void TestFirstGrade()
        //{
        //    stageStartTime = Time.time - 30f;
        //    ShowReward(1, () => Debug.Log("First Grade test completed"));
        //}

        //[ContextMenu("Test Second Grade (80초)")]
        //void TestSecondGrade()
        //{
        //    stageStartTime = Time.time - 80f;
        //    ShowReward(1, () => Debug.Log("Second Grade test completed"));
        //}

        //[ContextMenu("Test Third Grade (110초)")]
        //void TestThirdGrade()
        //{
        //    stageStartTime = Time.time - 110f;
        //    ShowReward(1, () => Debug.Log("Third Grade test completed"));
        //}

        //[ContextMenu("Test Last Grade (300초)")]
        //void TestLastGrade()
        //{
        //    stageStartTime = Time.time - 300f;
        //    ShowReward(1, () => Debug.Log("Last Grade test completed"));
        //}
    }
}