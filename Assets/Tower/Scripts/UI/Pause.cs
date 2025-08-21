using Tower.Player;
using UnityEngine;

namespace Tower.UI
{
    public class Pause : MonoBehaviour
    {
        #region Variables
        public GameObject pauseCanvas;
        #endregion

        #region Unity Event Method
        private void Update()
        {
            if (InputManager.Instance.PausePressed && !TeamManager.Instance.IsSomeoneParrying)
            {
                Toggle();
            }
        }
        #endregion

        #region Custom Method
        private void Toggle()
        {
            pauseCanvas.SetActive(!pauseCanvas.activeSelf);
            Time.timeScale = pauseCanvas.activeSelf ? 0 : 1;
        }
        #endregion

    }
}
