using UnityEngine;
using Tower.Game;
using System.Collections;
using Tower.Player;

namespace Tower.Game
{
    public class MastManager : MonoBehaviour
    {
        [Header("Dungeon Data")]
        public DungeonDataSO dungeonData;

        [Header("Current Mast")]
        public int currentMastIndex = 0;
        public GameObject currentMastInstance;

        [Header("SafetyZone Settings")]  // ⭐ 헤더 이름 변경
        public GameObject safetyZoneObject;  // ⭐ Scene의 SafetyZone 참조
        public Vector3 safetyZoneOffset = new Vector3(0, 0, 50);  // ⭐ 4층에서 떨어진 거리

        [Header("Debug")]
        public bool showDebugLogs = true;

        private static MastManager instance;
        public static MastManager Instance => instance;

        void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);

            // ⭐ Scene에서 SafetyZone 자동 찾기
            if (safetyZoneObject == null)
            {
                safetyZoneObject = GameObject.Find("SafetyZone");
            }
        }

        void Start()
        {
            // 게임 시작 시 첫 번째 마스트 로드
            LoadMast(0);
        }

        public void LoadMast(int mastIndex)
        {
            if (dungeonData == null || dungeonData.mastList == null)
            {
                Debug.LogError("DungeonData or MastList is null!");
                return;
            }

            if (mastIndex >= dungeonData.mastList.Count)
            {
                Debug.Log("All masts completed! Game Clear!");
                // TODO: 게임 클리어 처리
                return;
            }

            // 기존 마스트 제거
            if (currentMastInstance != null)
            {
                Destroy(currentMastInstance);
                // 메모리 정리
                Resources.UnloadUnusedAssets();
            }

            // 새 마스트 생성
            MastDataSO mastData = dungeonData.mastList[mastIndex];
            if (mastData == null || mastData.mastPrefab == null)
            {
                Debug.LogError($"MastData or MastPrefab is null for index {mastIndex}!");
                return;
            }

            currentMastInstance = Instantiate(mastData.mastPrefab);
            currentMastInstance.name = $"Mast_{mastIndex}_{mastData.mastName}";
            currentMastIndex = mastIndex;

            // MapSpawnArea들을 MapSpawnManager에 등록
            RegisterSpawnAreas();

            //// ⭐ SafetyZone을 현재 마스트 끝에 배치
            //PositionSafetyZone();

            // 팀을 마스트 시작 위치로 이동
            MoveTeamToMastStart();

            // 약간의 딜레이 후 첫 번째 맵 시작
            StartCoroutine(DelayedStartFirstMap());

            if (showDebugLogs)
                Debug.Log($"[MastManager] Loaded Mast {mastIndex}: {mastData.mastName}");
        }

        /// <summary>
        /// ⭐ SafetyZone으로 팀 이동 (4층 클리어 후 호출)
        /// </summary>
        public void MoveTeamToSafetyZone()
        {
            if (safetyZoneObject == null || TeamManager.Instance == null) return;

            Transform safetyStart = safetyZoneObject.transform.Find("StartPoint");
            if (safetyStart == null)
                safetyStart = safetyZoneObject.transform;

            TeamManager.Instance.MoveFormation(safetyStart.position, safetyStart.rotation);

            if (showDebugLogs)
                Debug.Log("[MastManager] Moved team to SafetyZone");
        }

        // ... 나머지 기존 메서드들은 그대로 ...

        private void RegisterSpawnAreas()
        {
            if (MapSpawnManager.Instance == null)
            {
                Debug.LogError("MapSpawnManager Instance not found!");
                return;
            }

            // 현재 마스트에서 모든 MapSpawnArea 찾기
            MapSpawnArea[] areas = currentMastInstance.GetComponentsInChildren<MapSpawnArea>();

            // MapSpawnManager에 등록
            MapSpawnManager.Instance.mapSpawnAreas = areas;

            // MapSpawnManager 초기화
            MapSpawnManager.Instance.clearedMaps.Clear();
            MapSpawnManager.Instance.currentMapIndex = 0;

            if (showDebugLogs)
                Debug.Log($"[MastManager] Registered {areas.Length} spawn areas");
        }

        private void MoveTeamToMastStart()
        {
            if (TeamManager.Instance == null)
            {
                Debug.LogError("TeamManager Instance not found!");
                return;
            }

            Transform startPoint = FindStartPointInMast();

            if (startPoint != null)
            {
                TeamManager.Instance.MoveFormation(startPoint.position, startPoint.rotation);

                if (showDebugLogs)
                    Debug.Log($"[MastManager] Moved team to: {startPoint.position}");
            }
            else
            {
                Debug.LogWarning("No start point found in mast! Using default position.");
                TeamManager.Instance.MoveFormation(Vector3.zero, Quaternion.identity);
            }
        }

        private Transform FindStartPointInMast()
        {
            if (currentMastInstance == null) return null;

            string[] possiblePaths = new string[]
            {
                "01_SkeletonMap/SkeletonMap/StartPoint00",
                "01_SkeletonMap/StartPoint00",
                
            };

            foreach (string path in possiblePaths)
            {
                Transform point = currentMastInstance.transform.Find(path);
                if (point != null)
                {
                    if (showDebugLogs)
                        Debug.Log($"[MastManager] Found start point at: {path}");
                    return point;
                }
            }

            Transform[] allChildren = currentMastInstance.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.name.Contains("StartPoint") || child.name.Contains("SpawnPoint"))
                {
                    if (showDebugLogs)
                        Debug.Log($"[MastManager] Found start point by name: {child.name}");
                    return child;
                }
            }

            Transform firstFloor = currentMastInstance.transform.GetChild(0);
            if (firstFloor != null)
            {
                if (showDebugLogs)
                    Debug.Log($"[MastManager] Using first floor as start point");
                return firstFloor;
            }

            return null;
        }

        IEnumerator DelayedStartFirstMap()
        {
            yield return new WaitForSeconds(0.5f);

            if (MapSpawnManager.Instance != null)
            {
                MapSpawnManager.Instance.StartFirstMap();
            }

            // ⭐ 첫 번째 마스트일 때만 타이머 시작
            if (currentMastIndex == 0 && StageTimer.Instance != null)
            {
                StageTimer.Instance.StartStageTimer(0);
                //Debug.Log("[MastManager] Started stage timer for first mast");
            }
        }

        public void LoadNextMast()
        {
            if (showDebugLogs)
                Debug.Log($"[MastManager] Loading next mast from {currentMastIndex} to {currentMastIndex + 1}");

            LoadMast(currentMastIndex + 1);
        }

        public void ResetToFirstMast()
        {
            if (showDebugLogs)
                Debug.Log("[MastManager] Resetting to first mast");

            var mapSpawnManager = MapSpawnManager.Instance;
            mapSpawnManager.ResetGame();

            currentMastIndex = 0;
            LoadMast(0);
        }

        public bool IsLastMast()
        {
            return currentMastIndex >= dungeonData.mastList.Count - 1;
        }

        public void MoveTeamToFloor(int floorIndex)
        {
            if (currentMastInstance == null || TeamManager.Instance == null) return;

            Transform floorStart = currentMastInstance.transform.Find($"{floorIndex:00}_*/*/StartPoint*");
            if (floorStart != null)
            {
                TeamManager.Instance.MoveFormation(floorStart.position, floorStart.rotation);

                if (showDebugLogs)
                    Debug.Log($"[MastManager] Moved team to floor {floorIndex}");
            }
        }
    }
}