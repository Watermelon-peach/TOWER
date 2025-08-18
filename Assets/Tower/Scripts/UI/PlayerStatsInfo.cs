using System;
using UnityEngine;
using UnityEngine.UI;
using Tower.Player;
using Tower.Util;

namespace Tower.UI
{
    [Serializable]
    public struct CharacterInfoUI
    {
        public Image characterIcon;
        public Image HPGauge;
        public Image MPGauge;
    }
    public class PlayerStatsInfo : Singleton<PlayerStatsInfo>
    {
        #region Variables
        public CharacterInfoUI[] infoUIs = new CharacterInfoUI[3];

        private Character currentCharacter;
        #endregion

        #region Unity Event Method
        protected override void Awake()
        {
            //싱글톤 불러오기
            base.Awake();
        }
        #endregion

        #region Custom Method
        public void SwitchCharatersInfo()
        {
            //Debug.Log("캐릭캐릭체인지!");
            currentCharacter = TeamManager.Instance.characters[TeamManager.Instance.CurrentIndex];
            for (int i = 0; i < infoUIs.Length; i++)
            {
                //캐릭터 인덱스에 맞는 UI 슬롯 할당
                int characterIndex = (TeamManager.Instance.CurrentIndex + i) % 3;
                Character character = TeamManager.Instance.characters[characterIndex];
                infoUIs[i].characterIcon.sprite = character.characterBase.characterIcon;
                //사망한 캐릭터 아이콘 색상 처리
                infoUIs[i].characterIcon.color = character.IsDead? new Color32(80, 80, 80, 255) : Color.white;
                infoUIs[i].HPGauge.fillAmount = character.CurrentHP / character.characterBase.maxHp;
                infoUIs[i].MPGauge.fillAmount = character.CurrentMP / character.characterBase.maxMp;
            }
        }

        //현재 선택한 캐릭터 HP정보 업데이트
        public void UpdateCurrentHPInfo()
        {
            infoUIs[0].HPGauge.fillAmount = currentCharacter.CurrentHP / currentCharacter.characterBase.maxHp;
            infoUIs[0].characterIcon.color = currentCharacter.IsDead ? new Color32(80, 80, 80, 255) : Color.white;
        }

        //현재 선택한 캐릭터 MP정보 업데이트
        public void UpdateCurrentMPInfo()
        {
            infoUIs[0].MPGauge.fillAmount = currentCharacter.CurrentMP / currentCharacter.characterBase.maxMp;
        }
        #endregion
    }

}
