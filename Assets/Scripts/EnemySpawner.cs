using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;  // 소환할 적 프리팹
    public float spawnInterval = 1f; // 소환 간격 (초)
    public float spawnRadius = 10f; // 플레이어로부터 얼마나 떨어져서 소환될지 (반지름)

    private Transform playerTransform;

    // 새로 추가: 적들을 모아둘 부모 오브젝트의 Transform
    private Transform enemyListParent;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // ⭐ 1. EnemyList 오브젝트를 찾아 Transform을 할당합니다.
        GameObject enemyListObj = GameObject.Find("EnemyList");
        if (enemyListObj != null)
        {
            enemyListParent = enemyListObj.transform;
        }

        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            // 플레이어가 존재할 때만 스폰
            if (playerTransform != null)
            {
                SpawnEnemy();
            }

            // 지정된 시간만큼 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        // 1. 랜덤 위치 계산
        Vector2 randomPoint = Random.insideUnitCircle.normalized * spawnRadius;

        // 2. 최종 소환 위치 = 플레이어 위치 + 랜덤 오프셋
        Vector3 spawnPosition = playerTransform.position + new Vector3(randomPoint.x, randomPoint.y, 0);

        // 3. 적 생성
        // 기존 코드: Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // ⭐ 수정된 코드: 부모 Transform을 지정합니다.
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, enemyListParent);
    }
}