using UnityEngine;
using UnityEngine.SceneManagement;
using Tower.Game;
using Tower.Player.Data;
using Tower.UI;

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

        private PlayerController p_controller;
        protected Animator animator;

        private Parrying parrying;
        #endregion

        #region Property
        public bool IsDead => currentHP <= 0;
        public float Atk => characterBase.atk;
        public float AtkBuff { get; set; } = 1f;
        public float CurrentHP => currentHP;
        public float CurrentMP => currentMP;
        #endregion

        #region Unity Event Method
        protected virtual void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            //참조
            animator = GetComponent<Animator>();
            p_controller = GetComponent<PlayerController>();
            parrying = GetComponent<Parrying>();
            UpdateStats();
            //초기화
            currentHP = maxHP;
            currentMP = maxMP;
        }

        private void Start()
        {
            
            //Debug.Log("초기화 체력" + maxHP);
        }
        private void OnEnable()
        {
            if (fgo != null)
            {
                Destroy(fgo);
            }
            p_controller.enabled = true;
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
            
            if(!IsDead)
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

        //HP 관련 외부 호출 메서드
        #region HP
        public void TakeDamage(float damage, int groggyAmount = 0)
        {
            //패링중 무적, 비활성화 상태시 무적
            if (parrying.IsParrying || !gameObject.activeSelf)
                return;

            Debug.Log(currentHP);

            if (IsDead)
            {
                //Debug.Log("이미 죽어있는뎁쇼?");
                PlayerStatsInfo.Instance.UpdateCurrentHPInfo();
                return;
            }

            //애니메이션연출
            animator.SetTrigger(AnimHash.hit);

            damage = Mathf.Max(damage * (100f / (100f +characterBase.def)),1f);
            Debug.Log("방어력 적용 대미지: " + damage);
            //Debug.Log("아야");
            currentHP = Mathf.Max(currentHP - damage, 0);
            PlayerStatsInfo.Instance.UpdateCurrentHPInfo();
            if (IsDead)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            //TODO: 이펙트 추가
            //...
            currentHP = Mathf.Min(currentHP + amount, maxHP);
        }
        #endregion

        //MP 관련 외부 호출 메서드
        #region MP
        public bool UseMana(float amount)
        {
            if (currentMP < amount)
                return false;

            currentMP -= amount;
            return true;
        }

        public void ManaRecover(float amount)
        {
            if (IsDead) return;
            currentMP = Mathf.Min(currentMP + amount, maxMP);
        }
        #endregion

        private void Die()
        {
            //사망처리
            //p_controller.enabled = false; 애니메이터로 옮김
            Debug.Log("사망");
            animator.SetBool(AnimHash.isDead, true);
            //마나도 0
            currentMP = 0f;
            PlayerStatsInfo.Instance.UpdateCurrentHPInfo();
            //다음 캐릭터로 넘어가게
            //...
        }

        public void Revibe()
        {
            //TODO: 캐릭터 활성화
            //...
            //Heal(maxHP);
            //ManaRecover(maxMP);
            UpdateStats();
            PlayerStatsInfo.Instance.SwitchCharatersInfo();
        }

        public int GetHPForUI()
        {
            return Mathf.CeilToInt(currentHP);
        }

        public void UpdateStats()
        {
            maxHP = characterBase.maxHp;
            maxMP = characterBase.maxMp;
            //Debug.Log(maxHP);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            isSceneUnloading = true;
        }
        #endregion


    }

}
