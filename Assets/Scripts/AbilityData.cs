using UnityEngine;

[CreateAssetMenu(fileName = "New Ability Data", menuName = "Game/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public Sprite icon;
    public int maxLevel = 5;

    [Header("Base Stats (Level 1)")]
    public float baseDamage = 10f;
    public float baseCooldown = 12f; // 조폭 소환 쿨타임 12초
    public float mpCost = 50f;       // 소모 MP 50
    public float range = 2f;         // 소환 범위 (본체 주변 1~2m)

    [Header("Visuals & Projectiles")]
    public GameObject projectilePrefab; // ★ 메테오 폭발 이펙트 등을 위해 필요

    [Header("Summon Specifics")]
    public GameObject summonPrefab;  // 소환할 조폭 프리팹
    public int maxUnitCount = 6;     // 최대 유지 가능 수 (2그룹 = 6명)
    public int unitsPerCast = 3;     // 한 번 시전 당 소환 수 (1그룹)

    [Header("Growth Stats (Per Level)")]
    public float damageGrowth = 0f;       // 소환수는 본체 스탯 비례라 고정값 성장은 0일 수도 있음
    public float cooldownReduction = 0.05f;
}