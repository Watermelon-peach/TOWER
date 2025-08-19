using UnityEngine;

public class VfxManager : MonoBehaviour
{
    [Header("References")]
    public VfxPool pool;

    [Header("Defaults")]
    public Transform defaultAttach;    // 기본 부착(예: 캐릭터 루트/손뼈)
    public bool followInWorldSpace = false;
    public Vector3 defaultOffset = Vector3.zero;
    public float defaultLifetime = 2f; // 0 이하이면 자동 종료까지

    // 외부에서 호출: 애니메이션 이벤트로 key만 넘겨도 됨
    public void Play(string key)
    {
        Play(key, defaultAttach, defaultOffset, defaultLifetime);
    }

    public void Play(string key, Transform attachTo, Vector3 offset, float lifetime)
    {
        if (pool == null || !pool.HasKey(key))
        {
            Debug.LogWarning($"[VfxManager] No VFX key: {key}");
            return;
        }

        var inst = pool.Spawn(key);
        if (inst == null) return;

        inst.Play(
            attachTo,
            followInWorldSpace,
            offset,
            lifetime,
            (finished) => pool.Despawn(key, finished)
        );
    }

    // 샘플: 우클릭으로 테스트 재생
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // 예시: "LevelUp" 키를 기본 위치에 재생
            Play("LevelUp");
        }
    }

    // 애니메이션 이벤트에 직접 연결해서 사용 가능:
    // Animator 이벤트창에서 함수 이름 "PlayAnimEvent"로 key 전달
    public void PlayAnimEvent(string key)
    {
        Play(key);
    }
}
