using System.Collections;
using TMPro;
using UnityEngine;
using Tower.Game.Data.Bless;
using Tower.Player;

namespace Tower.Game.Bless
{ 
    public class BlessManager : MonoBehaviour
    {
        #region Variables
        [SerializeField] private StatblessData attackData;
        [SerializeField] private StatblessData healthData;
        [SerializeField] private StatblessData shieldData;
        [SerializeField] private AbilityBlessData manaData;
        [SerializeField] private AbilityBlessData speedData;
        [SerializeField] private AbilityBlessData coolTimeData; 

        public GameObject maxTierPopup;

        private bool nowBlessing = false;
        
        private Character activeCharacter;
        #endregion

        #region Property
        public bool NowBlessing => nowBlessing;
        #endregion

        #region Unity Event Method
        private void Start()
        {
            nowBlessing = false;
        }
        #endregion

        #region Custom Method
        public void IncreceAttack() 
        {
            if (nowBlessing == false)
            {
                if (attackData.nowTier < attackData.maxblessTier)
                {
                    StartCoroutine(Blessing(attackData.blessEffect));
                    attackData.nowTier += 1;
                }
                else
                {
                    StartCoroutine(maxTierPopUp());
                }
            }
        }

        public void IncreceHealth() 
        {
            Debug.Log("123");
            if (nowBlessing == false)
            {
                if (healthData.nowTier < healthData.maxblessTier)
                {
                    StartCoroutine(Blessing(healthData.blessEffect));
                    healthData.nowTier += 1;
                }
                else
                {
                    StartCoroutine(maxTierPopUp());
                }
            }
        }

        public void IncreceShield() 
        {
            if (nowBlessing == false) 
            {
                if (shieldData.nowTier < shieldData.maxblessTier) 
                {
                    StartCoroutine(Blessing(shieldData.blessEffect));
                    shieldData.nowTier += 1;
                } 
                else 
                {
                    StartCoroutine(maxTierPopUp());
                }
            }
        }

        public void IncreceMana() 
        {
            if (nowBlessing == false) 
            {
                if (manaData.nowTier < manaData.maxTier) 
                {
                    StartCoroutine(Blessing(manaData.blessEffect));
                    manaData.nowTier += 1;
                }
                else 
                {
                    StartCoroutine(maxTierPopUp());
                }
            }
        }

        public void IncreceSpeed() 
        {
            if (nowBlessing == false) 
            {
                if (speedData.nowTier < speedData.maxTier) 
                {
                    StartCoroutine(Blessing(speedData.blessEffect));
                    speedData.nowTier += 1;
                } 
                else 
                {
                    StartCoroutine(maxTierPopUp());
                }
            }
        }

        public void IncreceCoolTime() 
        {
            if (nowBlessing == false) 
            {
                if (coolTimeData.nowTier < coolTimeData.maxTier) 
                {
                    StartCoroutine(Blessing(coolTimeData.blessEffect));
                    coolTimeData.nowTier += 1;
                } 
                else 
                {
                    StartCoroutine(maxTierPopUp());
                }
            }
        }



        IEnumerator Blessing(GameObject bless) 
        {
            nowBlessing = true;
            //bless.SetActive(true);

            activeCharacter = TeamManager.Instance.characters[TeamManager.Instance.CurrentIndex];
            // Vector3 targetPos = storeCam.transform.position;
            // // 높이 무시 (Y값을 activeCharacter 위치와 동일하게)
            // targetPos.y = activeCharacter.transform.position.y;

            Vector3 BlessPos = new Vector3(activeCharacter.transform.position.x, 0.03f, activeCharacter.transform.position.z);

            //이펙트 생성
            GameObject gotBless = Instantiate(bless, BlessPos, Quaternion.identity);
            
            // Debug.Log(gotBless.transform.position);
            // Debug.Log(activeCharacter.transform.position);
            //이펙트 방향 맞추기
            // gotBless.transform.LookAt(targetPos);

            yield return new WaitForSeconds(2f);
            //이펙트 제거
            Destroy(gotBless);

            //bless.SetActive(false);
            nowBlessing = false;
        }

        IEnumerator maxTierPopUp() 
        {
            maxTierPopup.SetActive(true);

            yield return new WaitForSeconds(1.5f);

            maxTierPopup.SetActive(false);
        }
        #endregion
    }
}