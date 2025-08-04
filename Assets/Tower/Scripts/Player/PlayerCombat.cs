using UnityEngine;

namespace Tower.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        #region Variables
        private Animator animator;
        #endregion


        #region unity Event Method
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }
        #endregion

        #region Custom Method
        public void Attack()
        {
            animator.SetTrigger(AnimHash.attack);
            //Debug.Log("Attack!");
        }
        #endregion


    }
}