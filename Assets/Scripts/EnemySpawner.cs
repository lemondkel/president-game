using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Settings")]
    public Transform playerTarget; // 플레이어 참조
    public float spawnRange = 10f; // 스폰 반경

    void Start()
    {
        // 단일 스폰 루틴 시작
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 1. 매니저나 데이터가 준비 안 됐으면 대기
            if (StageManager.Instance == null || StageManager.Instance.currentStageData == null)
            {
                yield return null;
                continue;
            }

            StageData currentStage = StageManager.Instance.currentStageData;

            // 2. 이 스테이지에 등록된 몬스터가 하나도 없으면 대기 (에러 방지)
            if (currentStage.enemyList == null || currentStage.enemyList.Count == 0)
            {
                // Debug.LogWarning("이 스테이지에 등록된 몬스터가 없습니다!");
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            // 3. 목표 마리수를 다 채웠으면 대기
            // (spawnedCount는 StageManager가 관리)
            if (StageManager.Instance.spawnedCount >= currentStage.maxEnemyCount)
            {
                yield return null;
                continue;
            }

            // 4. 몬스터 소환
            SpawnRandomEnemy(currentStage);

            // 5. 스테이지 설정된 주기만큼 대기
            yield return new WaitForSeconds(currentStage.spawnInterval);
        }
    }

    void SpawnRandomEnemy(StageData stageData)
    {
        if (playerTarget == null) return;

        // 1. 위치 계산
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRange;
        Vector3 spawnPos = playerTarget.position + new Vector3(randomCircle.x, randomCircle.y, 0);

        // 2. 스테이지에 등록된 적 목록 중 하나를 랜덤 선택
        int randomIndex = Random.Range(0, stageData.enemyList.Count);
        StageData.StageEnemyEntry selectedEnemy = stageData.enemyList[randomIndex];

        if (selectedEnemy.prefab == null) return;

        // 3. 생성
        GameObject instance = Instantiate(selectedEnemy.prefab, spawnPos, Quaternion.identity, transform);

        // 4. 초기화 (선택된 몬스터의 Data + 현재 스테이지의 보정치 + ★ 색상 정보)
        EnemyBehavior behavior = instance.GetComponent<EnemyBehavior>();
        if (behavior != null)
        {
            // 프리팹과 짝지어진 EnemyData 및 스테이지 정보, 그리고 색상 정보를 함께 전달
            behavior.Initialize(selectedEnemy.data, playerTarget, stageData, selectedEnemy.useTintColor, selectedEnemy.tintColor);
        }

        // 5. 카운트 증가
        StageManager.Instance.spawnedCount++;
    }
}