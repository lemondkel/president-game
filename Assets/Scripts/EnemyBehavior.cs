using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [Header("References")]
    // public SpriteRenderer spriteRenderer; 

    [Header("Movement")]
    public float moveSpeed = 2.0f;

    // 실제 런타임 스탯
    private float currentHp;
    private float currentDamage;
    private float currentDefense;

    private Transform target;
    private EnemyData baseData; // 원본 데이터 참조 (null일 수 있음)

    public void Initialize(EnemyData data, Transform playerTransform, StageData stageInfo, bool useTint, Color tintColor)
    {
        target = playerTransform;
        this.baseData = data; // 데이터 저장 (null일 수도 있음)

        // ★ [수정] baseData에 직접 쓰는 게 아니라, 계산용 지역 변수를 사용합니다.
        float baseHp = 1f;      // 기본 체력
        float baseDamage = 1f;   // 기본 공격력
        float baseDefense = 1f;  // 기본 방어력

        if (data != null)
        {
            // 데이터가 있으면 그 값을 사용
            baseHp = data.maxHp;       // (EnemyData 필드명에 맞게 수정하세요. 예: maxHp)
            baseDamage = data.attackPower; // (예: attackPower)
            baseDefense = data.defense;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] EnemyData가 연결되지 않았습니다! 기본 HP(1)를 사용합니다.");
        }

        // 1. 스탯 계산 (기본값 * 스테이지 보정치)
        float hpMult = stageInfo ? stageInfo.hpMultiplier : 1f;
        float dmgMult = stageInfo ? stageInfo.damageMultiplier : 1f;
        float defMult = stageInfo ? stageInfo.defenseMultiplier : 1f;

        currentHp = baseHp * hpMult;
        currentDamage = baseDamage * dmgMult;
        currentDefense = baseDefense * defMult;

        ApplyTintColor(useTint, tintColor);
    }


    // PlayerHealth에서 이 적의 공격력을 알 수 있도록 Getter 제공
    public float GetCurrentDamage()
    {
        return currentDamage;
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void ApplyTintColor(bool useTint, Color tintColor)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        if (renderers == null || renderers.Length == 0) return;

        foreach (SpriteRenderer renderer in renderers)
        {
            if (useTint) renderer.color = tintColor;
            else renderer.color = Color.white;
        }
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = Mathf.Max(1, damage - currentDefense);
        currentHp -= finalDamage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 1. 경험치 처리
        if (GameManager.Instance != null)
        {
            // ★ [수정] baseData가 null일 경우 기본 경험치(1) 지급
            int exp = (baseData != null) ? baseData.expReward : 1;
            GameManager.Instance.AddExp(exp);
        }

        // 2. 아이템 드랍
        if (LootManager.Instance != null)
        {
            LootManager.Instance.SpawnLoot(transform.position);
        }

        // 3. 킬 카운트
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnEnemyKilled();
        }

        // 4. 소멸
        Destroy(gameObject);
    }
}