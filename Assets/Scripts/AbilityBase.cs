using UnityEngine;

public abstract class AbilityBase : MonoBehaviour
{
    [Header("Data")]
    public AbilityData data; // 위에서 만든 데이터 연결

    [Header("Current Status (Read Only)")]
    public int currentLevel = 0; // 0이면 미습득, 1부터 시작
    public float currentCooldown;
    protected float timer;

    // 외부에서 레벨업 시 호출
    public void LevelUp()
    {
        if (currentLevel < data.maxLevel)
        {
            currentLevel++;
            // 레벨에 따른 스탯 재계산 (공식은 기획에 따라 변경 가능)
            RecalculateStats();

            Debug.Log($"[{data.skillName}] Level Up! Lv.{currentLevel}");
        }
    }

    protected virtual void Start()
    {
        // 시작 시 레벨 1로 초기화 (테스트용)
        if (currentLevel == 0) LevelUp();
        RecalculateStats();
    }

    protected virtual void Update()
    {
        if (currentLevel == 0) return; // 배우지 않은 스킬은 동작 X

        timer += Time.deltaTime;

        if (timer >= currentCooldown)
        {
            // 자식 클래스에서 구현한 실제 스킬 로직 실행
            if (TryActivate())
            {
                timer = 0f;
            }
        }
    }

    // 스탯 계산 공식 (Base + (Level-1 * Growth))
    protected void RecalculateStats()
    {
        // 쿨타임 감소 공식 예시: 기본쿨 * (1 - (레벨 * 감소율))
        currentCooldown = data.baseCooldown * (1f - ((currentLevel - 1) * data.cooldownReduction));
        currentCooldown = Mathf.Max(0.1f, currentCooldown); // 최소 0.1초 제한
    }

    // 현재 데미지 계산 함수 (자식들이 가져다 씀)
    public float GetCurrentDamage()
    {
        return data.baseDamage + ((currentLevel - 1) * data.damageGrowth);
    }

    // 자식 클래스가 반드시 구현해야 하는 함수
    protected abstract bool TryActivate();

    // 1. 현재 쿨타임 중인지 확인
    public bool IsOnCooldown()
    {
        // 타이머가 0보다 크면 쿨타임 중
        return timer > 0f;
    }

    // 2. 남은 시간 비율 (0.0 ~ 1.0) 계산해서 반환
    public float GetCooldownRatio()
    {
        if (currentCooldown <= 0f) return 0f; // 0으로 나누기 방지

        // 남은 시간 / 전체 시간 = 비율 (예: 2초 남음 / 5초 전체 = 0.4)
        return timer / currentCooldown;
    }

    // 3. (옵션) 남은 시간 초 단위로 반환 (텍스트 표시용)
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, timer);
    }
}