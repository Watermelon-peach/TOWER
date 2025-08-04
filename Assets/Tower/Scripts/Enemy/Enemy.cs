using UnityEngine;
using UnityEngine.UI;

using Tower.Enemy.Data;
using Tower.Game;
using System.Collections;

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

        private float currentHP;    //현재 체력
        private float maxHP;        //최대 체력 (db 기반)
        private float currentGP;    //현재 그로기 포인트
        private float maxGP;        //최대 그로기 포인트 (db 기반)

        private bool isGroggy;
        [SerializeField] private float groggyDuration = 10f;

        //스탯 바 관련 변수들
        private float hideDelay = 5f;
        [SerializeField] private float fadeDuration = 1f;
        private Coroutine hideCoroutine;
        private float lastDamageTime;

        #endregion

        #region Property
        public bool IsDead => currentHP <= 0;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            //값 설정
            maxHP = data.maxHp;
            maxGP = data.maxGp;
        }

        private void Start()
        {
            //초기화
            currentHP = maxHP;
            currentGP = 0f;
            HideStatBar();
        }

        private void Update()
        {
            //TODO : UI 테스트
            if (Input.GetKeyDown(KeyCode.M))
            {
                TakeDamage(10, 10);
            }
        }
        #endregion

        #region Custom Method
        public void TakeDamage(float damage, float groggyAmount = 0)
        {
            if (IsDead) return; //중첩사망처리 방지

            damage = Mathf.Max(damage * (100f / (100f + data.def)), 1f);
            Debug.Log("방어력 적용 대미지: " + damage);
            currentGP += groggyAmount;

            if (currentGP >= maxGP)
            {
                currentGP = maxGP;
                Debug.Log("그로기 상태 진입!!");


            }
            Debug.Log("현재 GP: " + currentGP);
            //그로기 약체화 배율 처리
            if (isGroggy)
                damage *= 1.5f;

            currentHP = Mathf.Max(currentHP - damage, 0);

            ShowStatBar();

            if (IsDead)
            {
                Die();
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
            gpGauge.fillAmount = currentGP / maxGP;

            // 이미 코루틴이 실행 중이면 중복 실행 방지
            if (hideCoroutine == null)
                hideCoroutine = StartCoroutine(HideStatBarAfterDelay());
        }

        private IEnumerator HideStatBarAfterDelay()
        {
            while (true)
            {
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
            Destroy(gameObject, 2f);

        }
        #endregion

    }

}
