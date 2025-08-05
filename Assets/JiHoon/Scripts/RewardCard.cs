using UnityEngine;

// 보상 등급
public enum RewardGrade
{
    D,   // 브론즈 (느림)
    C,   // 실버
    B,     // 골드
    A// 플래티넘 (빠름)
}

// 등급별 보상 데이터 ScriptableObject
[CreateAssetMenu(fileName = "RewardCard", menuName = "Reward/Card")]
public class RewardCard : ScriptableObject
{
    [Header("Grade Info")]
    public RewardGrade grade = RewardGrade.D;
    public string gradeName = "Bronze";
    public string gradeDescription = "기본 보상";

    [Header("Visual")]
    public Sprite gradeIcon;
    public Color gradeColor = Color.white;

    [Header("Reward Data")]
    public int baseGoldReward = 100;      // 기본 골드 보상
    public float goldMultiplier = 1.0f;   // 골드 배수

    [Header("Time Requirements")]
    public float timeThreshold = 300f;     // 이 등급을 받기 위한 최대 시간 (초)

    [Header("Additional Rewards")]
    public int bonusExp = 0;              // 보너스 경험치
    public int bonusGems = 0;             // 보너스 젬
}