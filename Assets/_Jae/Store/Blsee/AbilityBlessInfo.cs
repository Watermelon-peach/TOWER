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
        blssTierText.text = /*플레이어의 현제 티어*/abilityBlessData.nowTier + " Tier / " + abilityBlessData.maxTier + " Tier";

        blessPriceText.text = abilityBlessData.Price + " Stone";
        }
    }
}