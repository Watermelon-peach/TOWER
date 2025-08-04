using UnityEngine;

namespace Tower.Player.Data
{
    public enum CharacterType
    {
        Dealer,
        Tanker,
        Supporter
    }
    /// <summary>
    /// 캐릭터 기본 정보를 담고 있는 스크립터블 오브젝트
    /// </summary>
    [CreateAssetMenu(fileName = "New CharacterData", menuName = "Character/CharacterData")]
    public class CharacterBaseSO : ScriptableObject
    {
        #region Variables
        [Header("Basic Info")]
        public float atk;
        public float def;

        public float maxHp;
        public float maxMp;

        public CharacterType characterType;
        public Sprite characterIcon;

        [Header("Skill Info")]
        public Sprite skillIcon;
        public float manaCost;
        public float coolDown;
        public float ultRecoverRate;
        public float manaRecoverRate;
        #endregion
    }

}
