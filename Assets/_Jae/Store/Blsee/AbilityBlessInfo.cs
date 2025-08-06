using TMPro;
using UnityEngine;
using Tower.Game.Data.Bless;

namespace Tower.Game.Bless
{
    public class AbilityBlessInfo : MonoBehaviour
    {
        #region Variables
        public TextMeshProUGUI blssTierText;
        public TextMeshProUGUI blessPriceText;

        public AbilityBlessData abilityBlessData;
        #endregion

    private void Update() 
        {
        blssTierText.text = /*�÷��̾��� ���� Ƽ��*/abilityBlessData.nowTier + " Tier / " + abilityBlessData.maxTier + " Tier";

        blessPriceText.text = abilityBlessData.Price + " Stone";
        }
    }
}