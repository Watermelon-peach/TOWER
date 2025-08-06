using UnityEngine;

namespace Tower.Player
{
    /// <summary>
    /// 애니메이션 파라미터 해시 모음
    /// </summary>
    public static class AnimHash
    {
        public static readonly int attack = Animator.StringToHash("Attack");
        public static readonly int moveSpeed = Animator.StringToHash("MoveSpeed");
        public static readonly int isMoving = Animator.StringToHash("IsMoving");
        public static readonly int dash = Animator.StringToHash("Dash");
        public static readonly int groggy = Animator.StringToHash("Groggy");
        public static readonly int endGroggy = Animator.StringToHash("EndGroggy");
        public static readonly int hit = Animator.StringToHash("Hit");
        public static readonly int isDead = Animator.StringToHash("IsDead");
    }

}
