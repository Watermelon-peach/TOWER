using UnityEngine;

public class SetCameraTargetDisplay : MonoBehaviour
{
    public Camera targetCam;     // Inspector에서 연결
    public int displayIndex = 1; // 0 = Display 1, 1 = Display 2

    void Start()
    {
        if (targetCam != null)
            targetCam.targetDisplay = displayIndex;

        // 해당 디스플레이 활성화
        if (Display.displays.Length > displayIndex)
            Display.displays[displayIndex].Activate();
    }
}
