using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Stage_1", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Basic Info")]
    public int stageNumber;
    public string stageName;

    [Header("Spawn Rules")]
    public int maxEnemyCount = 10;      // 이 스테이지에서 나올 총 몬스터 수
    public float spawnInterval = 1.0f; // 나오는 속도

    [Header("Stat Modifiers")]
    public float hpMultiplier = 1.0f;
    public float damageMultiplier = 1.0f;
    public float defenseMultiplier = 1.0f;

    // 이 스테이지에서 등장할 적 목록
    [Header("Enemies in this Stage")]
    public List<StageEnemyEntry> enemyList;

    // 리스트에 들어갈 데이터 구조 정의
    [System.Serializable]
    public struct StageEnemyEntry
    {
        public string name;         // 인스펙터 보기용 이름 (예: "Cow")
        public GameObject prefab;   // 몬스터 프리팹 (EnemyBehavior가 붙은 것)
        public EnemyData data;      // 그 몬스터의 스탯 데이터 (ScriptableObject)

        // ★ [추가] 색상 덮어쓰기 옵션
        [Header("Tint Color Settings")]
        [Tooltip("체크하면 아래 색상으로 적을 덮어씌웁니다.")]
        public bool useTintColor;
        [Tooltip("덮어씌울 색상 (알파값으로 투명도 조절 가능)")]
        public Color tintColor;
    }
}