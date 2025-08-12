using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tower.Util
{
    public class GrayscaleEffect : MonoBehaviour
    {
        public Volume globalVolume;
        private ColorAdjustments colorAdjustments;
        private bool toggle = false;

        private void Start()
        {
            /*if (globalVolume.profile.TryGet(out colorAdjustments))
            {
                // 기본적으로 꺼놓고 시작
                colorAdjustments.active = false;
            }*/

            globalVolume.enabled = false;
        }

        private void Update()
        {
            //TODO : 테스트용 치트
            if (Input.GetKeyDown(KeyCode.M))
            {
                Toggle();
            }
        }
        public void Toggle()
        {
            toggle = !toggle;
            SetGrayscale(toggle);
        }

        public void SetGrayscale(bool enable)
        {
            globalVolume.enabled = enable;
        }
    }

}
