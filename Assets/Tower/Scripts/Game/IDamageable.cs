using UnityEngine;

namespace Tower.Game
{
    /// <summary>
    /// 대미지 처리가 가능한 오브젝트에 붙는 인터페이스
    /// </summary>
    public interface IDamageable
    {
        #region Property
        bool IsDead { get; }
        #endregion
        #region Custom Method
        void TakeDamage(float damage, int groggyAmount);
        #endregion
    }

}
