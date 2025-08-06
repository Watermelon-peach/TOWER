using System.Collections;
using TMPro;
using UnityEngine;
using Tower.Game.Data.Bless;

namespace Tower.Game.Bless
{ 
    public class Bless : MonoBehaviour
    {
        #region
        [SerializeField] private StatblessData attackData;
        [SerializeField] private StatblessData healthData;
        [SerializeField] private StatblessData shieldData;
        [SerializeField] private AbilityBlessData manaData;
        [SerializeField] private AbilityBlessData speedData;
        [SerializeField] private AbilityBlessData coolTimeData; 

        public Transform BlessEffectPos;

        public GameObject maxTierPopup;

        private bool blessing = false;
        #endregion

        #region Unity Event Method
        private void Start() 
        {
            blessing = false;
        }
        #endregion

        #region Custom Method
        public void IncreceAttack() 
        {
            if (blessing == false) 
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
            if (blessing == false) 
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
            if (blessing == false) 
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
            if (blessing == false) 
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
            if (blessing == false) 
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
            if (blessing == false) 
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
            blessing = true;
            //bless.SetActive(true);

            GameObject gotBless = Instantiate(bless, BlessEffectPos.position, Quaternion.identity);

            yield return new WaitForSeconds(2f);

            Destroy(gotBless);

            //bless.SetActive(false);
            blessing = false;
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