using UnityEngine;

namespace Tower.Player
{
    public enum PlayerState
    {
        Idle,
        Moving,
        Attacking
    }

    public class PlayerController : MonoBehaviour
    {
        public PlayerState currentState = PlayerState.Idle;
        private PlayerMovement movement;
        private PlayerCombat combat;
        private Animator animator;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            movement = GetComponent<PlayerMovement>();
            combat = GetComponent<PlayerCombat>();
            animator = GetComponent<Animator>();
        }
        
        private void Update()
        {
            //대시
            if (InputManager.Instance.DashPressed)
            {
                movement.TryDash();
            }
            switch (currentState)
            {
                case PlayerState.Idle:
                case PlayerState.Moving:
                    HandleMovement();
                    HandleAttack();
                    break;

                case PlayerState.Attacking:
                    // 공격 중에는 이동 막기
                    HandleAttack();
                    break;
            }
            //Debug.Log(currentState.ToString());
        }

        /*private void OnEnable()
        {
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        }*/

        //실험해보니까 여기가 더 안정적인것 같음
        private void OnDisable()
        {
            transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
            currentState = PlayerState.Idle;
        }

        void HandleMovement()
        {
            Vector2 input = InputManager.Instance.MoveInput;
            if (input.magnitude > 0.1f)
            {
                currentState = PlayerState.Moving;
                movement.Move(input);
            }
            else
            {
                currentState = PlayerState.Idle;
                movement.Stop();
            }

        }

        void HandleAttack()
        {
            if (InputManager.Instance.AttackPressed)
            {
                if (currentState != PlayerState.Attacking)
                    currentState = PlayerState.Attacking;

                combat.Attack();
            }
        }
    }


}
