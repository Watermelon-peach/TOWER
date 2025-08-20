using UnityEngine;
using UnityEngine.UI;
using Tower.Enemy.Data;
using Tower.Game;
using System.Collections;
using Tower.Player;
using TMPro;
using UnityEngine.AI;
using Unity.Behavior;

namespace Tower.Enemy
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        #region Variables
        [Header("적 데이터")]
        public EnemyBaseSO data;

        [Header("스탯 바")]
        public CanvasGroup statBar; //스탯 바 뭉탱이
        public Image hpGauge;
        public Image gpGauge;
        public GameObject groggyIcon;    //그로기 상태 시 표시할 아이콘
        public TextMeshProUGUI gpText;
        public Image groggyTimerImage;

        private Image gpFill;
        private float currentHP;    //현재 체력
        private float maxHP;        //최대 체력 (db 기반)
        private int currentGP;    //현재 그로기 포인트
        private int maxGP;        //최대 그로기 포인트 (db 기반)

        private bool isGroggy;
        [SerializeField] private float groggyDuration = 10f;

        //스탯 바 관련 변수들
        private float hideDelay = 5f;
        [SerializeField] private float fadeDuration = 1f;
        private Coroutine hideCoroutine;
        private float lastDamageTime;
        private Color startGpColor;

        [HideInInspector]public Animator animator;

        //Enemy 공격애니메이션 이벤트 메서드
        //[SerializeField] private Transform attackPoint; // 공격 지점
        [SerializeField] private float attackRadius = 2f; // 공격 범위
        [SerializeField] private LayerMask targetLayer; // 적 레이어

        [Header("패링")]
        [SerializeField] private float parryingTime = 0.5f;  //패링허용 시간(임시0.5s)
        [SerializeField] private GameObject jigumiya;

        [Header("이펙트")]
        [SerializeField] private ParticleSystem hitVfx;

        [Header("넉백")]
        [SerializeField] private float distance = 2f;
        [SerializeField] private float height = 2f;

        private NavMeshAgent agent;
        //private Rigidbody rb;
        private BehaviorGraphAgent bgAgent;
        private EnemyAI enemyAI;
        #endregion

        #region Property
        public bool IsDead => currentHP <= 0;
        public bool CanParry { get; private set; }
        
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            //참조
            animator = GetComponent<Animator>();
            gpFill = gpGauge.transform.Find("Fill").GetComponent<Image>();
            agent = GetComponent<NavMeshAgent>();
            //rb = GetComponent<Rigidbody>();
            bgAgent = GetComponent<BehaviorGraphAgent>();
            enemyAI = GetComponent<EnemyAI>();


            //값 설정
            maxHP = data.maxHp;
            maxGP = data.maxGp;
        }

        private void Start()
        {
            //초기화
            currentHP = maxHP;
            currentGP = 0;
            HideStatBar();
            startGpColor = gpFill.color;
            gpText.text = currentGP.ToString();
            groggyIcon.SetActive(false);
        }

        private void Update()
        {
/*            //TODO : UI 테스트
            if (Input.GetKeyDown(KeyCode.M))
            {
                TakeDamage(10, 10);
            }*/
        }
        #endregion

        
        #region Custom Method
        public void TakeDamage(float damage, int groggyAmount = 0)
        {
            if (IsDead) return; //중첩사망처리 방지
            
            damage = Mathf.Max(damage * (100f / (100f + data.def)), 1f);
            Debug.Log("방어력 적용 대미지: " + damage);

            //그로기 중복 적용 방지
            if(!isGroggy)
            {
                currentGP += groggyAmount;

                if (currentGP >= maxGP)
                {
                    currentGP = maxGP;
                    isGroggy = true;
                    StartCoroutine(OnGroggyState());
                }
            }
            if (!isGroggy)
                animator.SetTrigger(AnimHash.hit);

            //Debug.Log("현재 GP: " + currentGP);
            //그로기 약체화 배율 처리
            if (isGroggy)
                damage *= 1.5f;

            currentHP = Mathf.Max(currentHP - damage, 0);

            //UI, 이펙트
            ShowStatBar();
            hitVfx.Play();

            if (IsDead)
            {
                Die();
            }
        }

        //그로기타임
        private IEnumerator OnGroggyState()
        {
            //그로기 연출
            animator.SetTrigger(AnimHash.groggy);
            //상태 표시
            groggyIcon.SetActive(true);

            gpFill.color = Color.red;

            groggyTimerImage.fillAmount = 1f;
            float groggyCount = groggyDuration;

            while (groggyCount >= 0)
            {
                groggyCount -= Time.deltaTime;
                //타이머 UI 표시
                groggyTimerImage.fillAmount = groggyCount / groggyDuration;
                yield return null;
            }
            //그로기 끝, 상태 초기화
            groggyIcon.SetActive(false);
            currentGP = 0;
            gpFill.color = startGpColor;
            isGroggy = false;
            animator.SetTrigger(AnimHash.endGroggy);
            ShowStatBar();
        }

        //패링
        public void Jigumini()
        {
            CanParry = true;
            //vfx 활성화
            jigumiya.SetActive(true);
            StartCoroutine(ParryingCount());
        }

        private IEnumerator ParryingCount()
        {
            yield return new WaitForSeconds(parryingTime);
            jigumiya.SetActive(false);
            CanParry = false;
        }

        public IEnumerator Knockback(Vector3 dir, float duration)
        {
            EnablePhysics(false);
            Vector3 start = transform.position;
            Vector3 end = start + dir.normalized * distance;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                // 패러볼라 곡선 (에어본 연출)
                float yOffset = height * (1 - (2 * t - 1) * (2 * t - 1));

                transform.position = Vector3.Lerp(start, end, t) + Vector3.up * yOffset;
                yield return null;
            }
            EnablePhysics(true);
        }

        private void EnablePhysics(bool enabled)
        {
            enemyAI.CanBehave = enabled;
            enemyAI.enabled = enabled;
            bgAgent.enabled = enabled;
            agent.enabled = enabled;
            Debug.Log(enabled);
        }

        public void DealDamage()
        {
            //Debug.Log("호출 됨");
            // 공격 지점 설정 (attackPoint가 없으면 자신의 위치 사용)
            //Vector3 attackPos = attackPoint ? attackPoint.position : transform.position;

            // 범위 내의 히트박스 찾기
            Collider[] hitBoxes = Physics.OverlapSphere(transform.position, attackRadius, targetLayer);

            foreach (Collider hitBox in hitBoxes)
            {
                Character character = hitBox.transform.parent.GetComponent<Character>();
                //Debug.Log($"검출된 히트박스: {hitBox.name} / 부모: {hitBox.transform.parent.name}");
                if (character != null)
                {
                    character.TakeDamage(data.atk);
                }
            }
        }


        #region StatBar
        //스탯 바 UI
        private void ShowStatBar()
        {
            statBar.alpha = 1f;
            lastDamageTime = Time.time;

            //게이지 업데이트
            hpGauge.fillAmount = currentHP / maxHP;
            gpGauge.fillAmount = (float)currentGP / maxGP;

            //gp 카운트(%) 업데이트
            gpText.text = isGroggy? "" : ((float)currentGP / maxGP * 100).ToString("F0");

            // 이미 코루틴이 실행 중이면 중복 실행 방지
            if (hideCoroutine == null)
                hideCoroutine = StartCoroutine(HideStatBarAfterDelay());
        }

        private IEnumerator HideStatBarAfterDelay()
        {
            while (true)
            {
                if (IsDead)
                {
                    HideStatBar();
                    yield break;
                }
                // 5초 동안 대미지를 안 받았으면 페이드 아웃 시작
                if (Time.time - lastDamageTime >= hideDelay)
                {
                    yield return StartCoroutine(FadeOutStatBar());
                    hideCoroutine = null;
                    yield break;
                }
                yield return null;
            }
        }

        private IEnumerator FadeOutStatBar()
        {
            float timeCount = fadeDuration;
            while (timeCount > 0)
            {
                timeCount -= Time.deltaTime;
                statBar.alpha = timeCount / fadeDuration;
                yield return null;
            }

            //깔끔하게 처리
            HideStatBar();
        }
        private void HideStatBar()
        {
            statBar.alpha = 0f;
        }
        #endregion


        private void Die()
        {
            //사망처리(애니메이션, 이펙트)
            //...
            animator.SetTrigger(AnimHash.enemyDeath);
            Destroy(gameObject, 2f);    //죽는 애니메이션 길이에 따라 시간 조절

        }
        #endregion

    }

}
