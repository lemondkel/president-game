using UnityEngine;

[CreateAssetMenu(fileName = "Stage_1", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Basic Info")]
    public int stageNumber;
    public string stageName;

    [Header("Spawn Rules")]
    public int maxEnemyCount = 10;     // 이 스테이지에서 나올 총 몬스터 수
    public float spawnInterval = 1.0f; // 나오는 속도

    [Header("Stat Modifiers (Percentage)")]
    // 1.0 = 100%(기본), 1.5 = 150%(50% 증가)
    public float hpMultiplier = 1.0f;
    public float damageMultiplier = 1.0f;
    public float defenseMultiplier = 1.0f; // ★ 요청하신 방어력 % 증가
}