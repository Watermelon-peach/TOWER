using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AttackVFXController : MonoBehaviour
{
    public Animator animator;
    public AttackVFXTable table;

    // 상태명 해시 → 테이블 항목
    private readonly Dictionary<int, AttackVFXTable.Entry> map = new();

    // 현재 재생 중 인스턴스 (상태 해시별로 보관)
    private class Live
    {
        public Transform spawn;
        public VisualEffect vfx;
        public ParticleSystem ps;
        public AttackVFXTable.Entry entry;
    }
    private readonly Dictionary<int, Live> liveByState = new();

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        BuildMap();
    }

    void BuildMap()
    {
        map.Clear();
        if (table == null || table.entries == null) return;

        foreach (var e in table.entries)
        {
            if (string.IsNullOrEmpty(e.stateName)) continue;
            int hash = Animator.StringToHash(e.stateName);
            map[hash] = e;
        }
    }

    Transform ResolveSpawn(string spawnPath)
    {
        if (animator == null) return transform;
        if (string.IsNullOrEmpty(spawnPath)) return animator.transform;
        var t = animator.transform.Find(spawnPath);
        return t != null ? t : animator.transform;
    }

    // --- 브릿지(StateMachineBehaviour)에서 호출 ---
    public void OnStateEnterHash(int stateHash)
    {
        if (!map.TryGetValue(stateHash, out var e)) return; // 매핑 없으면 스킵

        var spawn = ResolveSpawn(e.spawnPath);

        var live = new Live { spawn = spawn, entry = e };

        if (e.vfxPrefab != null)
        {
            var v = Instantiate(e.vfxPrefab);
            if (e.parentToSpawn) v.transform.SetParent(spawn, false);
            v.transform.localPosition = e.localPositionOffset;
            v.transform.localRotation = Quaternion.Euler(e.localEulerOffset);
            if (!e.parentToSpawn)
                v.transform.SetPositionAndRotation(
                    spawn.TransformPoint(e.localPositionOffset),
                    spawn.rotation * Quaternion.Euler(e.localEulerOffset));
            v.Play();
            live.vfx = v;
        }

        if (e.psPrefab != null)
        {
            var p = Instantiate(e.psPrefab);
            if (e.parentToSpawn) p.transform.SetParent(spawn, false);
            p.transform.localPosition = e.localPositionOffset;
            p.transform.localRotation = Quaternion.Euler(e.localEulerOffset);
            if (!e.parentToSpawn)
                p.transform.SetPositionAndRotation(
                    spawn.TransformPoint(e.localPositionOffset),
                    spawn.rotation * Quaternion.Euler(e.localEulerOffset));
            p.Play();
            live.ps = p;
        }

        liveByState[stateHash] = live;
    }

    public void OnStateExitHash(int stateHash)
    {
        if (!liveByState.TryGetValue(stateHash, out var live)) return;

        var delay = live.entry.destroyDelay;

        if (live.vfx != null)
        {
            live.vfx.Stop();
            Destroy(live.vfx.gameObject, delay);
        }
        if (live.ps != null)
        {
            live.ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(live.ps.gameObject, delay);
        }

        liveByState.Remove(stateHash);
    }
}
