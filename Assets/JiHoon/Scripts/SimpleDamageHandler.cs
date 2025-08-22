using UnityEngine;
using System.Collections.Generic;
using Tower.Effects;
using Tower.Player;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimpleDamageHandler : MonoBehaviour
{
    // Inspector에서 수정 불가능하도록 private으로 변경
    private AttackEffectData effectData;
    private DamageType damageType = DamageType.None;
    private float damage = 0f;
    private float damageInterval = 1f;
    private bool hasDealtDamage = false;
    private Dictionary<GameObject, float> lastDamageTime = new Dictionary<GameObject, float>();

    [Header("Debug Info (Read Only)")]
    [SerializeField, ReadOnly] private string currentEffectName = "None";
    [SerializeField, ReadOnly] private float currentDamage = 0f;
    [SerializeField, ReadOnly] private string currentDamageType = "None";

    public void Initialize(AttackEffectData data)
    {
        if (data == null)
        {
            Debug.LogError($"[SimpleDamageHandler] AttackEffectData가 null입니다!");
            return;
        }

        effectData = data;
        damageType = data.damageType;
        damage = data.damage;
        damageInterval = data.damageInterval;

        // 디버그용 정보 업데이트
        currentEffectName = data.effectName;
        currentDamage = data.damage;
        currentDamageType = data.damageType.ToString();

        Debug.Log($"[SimpleDamageHandler] 초기화 완료 - 이펙트: {data.effectName}, 데미지: {damage}, 타입: {damageType}");

        hasDealtDamage = false;
        lastDamageTime.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        // effectData가 없으면 처리하지 않음
        if (effectData == null) return;
        if (damageType != DamageType.Collision) return;
        if (hasDealtDamage) return;

        if (other.CompareTag("Player"))
        {
            Character character = FindCharacterComponent(other);

            if (character != null)
            {
                Debug.Log($"[SimpleDamageHandler] {effectData.effectName} - {damage} 데미지 전달!");
                character.TakeDamage(damage);
                hasDealtDamage = true;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        // effectData가 없으면 처리하지 않음
        if (effectData == null) return;
        if (damageType != DamageType.Area) return;

        if (other.CompareTag("Player"))
        {
            if (!lastDamageTime.ContainsKey(other.gameObject))
            {
                lastDamageTime[other.gameObject] = 0;
            }

            if (Time.time - lastDamageTime[other.gameObject] >= damageInterval)
            {
                Character character = FindCharacterComponent(other);

                if (character != null)
                {
                    character.TakeDamage(damage);
                    lastDamageTime[other.gameObject] = Time.time;
                    Debug.Log($"[SimpleDamageHandler] {effectData.effectName} - 지속 데미지 {damage} 전달!");
                }
            }
        }
    }

    private Character FindCharacterComponent(Collider other)
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

        if (character == null)
        {
            Debug.LogError($"Character 컴포넌트를 찾을 수 없음: {other.name}");
        }

        return character;
    }

    void OnDisable()
    {
        hasDealtDamage = false;
        lastDamageTime.Clear();
    }
}

// ReadOnly 속성을 위한 PropertyAttribute
public class ReadOnlyAttribute : UnityEngine.PropertyAttribute { }

// Editor 폴더가 아니어도 작동하도록 조건부 컴파일
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif