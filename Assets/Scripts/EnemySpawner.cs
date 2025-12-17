using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Settings")]
    public Transform playerTarget; // Inspector에서 Player 오브젝트를 드래그 앤 드롭
    public float spawnRange = 10f; // 플레이어 주변 몇 미터에서 생성할지

    // (기존 SpawnInfo 리스트는 유지하되, 리스폰 주기는 StageData를 따르게 변경)
    public GameObject enemyPrefab; // 테스트용 단일 프리팹 (혹은 리스트 사용)
    public EnemyData baseEnemyData; // 기본 스탯 데이터

    [Header("Monster Configuration")]
    // 프리팹과 데이터를 짝지어서 리스트로 관리 (구조체 활용)
    public List<SpawnInfo> spawnList = new List<SpawnInfo>();

    [System.Serializable]
    public struct SpawnInfo
    {
        public string name;           // 구분용 이름 (Inspector 표시용)
        public GameObject prefab;     // 몬스터 프리팹 (EnemyBehavior가 붙어있어야 함)
        public EnemyData stats;       // 위에서 만든 ScriptableObject 데이터
    }

    void Start()
    {
        // 각 몬스터 별로 독립적인 코루틴(타이머) 실행
        foreach (var info in spawnList)
        {
            StartCoroutine(SpawnRoutine(info));
        }
    }

    IEnumerator SpawnRoutine(SpawnInfo info)
    {
        while (true)
        {
            // 스테이지 매니저가 없거나, 현재 스테이지 데이터가 없으면 대기
            if (StageManager.Instance == null || StageManager.Instance.currentStageData == null)
            {
                yield return null;
                continue;
            }

            StageData currentStage = StageManager.Instance.currentStageData;

            // 1. 목표 마리수를 다 뽑았으면 스폰 중지 (플레이어가 다 잡을 때까지 대기)
            if (StageManager.Instance.spawnedCount >= currentStage.maxEnemyCount)
            {
                yield return null; 
                continue;
            }

            // 2. 몬스터 생성
            SpawnEnemy(currentStage);

            // 3. 스테이지에 설정된 속도만큼 대기
            yield return new WaitForSeconds(currentStage.spawnInterval);
        }
    }

    void SpawnEnemy(StageData stageEffect)
    {
        if (playerTarget == null) return;

        // 1. 생성 위치 계산 (플레이어 주변 랜덤 원형 좌표)
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRange;
        Vector3 spawnPos = playerTarget.position + new Vector3(randomCircle.x, randomCircle.y, 0);

        GameObject instance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);

        // ★ 핵심: 기본 데이터 + 스테이지 버프(Multiplier)를 함께 전달
        EnemyBehavior behavior = instance.GetComponent<EnemyBehavior>();
        if (behavior != null)
        {
            behavior.Initialize(baseEnemyData, playerTarget, stageEffect);
        }

        // 스폰 카운트 증가
        StageManager.Instance.spawnedCount++;
    }
}