using UnityEngine;

namespace Tower.Player
{
    public class Dealer : Character
    {
        #region Variables
        private float skillCoolRemain; //남은 쿨타임
        #endregion

        #region Property
        public float SkillCoolRemain => skillCoolRemain;
        #endregion

    }

}
