using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// 스테이지 클리어 시 재화 획득을 표시하는 UI
public class CardRewardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject rewardPanel; // 보상 표시 패널
    [SerializeField] private Text gradeText; // 등급 텍스트
    [SerializeField] private Text goldAmountText; // 골드 획득량 텍스트
    [SerializeField] private Text clearTimeText; // 클리어 시간 텍스트
    [SerializeField] private Image gradeIcon; // 등급 아이콘
    [SerializeField] private Button confirmButton; // 확인 버튼
    [SerializeField] private GameObject autoCloseText; // "5초 후 자동으로 닫힙니다" 텍스트

    [Header("Grade Settings")]
    [SerializeField] private List<RewardCard> gradeRewards; // 등급별 보상 데이터 (빠른 순서대로)
    [SerializeField] private float autoCloseTime = 5f; // 자동 종료 시간

    private System.Action onRewardClosed; // 보상 UI 닫힐 때 콜백
    private Coroutine autoCloseCoroutine;
    private float stageStartTime; // 스테이지 시작 시간

    private static CardRewardUI instance;
    public static CardRewardUI Instance => instance;

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
        }

        // 시작 시 숨기기
        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        // 확인 버튼 이벤트 설정
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);

        // 등급 보상 데이터 정렬 (빠른 시간 순)
        if (gradeRewards != null && gradeRewards.Count > 0)
        {
            gradeRewards = gradeRewards.OrderBy(r => r.timeThreshold).ToList();
        }
    }

    // 스테이지 시작 시 호출
    public void OnStageStart()
    {
        stageStartTime = Time.time;
        Debug.Log($"Stage started at: {stageStartTime}");
    }

    // 보상 UI 표시 (기본 - 시간 자동 계산)
    public void ShowReward(System.Action onComplete)
    {
        float clearTime = Time.time - stageStartTime;
        ShowRewardWithTime(clearTime, onComplete);
    }

    // 보상 UI 표시 (맵 ID와 클리어 시간 지정)
    public void ShowReward(int mapID, System.Action onComplete)
    {
        float clearTime = Time.time - stageStartTime;
        ShowRewardWithTime(clearTime, onComplete, mapID);
    }

    // 보상 UI 표시 (클리어 시간 직접 지정)
    public void ShowRewardWithTime(float clearTime, System.Action onComplete, int mapID = 0)
    {
        onRewardClosed = onComplete;

        // 패널 활성화
        rewardPanel.SetActive(true);

        // 시간 정지
        Time.timeScale = 0f;

        // 등급 계산
        RewardCard gradeData = CalculateGrade(clearTime);

        if (gradeData == null)
        {
            Debug.LogError("No grade data found!");
            CloseRewardUI();
            return;
        }

        // 골드 계산
        int baseGold = gradeData.baseGoldReward + (mapID * 50);
        int finalGold = Mathf.RoundToInt(baseGold * gradeData.goldMultiplier);

        // 약간의 랜덤성 추가
        finalGold += Random.Range(-10, 11);

        // UI 업데이트
        if (gradeText != null)
            gradeText.text = gradeData.gradeName;

        if (gradeIcon != null && gradeData.gradeIcon != null)
        {
            gradeIcon.sprite = gradeData.gradeIcon;
            gradeIcon.color = gradeData.gradeColor;
        }

        if (goldAmountText != null)
            goldAmountText.text = $"골드 {finalGold} 획득!";

        if (clearTimeText != null)
        {
            int minutes = Mathf.FloorToInt(clearTime / 60);
            int seconds = Mathf.FloorToInt(clearTime % 60);
            clearTimeText.text = $"클리어 시간: {minutes:00}:{seconds:00}";
        }

        // 보상 지급
        ApplyRewards(finalGold, gradeData);

        // 자동 종료 코루틴 시작
        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);
        autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
    }

    // 클리어 시간에 따른 등급 계산
    RewardCard CalculateGrade(float clearTime)
    {
        if (gradeRewards == null || gradeRewards.Count == 0)
        {
            Debug.LogError("No grade rewards configured!");
            return null;
        }

        // 가장 빠른 시간부터 체크
        foreach (var grade in gradeRewards)
        {
            if (clearTime <= grade.timeThreshold)
            {
                Debug.Log($"Clear time: {clearTime}s, Grade: {grade.gradeName}");
                return grade;
            }
        }

        // 모든 시간을 초과하면 가장 낮은 등급
        return gradeRewards[gradeRewards.Count - 1];
    }

    // 실제 보상 지급
    void ApplyRewards(int goldAmount, RewardCard gradeData)
    {
        Debug.Log($"Rewards - Gold: {goldAmount}, Grade: {gradeData.gradeName}");

        // 골드 지급
        // 예: PlayerData.Instance.AddGold(goldAmount);

        // 추가 보상 지급
        if (gradeData.bonusExp > 0)
        {
            Debug.Log($"Bonus EXP: {gradeData.bonusExp}");
            // 예: PlayerData.Instance.AddExp(gradeData.bonusExp);
        }

        if (gradeData.bonusGems > 0)
        {
            Debug.Log($"Bonus Gems: {gradeData.bonusGems}");
            // 예: PlayerData.Instance.AddGems(gradeData.bonusGems);
        }
    }

    // 자동 종료 코루틴
    IEnumerator AutoCloseCoroutine()
    {
        float timer = autoCloseTime;

        // 자동 종료 텍스트 표시
        if (autoCloseText != null)
            autoCloseText.SetActive(true);

        // 실시간 모드에서 카운트다운
        while (timer > 0)
        {
            // Time.timeScale이 0이어도 작동하도록 실시간 사용
            yield return new WaitForSecondsRealtime(0.1f);
            timer -= 0.1f;

            // 남은 시간 표시 (선택사항)
            if (autoCloseText != null)
            {
                Text countdownText = autoCloseText.GetComponent<Text>();
                if (countdownText != null)
                    countdownText.text = $"{Mathf.Ceil(timer)}초 후 자동으로 닫힙니다";
            }
        }

        // 시간이 다 되면 UI 닫기
        CloseRewardUI();
    }

    // 확인 버튼 클릭
    void OnConfirmClicked()
    {
        // 자동 종료 코루틴 중지
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
        rewardPanel.SetActive(false);
        Time.timeScale = 1f; // 시간 복구

        if (autoCloseText != null)
            autoCloseText.SetActive(false);

        // 콜백 실행
        onRewardClosed?.Invoke();
    }

    // 외부에서 강제로 닫기
    public void ForceClose()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        CloseRewardUI();
    }

    // 옵션: 특정 등급 강제 표시 (테스트용)
    public void ShowSpecificGrade(RewardGrade grade, System.Action onComplete)
    {
        onRewardClosed = onComplete;
        rewardPanel.SetActive(true);
        Time.timeScale = 0f;

        RewardCard gradeData = gradeRewards.FirstOrDefault(r => r.grade == grade);
        if (gradeData != null)
        {
            // UI 업데이트
            if (gradeText != null)
                gradeText.text = gradeData.gradeName;

            if (gradeIcon != null && gradeData.gradeIcon != null)
            {
                gradeIcon.sprite = gradeData.gradeIcon;
                gradeIcon.color = gradeData.gradeColor;
            }

            if (goldAmountText != null)
                goldAmountText.text = $"골드 {gradeData.baseGoldReward} 획득!";
        }

        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);
        autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine());
    }
}