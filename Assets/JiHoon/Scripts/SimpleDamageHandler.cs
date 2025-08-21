using UnityEngine;
using System.Collections.Generic;
using Tower.Effects;
using Tower.Enemy;
using Tower.Player;  // Character 클래스용

public class SimpleDamageHandler : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private DamageType damageType = DamageType.Collision;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float damageInterval = 1f;

    private AttackEffectData effectData;
    private bool hasDealtDamage = false;
    private Dictionary<GameObject, float> lastDamageTime = new Dictionary<GameObject, float>();

    // Enemy 컴포넌트 참조 (데미지 값 가져오기용)
    private Enemy enemyComponent;

    void Start()
    {
        // Enemy 컴포넌트 찾기 (데미지 값을 가져오기 위해)
        enemyComponent = GetComponentInParent<Enemy>();
        if (enemyComponent == null)
        {
            enemyComponent = GetComponent<Enemy>();
        }
    }

    public void Initialize(AttackEffectData data)
    {
        if (data != null)
        {
            effectData = data;
            damageType = data.damageType;
            damage = data.damage;
            damageInterval = data.damageInterval;
        }
        hasDealtDamage = false;
        lastDamageTime.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        if (damageType != DamageType.Collision) return;
        if (hasDealtDamage) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log($"[SimpleDamageHandler] Player 히트!");

            // Character 컴포넌트 찾기
            Character character = other.GetComponent<Character>();
            if (character == null)
            {
                character = other.GetComponentInParent<Character>();
                if (character == null)
                {
                    character = other.GetComponentInChildren<Character>();
                }
            }

            if (character != null)
            {
                // Enemy의 공격력 값 사용 (없으면 기본 damage 값)
                float damageAmount = enemyComponent != null ? enemyComponent.data.atk : damage;

                Debug.Log($"[SimpleDamageHandler] {damageAmount} 데미지 전달!");
                character.TakeDamage(damageAmount);
                hasDealtDamage = true;
            }
            else
            {
                Debug.LogError($"Character 컴포넌트를 찾을 수 없음: {other.name}");
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (damageType != DamageType.Area) return;

        if (other.CompareTag("Player"))
        {
            if (!lastDamageTime.ContainsKey(other.gameObject))
            {
                lastDamageTime[other.gameObject] = 0;
            }

            if (Time.time - lastDamageTime[other.gameObject] >= damageInterval)
            {
                Character character = other.GetComponent<Character>();
                if (character == null)
                {
                    character = other.GetComponentInParent<Character>();
                    if (character == null)
                    {
                        character = other.GetComponentInChildren<Character>();
                    }
                }

                if (character != null)
                {
                    float damageAmount = enemyComponent != null ? enemyComponent.data.atk : damage;
                    character.TakeDamage(damageAmount);
                    lastDamageTime[other.gameObject] = Time.time;
                }
            }
        }
    }

    void OnDisable()
    {
        hasDealtDamage = false;
        lastDamageTime.Clear();
    }
}