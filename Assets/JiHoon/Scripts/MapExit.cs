using UnityEngine;
using System.Collections;

// 맵 클리어 시 활성화되는 간단한 출구 트리거
public class SimpleMapExit : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int currentMapID = 0; // 현재 맵 ID
    [SerializeField] private Vector3 nextMapStartPosition; // 다음 맵 시작 위치
    [SerializeField] private bool useTransformAsPosition = false; // Transform 사용 여부
    [SerializeField] private Transform nextMapStartTransform; // 다음 맵 시작 Transform

    private bool isTransitioning = false; // 클래스 변수로 추가
    private bool isActive = false;
    private MapSpawnArea mapArea;

    void Start()
    {
        // 이 맵의 MapSpawnArea 찾기
        MapSpawnArea[] areas = FindObjectsOfType<MapSpawnArea>();
        foreach (var area in areas)
        {
            if (area.mapID == currentMapID)
            {
                mapArea = area;
                break;
            }
        }

        // 초기에는 트리거 비활성화
        SetTriggerActive(false);
    }

    void Update()
    {
        // 맵이 클리어되었는지 체크
        if (!isActive && mapArea != null)
        {
            if (mapArea.GetActiveMonsterCount() == 0 && mapArea.IsAllMonstersSpawned())
            {
                SetTriggerActive(true);
            }
        }
    }

    void SetTriggerActive(bool active)
    {
        isActive = active;

        // 콜라이더 활성화/비활성화
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = active;
        }

        Debug.Log($"Map {currentMapID} exit trigger is now {(active ? "ACTIVE" : "INACTIVE")}!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive || isTransitioning) return; // isTransitioning 체크 추가

        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered exit trigger for map {currentMapID}!");
            StartCoroutine(HandleMapTransition(other.gameObject));
        }
    }

    IEnumerator HandleMapTransition(GameObject player)
    {
        // 중복 실행 방지
        isTransitioning = true;
        isActive = false;
        GetComponent<Collider>().enabled = false;

        Debug.Log($"Moving player from map {currentMapID} to map {currentMapID + 1}");

        // 딜레이 줄이기 (0.5초 → 0.1초)
        yield return new WaitForSeconds(0.1f);

        // Transform이 있으면 무조건 Transform 사용
        if (nextMapStartTransform != null)
        {
            player.transform.position = nextMapStartTransform.position;
            player.transform.rotation = nextMapStartTransform.rotation;
        }

        // 다음 맵 몬스터 스폰 시작
        if (MapSpawnManager.Instance != null)
        {
            MapSpawnManager.Instance.StartMap(currentMapID + 1);
        }
    }
}