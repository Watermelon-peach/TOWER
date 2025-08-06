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
        [SerializeField] private TextMeshProUGUI goldAmountText;  // 골드 획득량 텍스트
        [SerializeField] private TextMeshProUGUI clearTimeText;   // 클리어 시간 텍스트
        [SerializeField] private Button confirmButton;            // 확인 버튼3
        [SerializeField] private Image cardImage;   //카드 이미지

        [Header("Grade Settings")]
        [SerializeField] private List<RewardCard> gradeRewards = new List<RewardCard>();  // ScriptableObject 리스트

        [Header("Settings")]
        [SerializeField] private float autoCloseTime = 10f;        // 자동 종료 시간 (10초)

        private System.Action onRewardClosed;                     // 보상 UI 닫힐 때 콜백
        private Coroutine autoCloseCoroutine;
        private float stageStartTime;                             // 스테이지 시작 시간

        // 누적 골드 관리
        private int totalGoldEarned = 0;                          // 전체 게임에서 획득한 총 골드
        private int currentStageGold = 0;                         // 현재 스테이지에서 획득한 골드

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
            totalGoldEarned = 0;  // 게임 시작 시 총 골드 초기화
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

            // 골드 지급
            GiveGoldReward(grade.Score);

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
                    Debug.Log($"[CardRewardUI] Clear time: {clearTime:F1}s → Gold: {grade.Score}");
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
            }

            // 현재 스테이지 골드 저장
            currentStageGold = grade.Score;

            // 골드 텍스트 (현재 스테이지 골드 + 총 누적 골드)
            if (goldAmountText != null)
            {
                goldAmountText.text = $"+{currentStageGold} 골드";
                // 누적 골드가 있으면 추가 표시
                if (totalGoldEarned > 0)
                {
                    goldAmountText.text += $"\n<size=20>총 획득: {totalGoldEarned + currentStageGold} 골드</size>";
                }
            }

            // 클리어 시간 (실제 시간 표시)
            if (clearTimeText != null)
            {
                int minutes = Mathf.FloorToInt(clearTime / 60);
                int seconds = Mathf.FloorToInt(clearTime % 60);
                clearTimeText.text = $"클리어 시간: {minutes:00}:{seconds:00}";
            }
        }

        // 골드 지급
        void GiveGoldReward(int goldAmount)
        {
            // 누적 골드 업데이트
            totalGoldEarned += goldAmount;

            Debug.Log($"[CardRewardUI] 현재 스테이지 골드: {goldAmount}, 총 누적 골드: {totalGoldEarned}");

            // TODO: 실제 플레이어 골드 증가 코드 연결
            // 예시:
            // PlayerData.Instance?.AddGold(goldAmount);
            // GameManager.Instance?.AddPlayerGold(goldAmount);
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

        // 현재까지 획득한 총 골드 반환
        public int GetTotalGoldEarned()
        {
            return totalGoldEarned;
        }

        // 게임 초기화 시 골드 리셋
        public void ResetTotalGold()
        {
            totalGoldEarned = 0;
            currentStageGold = 0;
        }

        // ===== 테스트용 메서드들 =====
        [ContextMenu("Test First Grade (30초)")]
        void TestFirstGrade()
        {
            stageStartTime = Time.time - 30f;
            ShowReward(1, () => Debug.Log("First Grade test completed"));
        }

        [ContextMenu("Test Second Grade (80초)")]
        void TestSecondGrade()
        {
            stageStartTime = Time.time - 80f;
            ShowReward(1, () => Debug.Log("Second Grade test completed"));
        }

        [ContextMenu("Test Third Grade (110초)")]
        void TestThirdGrade()
        {
            stageStartTime = Time.time - 110f;
            ShowReward(1, () => Debug.Log("Third Grade test completed"));
        }

        [ContextMenu("Test Last Grade (300초)")]
        void TestLastGrade()
        {
            stageStartTime = Time.time - 300f;
            ShowReward(1, () => Debug.Log("Last Grade test completed"));
        }
    }
}