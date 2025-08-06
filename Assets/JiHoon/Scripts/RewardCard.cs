using UnityEngine;

// 등급별 보상 데이터 ScriptableObject
[CreateAssetMenu(fileName = "RewardCard", menuName = "Reward/Card")]
public class RewardCard : ScriptableObject
{
    [Header("Grade Info")]
    public string gradeName = "Bronze";
    public string gradeDescription = "기본 보상";

    [Header("Visual")]
    public Sprite gradeIcon;
    public Color gradeColor = Color.white;

    [Header("Reward Data")]
    public int Score = 100;      // 고정 골드 보상

    [Header("Time Requirements")]
    public float timeThreshold = 300f;     // 이 등급을 받기 위한 최대 시간 (초)
}