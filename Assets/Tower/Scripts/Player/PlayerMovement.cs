using System.Collections;
using UnityEngine;

namespace Tower.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        #region Variables
        private CharacterController controller;
        private Animator animator;

        public float currentSpeed = 0f;
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float acceleration = 10f;
        [Header("Dash")]
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float dashDuration = 0.5f;
        [SerializeField] private float dashCoolDown = 3f;
        private float dashCount = 0f;
        private bool isDashing = false;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
        }
        private void OnDisable()
        {
            //캐릭터 교체시 대시 막힘 버그 방지
            isDashing = false;
        }
        #endregion

        #region Custom Method
        public void Move(Vector2 input)
        {
            // 이동 애니메이션 활성화
            animator.SetBool(AnimHash.isMoving, true);

            // 목표 속도 계산
            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            // Blend Tree 파라미터 업데이트
            animator.SetFloat(AnimHash.moveSpeed, Mathf.InverseLerp(walkSpeed, runSpeed, currentSpeed));

            // 카메라 기준 방향 벡터 계산
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            // 입력을 카메라 기준으로 변환
            Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;

            // 이동 적용
            if (moveDir.sqrMagnitude > 0.001f)
            {
                controller.Move(moveDir * currentSpeed * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(moveDir);
            }
        }

        public void Stop()
        {
            // 이동 정지 시 애니메이션 초기화
            animator.SetBool(AnimHash.isMoving, false);
            currentSpeed = 0f;
        }

        public void TryDash()
        {
            if (isDashing) return;
            StartCoroutine(Dash());
        }
        private IEnumerator Dash()
        {
            isDashing = true;
            animator.SetTrigger(AnimHash.dash);
            while (dashCount <= dashDuration)
            {
                dashCount += Time.deltaTime;
                controller.Move(controller.transform.forward * dashSpeed * Time.deltaTime);
                yield return null;
            }
            dashCount = 0;

            yield return new WaitForSeconds(dashCoolDown - dashDuration);
            isDashing = false;

            //회피 처리 구현
            //주변 일정 범위만큼 캐스팅 해서 CanParry가 true인 적이 있을 시 Parrymode에 진입한다.
            //...
        }
        #endregion
    }

}
