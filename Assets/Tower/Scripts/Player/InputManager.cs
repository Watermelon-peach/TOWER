using UnityEngine;
using Tower.Util;

namespace Tower.Player
{
    public class InputManager : Singleton<InputManager>
    {
        #region Singleton
        protected override void Awake()
        {
            base.Awake();
        }
        #endregion


        #region Variables
        public Vector2 MoveInput { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool DashPressed { get; private set; }
        public bool SwapPressed { get; private set; }
        #endregion

        #region Unity Event Method
        void Update()
        {
            MoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            AttackPressed = Input.GetButtonDown("Fire1");
            DashPressed = Input.GetButtonDown("Fire2");
            SwapPressed = Input.GetButtonDown("Jump");
            if (SwapPressed)
            {
                Debug.Log("Jump");
            }
        }
        #endregion
    }

}
