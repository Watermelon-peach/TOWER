using UnityEngine;
using UnityEngine.SceneManagement;
using Tower.Game;
using Tower.Player.Data;
using System.Collections;
using Unity.VisualScripting;


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
        public GameObject input;

        private GameObject fgo;
        private bool isSceneUnloading = false;

        private float currentHP;
        private float maxHP;
        private float currentMP;
        private float maxMP;

        private Animator animator;
        #endregion

        #region Property
        public bool IsDead => currentHP <= 0;
        public float Atk => characterBase.atk;
        public float AtkBuff { get; set; } = 1f;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            Debug.Log("Awake");
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
        }

        private void Start()
        {
            animator = GetComponent<Animator>();
            UpdateStats(); //Awake 호출 안돼서 옮겼음
            //초기화
            currentHP = maxHP;
            currentMP = maxMP;
            //Debug.Log("초기화 체력" + maxHP);
        }
        private void OnEnable()
        {
            if (fgo != null)
            {
                Destroy(fgo);
            }
            input.SetActive(true);
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
        public void TakeDamage(float damage, int groggyAmount = 0)
        {
            Debug.Log(currentHP);

            if (IsDead)
            {
                //Debug.Log("이미 죽어있는뎁쇼?");
                return;
            }

            StartCoroutine(OnHit());

            damage = Mathf.Max(damage * (100f / (100f +characterBase.def)),1f);
            Debug.Log("방어력 적용 대미지: " + damage);
            //Debug.Log("아야");
            currentHP = Mathf.Max(currentHP - damage, 0);
            if (IsDead)
            {
                Die();
            }
        }

        private IEnumerator OnHit()
        {

            //애니메이션연출
            animator.SetTrigger(AnimHash.hit);
            input.SetActive(false);

            yield return new WaitForSeconds(1.05f);
            if (!IsDead)
            {
                input.SetActive(true);
            }
        }
        private void Die()
        {
            //사망처리
            Debug.Log("사망");
            animator.SetBool(AnimHash.isDead, true);
            input.SetActive(false);
            //다음 캐릭터로 넘어가게
            //...
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
