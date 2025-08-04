using UnityEngine;
using UnityEngine.SceneManagement;
using Tower.Game;
using Tower.Player.Data;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tower.Player
{
    public class Character : MonoBehaviour, IDamageable
    {
        #region Variables
        public CharacterBaseSO characterBase;
        public GameObject fairyForm;
        private GameObject fgo;
        private bool isSceneUnloading = false;

        private float currentHP;
        private float maxHP;
        private float currentMP;
        private float maxMP;

        #endregion

        #region Property
        public bool IsDead => currentHP <= 0;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            UpdateStats();
        }

        private void OnEnable()
        {
            if (fgo != null)
            {
                Destroy(fgo);
            }
        }

        private void OnDisable()
        {
            // 씬 전환 시 버그 방지
            if (isSceneUnloading)
                return;
            
#if UNITY_EDITOR
            //에디터 버그 방지
            if (!EditorApplication.isPlaying || !EditorApplication.isPlayingOrWillChangePlaymode)
                return;
#endif
            fgo = Instantiate(fairyForm, new Vector3(transform.position.x, 2f, transform.position.z), Quaternion.identity);
        }

        private void OnDestroy()
        {
            //버그 방지
            if (fgo != null)
            {
                Destroy(fgo);
            }
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
        #endregion

        #region Custom Method
        public void TakeDamage(float damage, float groggyAmount = 0)
        {
            if (IsDead) return;
            damage = Mathf.Max(damage * (100f / (100f +characterBase.def)),1f);
            Debug.Log("방어력 적용 대미지: " + damage);
            currentHP = Mathf.Max(currentHP - damage, 0);
            if (IsDead)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("사망");
            //Destroy(gameObject); 사망처리
        }

        public int GetHPForUI()
        {
            return Mathf.CeilToInt(currentHP);
        }

        public void UpdateStats()
        {
            maxHP = characterBase.maxHp;
            currentHP = maxHP;

            maxMP = characterBase.maxMp;
            currentMP = maxMP;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            isSceneUnloading = true;
        }
        #endregion


    }

}
