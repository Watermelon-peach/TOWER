using UnityEngine;
using UnityEngine.InputSystem;

namespace Sample
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 5f;

        private CharacterController controller;
        private Vector3 velocity;
        private Vector2 moveInput;
        private bool isGrounded;

        [Header("Ground Check")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundCheckDistance = 0.4f;

        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            // 지면 체크
            isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // 이동 처리
            Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
            controller.Move(move * moveSpeed * Time.deltaTime);

            
        }

        // Input System 콜백 메서드들
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }
        }

    }
}