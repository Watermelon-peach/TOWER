using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Tower.Game
{
    [System.Serializable]
    public class StageSection
    {
        public string sectionName;
        public int startStage;
        public int endStage;
        public bool hasSafetyZone;
        public Transform safetyZonePoint;
        public Transform sectionStartPoint;
    }

    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }

        [Header("Stage Configuration")]
        [SerializeField] private int stagesPerSection = 4; // 몇 층마다 Safety Zone
        [SerializeField] private List<StageSection> stageSections = new List<StageSection>();

        [Header("Player")]
        [SerializeField] private Transform player;

        [Header("Current Status")]
        [SerializeField] private int currentStage = 1;
        [SerializeField] private Vector3 lastCheckpoint;

        [Header("Death Settings")]
        [SerializeField] private float respawnDelay = 2f;
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private GameObject respawnEffectPrefab;

        [Header("UI")]
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private TMPro.TextMeshProUGUI respawnText;

        [Header("Safety Zones")]
        [SerializeField] private Transform[] safetyZonePositions; // Inspector에서 설정

        private bool isRespawning = false;
        private Sample.PlayerMovement playerMovement;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                AutoGenerateSections(); // 자동으로 섹션 생성
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player").transform;

            playerMovement = player.GetComponent<Sample.PlayerMovement>();
            lastCheckpoint = player.position;
        }

        // 자동으로 섹션 생성 (100층까지도 가능)
        void AutoGenerateSections()
        {
            if (stageSections.Count == 0) // 수동 설정이 없으면 자동 생성
            {
                int totalStages = 100; // 전체 층 수

                for (int i = 1; i <= totalStages; i += stagesPerSection)
                {
                    var section = new StageSection
                    {
                        sectionName = $"Stage {i}-{Mathf.Min(i + stagesPerSection - 1, totalStages)}",
                        startStage = i,
                        endStage = Mathf.Min(i + stagesPerSection - 1, totalStages),
                        hasSafetyZone = (i + stagesPerSection - 1) % stagesPerSection == 0
                    };
                    stageSections.Add(section);
                }
            }
        }

        public void OnStageEnter(int stageNumber)
        {
            currentStage = stageNumber;
            Debug.Log($"Entered Stage {stageNumber}");

            // 현재 섹션 찾기
            var section = GetCurrentSection(stageNumber);

            // Safety Zone 체크
            if (section != null && section.hasSafetyZone && stageNumber == section.endStage + 1)
            {
                UpdateCheckpoint(player.position);
                ShowSafetyZoneUI($"Safety Zone 도달! (Stage {stageNumber})");
            }
        }

        StageSection GetCurrentSection(int stageNumber)
        {
            return stageSections.FirstOrDefault(s =>
                stageNumber >= s.startStage && stageNumber <= s.endStage);
        }

        StageSection GetPreviousSafetyZone(int fromStage)
        {
            // 현재 스테이지 이전의 가장 가까운 Safety Zone 찾기
            return stageSections
                .Where(s => s.hasSafetyZone && s.endStage < fromStage)
                .OrderByDescending(s => s.endStage)
                .FirstOrDefault();
        }

        public void UpdateCheckpoint(Vector3 position)
        {
            lastCheckpoint = position;
            Debug.Log($"Checkpoint updated at stage {currentStage}");
        }

        public void OnPlayerDeath()
        {
            if (isRespawning) return;
            StartCoroutine(HandlePlayerDeath());
        }

        IEnumerator HandlePlayerDeath()
        {
            isRespawning = true;

            if (playerMovement != null)
                playerMovement.enabled = false;

            if (deathEffectPrefab != null)
                Instantiate(deathEffectPrefab, player.position, Quaternion.identity);

            ShowGameOverUI();

            yield return new WaitForSeconds(respawnDelay);

            Vector3 respawnPosition = GetRespawnPosition();
            RespawnPlayer(respawnPosition);

            isRespawning = false;
        }

        Vector3 GetSafetyZonePosition(int zoneNumber)
        {
            // Safety Zone 배열에서 위치 가져오기
            int index = zoneNumber - 1; // Zone 1 = index 0

            if (safetyZonePositions != null &&
                index >= 0 &&
                index < safetyZonePositions.Length &&
                safetyZonePositions[index] != null)
            {
                return safetyZonePositions[index].position;
            }

            Debug.LogWarning($"Safety Zone {zoneNumber} position not set!");
            return lastCheckpoint;
        }

        Vector3 GetRespawnPosition()
        {
            // 현재 스테이지에 따른 리스폰 위치
            if (currentStage >= 1 && currentStage <= 4)
            {
                // 1-4층에서 죽으면 → 1층으로
                return GetStageStartPosition(1);
            }
            else if (currentStage >= 5 && currentStage <= 8)
            {
                // 5-8층에서 죽으면 → 첫 번째 Safety Zone으로
                return GetSafetyZonePosition(1);
            }
            else if (currentStage >= 9 && currentStage <= 12)
            {
                // 9-12층에서 죽으면 → 두 번째 Safety Zone으로
                return GetSafetyZonePosition(2);
            }

            return lastCheckpoint;
        }

        Vector3 GetStageStartPosition(int stageNumber)
        {
            // MapSpawnManager에서 위치 가져오기
            if (MapSpawnManager.Instance != null &&
                stageNumber - 1 >= 0 &&
                stageNumber - 1 < MapSpawnManager.Instance.mapSpawnAreas.Length)
            {
                var spawnArea = MapSpawnManager.Instance.mapSpawnAreas[stageNumber - 1];

                if (spawnArea != null)
                {
                    // spawnPoints가 배열(Transform[])인 경우 Length 사용
                    if (spawnArea.spawnPoints != null && spawnArea.spawnPoints.Length > 0)
                    {
                        return spawnArea.spawnPoints[0].position;
                    }

                    // spawnPoints가 없으면 MapSpawnArea의 위치 사용
                    return spawnArea.transform.position;
                }
            }

            return lastCheckpoint;
        }

        void RespawnPlayer(Vector3 position)
        {
            player.position = position;

            if (respawnEffectPrefab != null)
                Instantiate(respawnEffectPrefab, position, Quaternion.identity);

            HideGameOverUI();

            if (playerMovement != null)
                playerMovement.enabled = true;
        }

        void ShowGameOverUI()
        {
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);

                if (respawnText != null)
                {
                    string respawnLocation = GetRespawnLocationText();
                    respawnText.text = $"게임 오버!\n{respawnLocation}에서 부활합니다...";
                }
            }
        }

        string GetRespawnLocationText()
        {
            var currentSection = GetCurrentSection(currentStage);

            if (currentSection != null && currentSection.startStage == 1)
                return "1층";

            var previousSafeZone = GetPreviousSafetyZone(currentStage);
            if (previousSafeZone != null)
                return $"Stage {previousSafeZone.endStage} Safety Zone";

            return "체크포인트";
        }

        void HideGameOverUI()
        {
            if (gameOverUI != null)
                gameOverUI.SetActive(false);
        }

        void ShowSafetyZoneUI(string message)
        {
            Debug.Log(message);
        }

        // Inspector 설정 도우미
        [ContextMenu("Generate 100 Stages")]
        public void GenerateStages()
        {
            stageSections.Clear();
            AutoGenerateSections();
        }
    }
}