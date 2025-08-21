using UnityEngine;

namespace Tower.Player
{
    public class HealerFairy : MonoBehaviour
    {
        #region Variables
        [SerializeField] private float buffMultiplier = 1.5f;
        private Character character;
        #endregion

        #region Unity Event Method
        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("HitBox"))
            {
                character = other.transform.parent.GetComponent<Character>();
                character.AtkBuff = buffMultiplier;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("HitBox"))
            {
                character = other.transform.parent.GetComponent<Character>();
                character.AtkBuff = 1;      //정상화
            }
        }
        #endregion
    }

}
