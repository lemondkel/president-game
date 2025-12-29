using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    [Header("Data")]
    public AbilityData data;

    [Header("Current Status (Read Only)")]
    public int currentLevel = 0; // 0이면 미습득, 1부터 시작
    public float baseCooldownPerLevel; // 레벨업으로 계산된 기본 쿨타임
    protected float timer;

    public void LevelUp()
    {
        if (currentLevel < data.maxLevel)
        {
            currentLevel++;
            RecalculateStats();
            Debug.Log($"[{data.skillName}] 레벨업! Lv.{currentLevel}");
        }
    }

    protected virtual void Start()
    {
        if (currentLevel == 0) LevelUp();
        RecalculateStats();
    }

    protected virtual void Update()
    {
        if (currentLevel == 0) return;

        timer += Time.deltaTime;

        // ★ [수정] 실시간 쿨타임 감소 스탯 반영
        if (timer >= GetFinalCooldown())
        {
            if (TryActivate())
            {
                timer = 0f;
            }
        }
    }

    // 레벨업 시에만 호출되는 기본 스탯 재계산
    protected void RecalculateStats()
    {
        // 스킬 레벨에 따른 쿨타임 감소만 먼저 계산해 둡니다.
        baseCooldownPerLevel = data.baseCooldown * (1f - ((currentLevel - 1) * data.cooldownReduction));
    }

    // ★ [추가] 플레이어 스탯(cooldownReduction)까지 합산한 최종 쿨타임 반환
    public float GetFinalCooldown()
    {
        float statReduction = 0f;
        if (PlayerStats.Instance != null)
        {
            // PlayerStats.Instance.cooldownReduction이 20이면 20% 감소
            statReduction = PlayerStats.Instance.cooldownReduction / 100f;
        }

        // 공식: (레벨업 적용 쿨타임) * (1 - 스탯 감소율)
        float finalCooldown = baseCooldownPerLevel * (1f - statReduction);

        // 최소 0.1초 쿨타임 보장 (무한 연사 방지)
        return Mathf.Max(0.1f, finalCooldown);
    }

    // 현재 데미지 계산 함수 (skillDamage 반영)
    public float GetCurrentDamage()
    {
        float skillBaseDamage = data.baseDamage + ((currentLevel - 1) * data.damageGrowth);
        float multiplier = 1f;
        if (PlayerStats.Instance != null)
        {
            multiplier += (PlayerStats.Instance.skillDamage / 100f);
        }
        return skillBaseDamage * multiplier;
    }

    protected abstract bool TryActivate();

    public bool IsOnCooldown() => timer < GetFinalCooldown();

    public float GetCooldownRatio()
    {
        float finalCooldown = GetFinalCooldown();
        if (finalCooldown <= 0f) return 0f;
        return Mathf.Clamp01(timer / finalCooldown);
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, GetFinalCooldown() - timer);
    }
}