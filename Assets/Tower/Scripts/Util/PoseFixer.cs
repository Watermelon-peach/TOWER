using UnityEngine;

[ExecuteInEditMode]  // 에디터 모드에서도 실행되게
public class PoseFixer : MonoBehaviour
{
    public AnimationClip clip;
    [Range(0f, 1f)] public float normalizedTime = 0f;

    void Update()
    {
        if (clip != null)
        {
            clip.SampleAnimation(gameObject, clip.length * normalizedTime);
        }
    }
}
