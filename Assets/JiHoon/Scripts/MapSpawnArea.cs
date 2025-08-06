using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tower.Enemy;

namespace Tower.Game
{
    // 맵별 스폰 설정을 담는 ScriptableObject
    [CreateAssetMenu(fileName = "MapSpawnConfig", menuName = "Spawn/Map Spawn Config")]
    public class MapSpawnConfig : ScriptableObject
    {
        [Header("Map Info")]
        public string mapName;
        public int mapID;

        [Header("Monster Settings")]
        public GameObject[] monsterPrefabs; // 이 맵에서 스폰될 몬스터 종류들
        public int totalMonsterCount = 21;
        public int initialSpawnCount = 6;
        public int respawnCount = 2; // 몇 마리 죽으면 리스폰
        public int respawnAmount = 2; // 한번에 리스폰되는 수

        [Header("Spawn Weights")]
        public float[] spawnWeights; // 각 몬스터의 스폰 확률 가중치
    }
}

namespace Tower.Game
{

    // 개별 맵의 스폰 포인트를 관리하는 컴포넌트
    public class MapSpawnArea : MonoBehaviour
    {
        [Header("Map Settings")]
        public int mapID;
        public MapSpawnConfig spawnConfig;

        [Header("Spawn Points")]
        public Transform[] spawnPoints; // 인스펙터에서 직접 배치하거나
        public bool autoGeneratePoints = true; // 자동으로 생성

        [Header("Fixed Spawn Settings")]
        public bool useFixedSpawnPositions = false; // 고정 위치 사용 여부
        public Transform[] fixedSpawnPositions; // 고정 스폰 위치들

        [Header("Spawn Parent")]
        public Transform spawnParent; // 스폰된 몬스터들의 부모 오브젝트

        [Header("Auto Generation Settings")]
        public Vector3 areaSize = new Vector3(20f, 0f, 30f);
        public int rows = 7;
        public int columns = 3;
        public float waveStrength = 2f;

        private List<Transform> availableSpawnPoints = new List<Transform>();
        private List<GameObject> activeMonsters = new List<GameObject>();
        private int currentSpawnedCount = 0;
        private int killedInCurrentWave = 0;

        void Start()
        {
            // 기존 스폰 포인트 클리어
            availableSpawnPoints.Clear();

            // Spawn Parent 설정
            if (spawnParent == null)
            {
                GameObject spawnEnemyObj = GameObject.Find("SpawnEnemy");
                if (spawnEnemyObj == null)
                {
                    spawnEnemyObj = new GameObject("SpawnEnemy");
                }
                spawnParent = spawnEnemyObj.transform;
            }

            if (autoGeneratePoints && !useFixedSpawnPositions)
            {
                GenerateSpawnPoints();
            }
            else if (!useFixedSpawnPositions)
            {
                availableSpawnPoints.AddRange(spawnPoints);
            }
        }

        void GenerateSpawnPoints()
        {
            GameObject spawnPointsParent = new GameObject($"SpawnPoints_Map{mapID}");
            spawnPointsParent.transform.SetParent(transform);
            spawnPointsParent.transform.localPosition = Vector3.zero;

            List<Transform> generatedPoints = new List<Transform>();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    float xPos = (col - columns / 2f) * (areaSize.x / columns);
                    float zPos = (row - rows / 2f) * (areaSize.z / rows);

                    // 웨이브 형태 적용
                    float waveOffset = Mathf.Sin((float)row / rows * Mathf.PI * 2) * waveStrength;
                    xPos += waveOffset;

                    Vector3 localPos = new Vector3(xPos, 0, zPos);
                    GameObject point = new GameObject($"SpawnPoint_{row}_{col}");
                    point.transform.SetParent(spawnPointsParent.transform);
                    point.transform.localPosition = localPos;

                    generatedPoints.Add(point.transform);
                }
            }

            spawnPoints = generatedPoints.ToArray();
            availableSpawnPoints.Clear();
            availableSpawnPoints.AddRange(spawnPoints);
        }

        public void StartSpawning()
        {
            if (spawnConfig == null)
            {
                Debug.LogError($"Map {mapID}: SpawnConfig is not set!");
                return;
            }

            Debug.Log($"[Map {mapID}] Starting spawn at position: {transform.position}");
            Debug.Log($"[Map {mapID}] Use fixed positions: {useFixedSpawnPositions}");

            StartCoroutine(InitialSpawn());
        }

        IEnumerator InitialSpawn()
        {
            // 고정 위치 스폰 사용하는 경우
            if (useFixedSpawnPositions && fixedSpawnPositions != null && fixedSpawnPositions.Length > 0)
            {
                Debug.Log($"[Map {mapID}] Using fixed spawn positions");

                // 고정 위치 개수와 몬스터 종류 개수 중 작은 값만큼 스폰
                int spawnCount = Mathf.Min(spawnConfig.initialSpawnCount,
                                          fixedSpawnPositions.Length,
                                          spawnConfig.monsterPrefabs.Length);

                for (int i = 0; i < spawnCount; i++)
                {
                    if (fixedSpawnPositions[i] != null && spawnConfig.monsterPrefabs[i] != null)
                    {
                        // i번째 몬스터를 i번째 위치에 스폰
                        GameObject monster = Instantiate(spawnConfig.monsterPrefabs[i],
                                                       fixedSpawnPositions[i].position,
                                                       fixedSpawnPositions[i].rotation);

                        if (spawnParent != null)
                        {
                            monster.transform.SetParent(spawnParent);
                        }

                        activeMonsters.Add(monster);
                        currentSpawnedCount++;

                        var enemyAI = monster.GetComponent<EnemyAI>();
                        if (enemyAI != null)
                        {
                            enemyAI.SetSpawnArea(this);
                        }
                        else
                        {
                            monster.SendMessage("SetSpawnArea", this, SendMessageOptions.DontRequireReceiver);
                        }

                        yield return new WaitForSeconds(0.3f);
                    }
                }
            }
            else
            {
                // 기존 랜덤 스폰 로직
                List<Transform> tempAvailable = new List<Transform>(availableSpawnPoints);

                for (int i = 0; i < spawnConfig.initialSpawnCount && i < tempAvailable.Count; i++)
                {
                    Transform spawnPoint = GetRandomSpawnPoint(tempAvailable);
                    if (spawnPoint != null)
                    {
                        SpawnMonster(spawnPoint.position);
                        tempAvailable.Remove(spawnPoint);
                        yield return new WaitForSeconds(0.3f);
                    }
                }
            }
        }

        void SpawnMonster(Vector3 position)
        {
            if (currentSpawnedCount >= spawnConfig.totalMonsterCount) return;

            GameObject monsterPrefab = GetRandomMonsterPrefab();
            if (monsterPrefab == null) return;

            GameObject monster = Instantiate(monsterPrefab, position, Quaternion.identity);

            if (spawnParent != null)
            {
                monster.transform.SetParent(spawnParent);
            }

            activeMonsters.Add(monster);
            currentSpawnedCount++;

            var enemyAI = monster.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.SetSpawnArea(this);
            }
            else
            {
                monster.SendMessage("SetSpawnArea", this, SendMessageOptions.DontRequireReceiver);
            }
        }

        GameObject GetRandomMonsterPrefab()
        {
            if (spawnConfig.monsterPrefabs.Length == 0) return null;

            if (spawnConfig.spawnWeights != null && spawnConfig.spawnWeights.Length > 0)
            {
                float totalWeight = 0f;
                for (int i = 0; i < Mathf.Min(spawnConfig.monsterPrefabs.Length, spawnConfig.spawnWeights.Length); i++)
                {
                    totalWeight += spawnConfig.spawnWeights[i];
                }

                float randomValue = Random.Range(0f, totalWeight);
                float currentWeight = 0f;

                for (int i = 0; i < Mathf.Min(spawnConfig.monsterPrefabs.Length, spawnConfig.spawnWeights.Length); i++)
                {
                    currentWeight += spawnConfig.spawnWeights[i];
                    if (randomValue <= currentWeight)
                    {
                        return spawnConfig.monsterPrefabs[i];
                    }
                }
            }

            return spawnConfig.monsterPrefabs[Random.Range(0, spawnConfig.monsterPrefabs.Length)];
        }

        Transform GetRandomSpawnPoint(List<Transform> availableList = null)
        {
            List<Transform> checkList = availableList ?? availableSpawnPoints;
            List<Transform> freePoints = new List<Transform>();

            foreach (Transform point in checkList)
            {
                bool occupied = false;
                foreach (GameObject monster in activeMonsters)
                {
                    if (monster != null && Vector3.Distance(monster.transform.position, point.position) < 2f)
                    {
                        occupied = true;
                        break;
                    }
                }
                if (!occupied) freePoints.Add(point);
            }

            if (freePoints.Count == 0) return null;
            return freePoints[Random.Range(0, freePoints.Count)];
        }

        public void NotifyMonsterKilled(GameObject monster)
        {
            activeMonsters.Remove(monster);
            killedInCurrentWave++;

            if (killedInCurrentWave >= spawnConfig.respawnCount &&
                currentSpawnedCount < spawnConfig.totalMonsterCount)
            {
                killedInCurrentWave = 0;
                StartCoroutine(RespawnWave());
            }

            if (activeMonsters.Count == 0 && currentSpawnedCount >= spawnConfig.totalMonsterCount)
            {
                OnMapCleared();
            }
        }

        IEnumerator RespawnWave()
        {
            int toSpawn = Mathf.Min(spawnConfig.respawnAmount,
                                    spawnConfig.totalMonsterCount - currentSpawnedCount);

            for (int i = 0; i < toSpawn; i++)
            {
                Transform spawnPoint = GetRandomSpawnPoint();
                if (spawnPoint != null)
                {
                    SpawnMonster(spawnPoint.position);
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }

        void OnMapCleared()
        {
            Debug.Log($"Map {mapID} cleared!");
            MapSpawnManager.Instance?.OnMapCleared(mapID);
        }

        public int GetActiveMonsterCount()
        {
            activeMonsters.RemoveAll(monster => monster == null);
            return activeMonsters.Count;
        }

        public bool IsAllMonstersSpawned()
        {
            return currentSpawnedCount >= spawnConfig.totalMonsterCount;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, areaSize);

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Gizmos.color = Color.red;
                foreach (Transform point in spawnPoints)
                {
                    if (point != null)
                        Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
            else if (Application.isPlaying == false && autoGeneratePoints)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < columns; col++)
                    {
                        float xPos = (col - columns / 2f) * (areaSize.x / columns);
                        float zPos = (row - rows / 2f) * (areaSize.z / rows);

                        float waveOffset = Mathf.Sin((float)row / rows * Mathf.PI * 2) * waveStrength;
                        xPos += waveOffset;

                        Vector3 worldPos = transform.position + new Vector3(xPos, 0, zPos);
                        Gizmos.DrawWireSphere(worldPos, 0.3f);
                    }
                }
            }
        }
    }
}