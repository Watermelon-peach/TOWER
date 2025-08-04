using UnityEngine;

namespace Tower.Util
{
    /// <summary>
    /// (UI 전용) 카메라 보기
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        #region Variables
        private Transform target;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            target = Camera.main.transform;
        }
        private void Update()
        {
            Vector3 dir = transform.position - target.position;
            transform.rotation = Quaternion.LookRotation(dir);
        }
        #endregion
    }

}
