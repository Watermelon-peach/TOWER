using UnityEngine;
using TMPro;
using Tower.Game.Data.Bless;

public class StatBlessInfo : MonoBehaviour
{
    #region Variables
    public TextMeshProUGUI blssTierText;
    public TextMeshProUGUI blessPriceText;

    public StatblessData statblessData;
    #endregion

    private void Update() 
    {
        blssTierText.text = /*�÷��̾��� ���� Ƽ��*/statblessData.nowTier + " Tier / " + statblessData.maxblessTier + "Tier";

        blessPriceText.text = statblessData.Price(statblessData.nowTier) + "Stone";
    }
}
