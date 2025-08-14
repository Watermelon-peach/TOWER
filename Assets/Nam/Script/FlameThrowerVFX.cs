using UnityEngine;
using UnityEngine.VFX;

public class FlameThrowerVFX : MonoBehaviour
{
    public Animator animator;               // 캐릭터 Animator
    public VisualEffect vfxPrefab;          // (선택) VFX Graph 프리팹
    public ParticleSystem psPrefab;         // (선택) 파티클 프리팹
    public Transform spawnPoint;

    private VisualEffect vfxInstance;
    private ParticleSystem psInstance;
    private bool effectPlaying = false;

    void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;

        if (vfxPrefab != null)
            vfxInstance = Instantiate(vfxPrefab, spawnPoint.position, spawnPoint.rotation);

        if (psPrefab != null)
            psInstance = Instantiate(psPrefab, spawnPoint.position, spawnPoint.rotation);

        StopEffect();
    }

    void Update()
    {
        bool isAttack3 = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack3");

        if (isAttack3 && !effectPlaying)
        {
            StartEffect();
        }
        else if (!isAttack3 && effectPlaying)
        {
            StopEffect();
        }
    }

    void StartEffect()
    {
        effectPlaying = true;

        if (vfxInstance != null)
        {
            vfxInstance.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            vfxInstance.Play();
        }

        if (psInstance != null)
        {
            psInstance.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            psInstance.Play();
        }
    }

    void StopEffect()
    {
        effectPlaying = false;

        if (vfxInstance != null)
            vfxInstance.Stop();

        if (psInstance != null)
            psInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}