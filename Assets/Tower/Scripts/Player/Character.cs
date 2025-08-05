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


        [Header("Attack Settings")]
        [SerializeField] private Transform attackPoint; // 공격 지점
        [SerializeField] private float attackRadius = 2f; // 공격 범위
        [SerializeField] private LayerMask enemyLayer; // 적 레이어

        #endregion

        #region Property
        public bool IsDead => currentHP <= 0;
        public float Atk => characterBase.atk;
        public float AtkBuff { get; set; } = 1f;
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
        public void TakeDamage(float damage, int groggyAmount = 0)
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

        public void DealDamage()
        {
            // 공격 지점 설정 (attackPoint가 없으면 자신의 위치 사용)
            Vector3 attackPos = attackPoint ? attackPoint.position : transform.position;

            // 범위 내의 모든 적 찾기
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, attackRadius, enemyLayer);

            foreach (Collider enemy in hitEnemies)
            {
                // IDamageable 인터페이스를 구현한 적에게 데미지 주기
                Character damageable = enemy.GetComponent<Character>();
                if (damageable != null)
                {
                    // 데미지 주기 
                    damageable.TakeDamage(Atk); 

                    // 넉백 효과 (선택사항)
                    Rigidbody rb = enemy.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                        rb.AddForce(knockbackDir * 5f, ForceMode.Impulse);
                    }
                }
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
