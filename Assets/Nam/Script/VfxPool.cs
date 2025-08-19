using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VfxEntry
{
    public string key;                 // "Attack1", "LevelUp" 등
    public GameObject prefab;          // PooledVfx가 붙은 프리팹
    public int initialPoolSize = 3;
}

public class VfxPool : MonoBehaviour
{
    [Header("Library")]
    public VfxEntry[] entries;

    private readonly Dictionary<string, Queue<PooledVfx>> pools = new();
    private readonly Dictionary<string, GameObject> prefabMap = new();

    private void Awake()
    {
        foreach (var e in entries)
        {
            if (e == null || e.prefab == null || string.IsNullOrEmpty(e.key)) continue;
            if (!prefabMap.ContainsKey(e.key)) prefabMap.Add(e.key, e.prefab);

            var q = new Queue<PooledVfx>(Mathf.Max(1, e.initialPoolSize));
            pools[e.key] = q;

            for (int i = 0; i < e.initialPoolSize; i++)
            {
                q.Enqueue(CreateInstance(e.key));
            }
        }
    }

    private PooledVfx CreateInstance(string key)
    {
        var prefab = prefabMap[key];
        var go = Instantiate(prefab, transform);
        go.name = $"VFX_{key}";
        var pooled = go.GetComponent<PooledVfx>();
        if (!pooled)
        {
            pooled = go.AddComponent<PooledVfx>(); // 안전장치
        }
        go.SetActive(false);
        return pooled;
    }

    public PooledVfx Spawn(string key)
    {
        if (!pools.TryGetValue(key, out var q))
        {
            Debug.LogWarning($"[VfxPool] Unknown key: {key}");
            return null;
        }

        if (q.Count == 0)
        {
            // 필요 시 동적 확장
            q.Enqueue(CreateInstance(key));
        }
        return q.Dequeue();
    }

    public void Despawn(string key, PooledVfx obj)
    {
        if (!pools.ContainsKey(key))
        {
            Destroy(obj.gameObject);
            return;
        }
        obj.transform.SetParent(transform, false);
        pools[key].Enqueue(obj);
    }

    public bool HasKey(string key) => prefabMap.ContainsKey(key);
}
