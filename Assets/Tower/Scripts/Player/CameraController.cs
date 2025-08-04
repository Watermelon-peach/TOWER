using Unity.Cinemachine;
using UnityEngine;

namespace Tower.Player
{
    public class CameraController : MonoBehaviour
    {
        #region Variables
        public CinemachineCamera freelookCamera;
        public Transform[] characterOffsets = new Transform[3];
        #endregion

        #region Unity Event Method

        #endregion

        #region Custom Method
        public void LookAtCharacter(int index)
        {
            var target = freelookCamera.Target;
            target.TrackingTarget = characterOffsets[index];
            freelookCamera.Target = target;
            freelookCamera.UpdateTargetCache();
            Debug.Log($"{index}번 캐릭터로 카메라 전환 !");
        }

        //전투 상황 시
        public void LookAtEnemy()
        {

        }
        #endregion

    }

}
