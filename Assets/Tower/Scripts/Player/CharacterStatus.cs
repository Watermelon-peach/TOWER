using UnityEngine;

namespace Tower.Player
{
    public class CharacterStatus
    {
        public float currentHP;
        public float maxHP;

        public CharacterStatus(float maxHp)
        {
            maxHP = maxHp;
            currentHP = maxHp;
        }

        public bool IsDead => currentHP <= 0;
    }


}
