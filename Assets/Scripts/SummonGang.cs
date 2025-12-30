using UnityEngine;
using System.Collections.Generic;

public class SummonGang : AbilityBase
{
    // 현재 살아있는 조폭들을 관리하는 리스트
    private List<GangUnit> activeUnits = new List<GangUnit>();

    protected override bool TryActivate()
    {
        // 1. 리스트 청소: 죽어서 사라진(null) 유닛들은 리스트에서 제거
        activeUnits.RemoveAll(unit => unit == null || !unit.gameObject.activeInHierarchy);

        // 2. 최대 소환 수 체크
        // 데이터에 설정된 maxUnitCount(예: 6)와 비교
        if (activeUnits.Count >= data.maxUnitCount)
        {
            Debug.Log($"[Summon] 최대 소환 수({data.maxUnitCount}명)에 도달하여 소환할 수 없습니다.");
            return false; // 스킬 실패 처리 (쿨타임 안 돔)
        }

        // 3. 소환할 마리 수 계산
        // 기본 소환 수(unitsPerCast)와 남은 자리 중 작은 값 선택
        int remainingSpace = data.maxUnitCount - activeUnits.Count;
        int spawnCount = Mathf.Min(data.unitsPerCast, remainingSpace);

        if (spawnCount <= 0) return false;

        // 4. 실제 소환 루프
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnUnit();
        }

        Debug.Log($"[Summon] 조폭 {spawnCount}명 추가 소환! (현재 총 {activeUnits.Count}명)");
        return true; // 스킬 성공 -> 쿨타임 시작
    }

    void SpawnUnit()
    {
        if (data.summonPrefab == null) return;

        // 본체 주변 랜덤 위치 (1~2m 반경)
        Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(1f, data.range);
        Vector3 spawnPos = transform.position + (Vector3)randomOffset;

        // 생성
        GameObject go = Instantiate(data.summonPrefab, spawnPos, Quaternion.identity);

        // 조폭 초기화 및 리스트 등록
        GangUnit unit = go.GetComponent<GangUnit>();
        if (unit != null)
        {
            float playerMaxHp = 1f;
            float playerAttack = 1f;

            if (PlayerStats.Instance != null)
            {
                int maxHp = GameManager.Instance.getMaxHp();
                playerMaxHp = maxHp * 5;
                playerAttack = PlayerStats.Instance.attack;
            }

            // 스탯 전달
            unit.Initialize(transform, playerMaxHp, playerAttack);

            // ★ 중요: 리스트에 추가해서 관리
            activeUnits.Add(unit);
        }
    }
}