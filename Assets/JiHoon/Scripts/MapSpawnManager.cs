using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 전체 맵들의 스폰을 관리하는 중앙 매니저
public class MapSpawnManager : MonoBehaviour
{
    private static MapSpawnManager instance;
    public static MapSpawnManager Instance => instance;

    [Header("Maps")]
    public MapSpawnArea[] mapSpawnAreas; // 4개의 맵

    [Header("Game Settings")]
    public bool autoStartFirstMap = true; // 첫 맵 자동 시작
    public int currentMapIndex = 0; // 현재 진행 중인 맵 인덱스

    [Header("Events")]
    public UnityEvent<int> onMapCleared; // 맵 클리어 시 이벤트
    public UnityEvent onAllMapsCleared; // 모든 맵 클리어 시 이벤트

    private HashSet<int> clearedMaps = new HashSet<int>();
    private bool isGameStarted = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 각 맵 영역 자동 찾기 (선택사항)
        if (mapSpawnAreas == null || mapSpawnAreas.Length == 0)
        {
            mapSpawnAreas = FindObjectsOfType<MapSpawnArea>();
            Debug.Log($"Auto-found {mapSpawnAreas.Length} map spawn areas");
        }

        Debug.Log($"MapSpawnManager: Found {mapSpawnAreas.Length} maps");
        for (int i = 0; i < mapSpawnAreas.Length; i++)
        {
            if (mapSpawnAreas[i] != null)
            {
                Debug.Log($"Map {i}: ID={mapSpawnAreas[i].mapID}, Position={mapSpawnAreas[i].transform.position}");
            }
        }

        // 첫 번째 맵만 자동 시작
        if (autoStartFirstMap && !isGameStarted)
        {
            StartFirstMap();
        }
    }

    // 첫 번째 맵만 시작
    public void StartFirstMap()
    {
        if (isGameStarted) return;
        isGameStarted = true;
        currentMapIndex = 0;

        if (mapSpawnAreas.Length > 0 && mapSpawnAreas[0] != null)
        {
            Debug.Log("Starting first map only");
            mapSpawnAreas[0].StartSpawning();
        }
    }

    // 다음 맵 시작 (플레이어가 이동했을 때 호출)
    public void StartNextMap()
    {
        currentMapIndex++;

        if (currentMapIndex < mapSpawnAreas.Length && mapSpawnAreas[currentMapIndex] != null)
        {
            Debug.Log($"Starting map {currentMapIndex}");
            mapSpawnAreas[currentMapIndex].StartSpawning();
        }
        else
        {
            Debug.Log("No more maps to start");
        }
    }

    // 특정 맵 시작 (맵 입구에 트리거 등으로 호출)
    public void StartMap(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex < mapSpawnAreas.Length && mapSpawnAreas[mapIndex] != null)
        {
            if (!clearedMaps.Contains(mapSpawnAreas[mapIndex].mapID))
            {
                Debug.Log($"Starting map at index {mapIndex}");
                currentMapIndex = mapIndex;
                mapSpawnAreas[mapIndex].StartSpawning();
            }
            else
            {
                Debug.Log($"Map {mapIndex} already cleared");
            }
        }
    }

    public void OnMapCleared(int mapID)
    {
        if (!clearedMaps.Contains(mapID))
        {
            clearedMaps.Add(mapID);
            onMapCleared?.Invoke(mapID);

            Debug.Log($"Map {mapID} cleared! Total cleared: {clearedMaps.Count}/{mapSpawnAreas.Length}");

            // 보상 처리 등을 여기서 할 수 있음
            HandleMapClearReward(mapID);

            // 모든 맵 클리어 체크
            if (clearedMaps.Count >= mapSpawnAreas.Length)
            {
                OnAllMapsCleared();
            }
        }
    }

    // 맵 클리어 보상 처리
    void HandleMapClearReward(int mapID)
    {
        Debug.Log($"Handling rewards for map {mapID}");
        // 여기에 보상 처리 로직 추가
        // 예: 아이템 드롭, 경험치 획득, UI 표시 등
    }

    void OnAllMapsCleared()
    {
        Debug.Log("All maps cleared! Game Complete!");
        onAllMapsCleared?.Invoke();
    }

    // 현재 진행 중인 맵 확인
    public int GetCurrentMapIndex()
    {
        return currentMapIndex;
    }

    // 특정 맵이 클리어되었는지 확인
    public bool IsMapCleared(int mapID)
    {
        return clearedMaps.Contains(mapID);
    }

    public int GetClearedMapCount()
    {
        return clearedMaps.Count;
    }

    // 전체 게임 리셋
    public void ResetGame()
    {
        clearedMaps.Clear();
        isGameStarted = false;
        currentMapIndex = 0;

        // 모든 활성 몬스터 제거
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject monster in monsters)
        {
            Destroy(monster);
        }

        // 다시 시작
        StartFirstMap();
    }

    // 특정 맵만 다시 시작
    public void RestartMap(int mapID)
    {
        MapSpawnArea targetMap = System.Array.Find(mapSpawnAreas, m => m.mapID == mapID);
        if (targetMap != null)
        {
            clearedMaps.Remove(mapID);
            targetMap.StartSpawning();
        }
    }
}