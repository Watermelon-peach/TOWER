using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Tower.Game;
using Tower.UI;

namespace Tower.Game
{
    // 전체 맵들의 스폰을 관리하는 중앙 매니저
    public class MapSpawnManager : MonoBehaviour
    {

        // 초기 위치 저장용 Dictionary 추가
        private Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
        private Dictionary<GameObject, Quaternion> initialRotations = new Dictionary<GameObject, Quaternion>();


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

        public HashSet<int> clearedMaps = new HashSet<int>();
        private bool isGameStarted = false;

        public bool IsGameStarted { get; set; } = false; // 게임 시작 여부

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
            // 게임 시작 시 모든 플레이어의 초기 위치 저장
            SaveInitialPlayerPositions();

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
                StartCoroutine(DelayedStart());
            }
        }

        // 게임 시작 딜레이 (초기화용)
        IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(0.1f); // 모든 오브젝트가 준비될 때까지 대기
            StartFirstMap();
        }

        // 첫 번째 맵만 시작
        public void StartFirstMap()
        {
            if (isGameStarted) return;
            isGameStarted = true;
            currentMapIndex = 0;

            // StageTimer 시작
            if (StageTimer.Instance != null)
            {
                StageTimer.Instance.StartStageTimer(0);
                Debug.Log("[MapSpawnManager] Stage timer started for first map");
            }
            else
            {
                Debug.LogWarning("[MapSpawnManager] StageTimer Instance not found!");
            }

            // 첫 번째 맵 시작 시간 기록
            if (CardRewardUI.Instance != null)
            {
                CardRewardUI.Instance.OnMapStart(0);
                Debug.Log("[MapSpawnManager] First map time record started");
            }

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
        public void StartMap(int mapIndex)  // 파라미터 이름은 mapIndex지만 실제로는 mapID로 사용
        {
            // 맵 시작 시간 기록
            if (CardRewardUI.Instance != null)
            {
                CardRewardUI.Instance.OnMapStart(mapIndex);
                Debug.Log($"[MapSpawnManager] Map {mapIndex} timer started");
            }

            // ⭐ mapID로 MapSpawnArea 찾기 (배열 인덱스가 아님!)
            MapSpawnArea targetArea = null;
            int targetIndex = -1;

            for (int i = 0; i < mapSpawnAreas.Length; i++)
            {
                if (mapSpawnAreas[i] != null && mapSpawnAreas[i].mapID == mapIndex)
                {
                    targetArea = mapSpawnAreas[i];
                    targetIndex = i;
                    break;
                }
            }

            if (targetArea != null)
            {
                // SafetyZone 체크
                if (targetArea.spawnConfig == null)
                {
                    Debug.Log($"Map {mapIndex} is SafetyZone, skipping spawn");
                    return;
                }

                if (!clearedMaps.Contains(targetArea.mapID))
                {
                    Debug.Log($"Starting map with ID {mapIndex} (array index: {targetIndex})");
                    currentMapIndex = targetIndex;  // 배열 인덱스 저장
                    targetArea.StartSpawning();
                }
                else
                {
                    Debug.Log($"Map {mapIndex} already cleared");
                }
            }
            else
            {
                Debug.LogWarning($"[MapSpawnManager] No map found with ID {mapIndex}!");
            }
        }

        // 맵 클리어 처리
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

        // 모든 맵 클리어 시
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

        // 클리어한 맵 개수 반환
        public int GetClearedMapCount()
        {
            return clearedMaps.Count;
        }

        void SaveInitialPlayerPositions()
        {
            Sample.PlayerMovement[] allPlayers = FindObjectsOfType<Sample.PlayerMovement>(true);

            foreach (var player in allPlayers)
            {
                GameObject playerObj = player.gameObject;
                initialPositions[playerObj] = playerObj.transform.position;
                initialRotations[playerObj] = playerObj.transform.rotation;

                Debug.Log($"[MapSpawnManager] Saved initial position for {playerObj.name}: {playerObj.transform.position}");
            }
        }

        // 전체 게임 리셋
        public void ResetGame()
        {
            Debug.Log("[MapSpawnManager] Full game reset initiated");

            // 1. 게임 상태 초기화
            clearedMaps.Clear();
            isGameStarted = false;
            currentMapIndex = 0;

            // 2. 모든 활성 몬스터 제거
            GameObject[] monsters = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject monster in monsters)
            {
                Destroy(monster);
            }

            // 3. 모든 맵 영역 리셋
            foreach (var area in mapSpawnAreas)
            {
                if (area != null)
                {
                    area.ResetArea();  // 각 영역의 스폰 상태 초기화
                }
            }

            // 4. 플레이어들을 초기 위치로 리셋
            Tower.Player.PlayerMovement[] allPlayers = FindObjectsOfType<Tower.Player.PlayerMovement>(true);

            foreach (var player in allPlayers)
            {
                // 오브젝트 활성화
                player.gameObject.SetActive(true);

                //TODO: 각 플레이어의 리셋 메서드 호출

                Debug.Log($"[MapSpawnManager] {player.name} reset to initial position");
            }

            // 5. UI 리셋
            if (Tower.UI.CardRewardUI.Instance != null)
            {
                Tower.UI.CardRewardUI.Instance.ResetTotalScore();
                Tower.UI.CardRewardUI.Instance.OnMapStart(0);  // 첫 맵 시작 기록
            }

            // 6. 첫 번째 맵 시작 (딜레이 후)
            StartCoroutine(DelayedResetStart());
        }

        // 리셋 후 첫 맵 시작 딜레이
        IEnumerator DelayedResetStart()
        {
            yield return new WaitForSeconds(0.5f);

            Debug.Log("[MapSpawnManager] Starting first map after reset");
            StartFirstMap();

            // 타이머도 재시작
            if (StageTimer.Instance != null)
            {
                StageTimer.Instance.StartStageTimer(0);
            }
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

        public void StartFirstMapOfCurrentMast()
        {
            if (mapSpawnAreas == null || mapSpawnAreas.Length == 0) return;

            // ⭐ 배열의 첫 번째 MapSpawnArea를 시작 (ID와 무관하게!)
            mapSpawnAreas[0].StartSpawning();

            Debug.Log($"Started first map: ID {mapSpawnAreas[0].mapID}");
        }
    }
}