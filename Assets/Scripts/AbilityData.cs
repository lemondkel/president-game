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
    public float baseCooldown = 0.5f; // 평타는 0.5초, 궁극기는 60초
    public float range = 10f;
    public GameObject projectilePrefab; // 투사체나 이펙트

    [Header("Growth Stats (Per Level)")]
    public float damageGrowth = 2f;   // 레벨업당 데미지 +2
    public float cooldownReduction = 0.05f; // 레벨업당 쿨타임 5% 감소
}