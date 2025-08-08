using UnityEngine;
using Tower.Player;

namespace Tower.Game.Bless
{
    public class StoreAreaCheck : MonoBehaviour
    {
        #region Variables

        private Character activeCharacter;
        private Character previousCharacter;
        private bool isPlayerInRange;

        public bool IsPlayerInRange => isPlayerInRange;
        #endregion

        #region Unity Event Method
        private void Update()
        {
            activeCharacter = TeamManager.Instance.characters[TeamManager.Instance.CurrentIndex];

            if (previousCharacter != null && activeCharacter != previousCharacter)
            {
                isPlayerInRange = false;
            }

            previousCharacter = activeCharacter;
        }
        #endregion

        #region Custom Method
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == TeamManager.Instance.characters[TeamManager.Instance.CurrentIndex].gameObject)
            {
                isPlayerInRange = true;
                Debug.Log(isPlayerInRange);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == TeamManager.Instance.characters[TeamManager.Instance.CurrentIndex].gameObject)
            {
                isPlayerInRange = false;
                Debug.Log(isPlayerInRange);
            }
        }
        #endregion
    }
}