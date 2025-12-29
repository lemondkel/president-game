using UnityEngine;
using System.Collections;
using CodeMonkey.HealthSystemCM; // HealthSystem 사용을 위해 추가

public class EnemyBehavior : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.0f;

    [Header("Visual Effects")]
    public Color hitColor = Color.red;
    public Color critHitColor = Color.yellow; // 치명타 피격 색상
    public float flashDuration = 0.1f;
    public float critScaleMultiplier = 1.3f;  // 치명타 시 크기 확대 배율

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;      // 밀려나는 힘
    public float knockbackDuration = 0.2f; // 밀려나는 시간

    // 실제 런타임 스탯
    private float currentHp;
    private float currentDamage;
    private float currentDefense;

    private Transform target;
    private EnemyData baseData;

    private Color originalTintColor = Color.white;
    private Vector3 originalScale;
    private Coroutine flashCoroutine;
    private Coroutine knockbackCoroutine;

    // 현재 넉백 중인지 여부
    private bool isKnockingBack = false;

    public void Initialize(EnemyData data, Transform playerTransform, StageData stageInfo, bool useTint, Color tintColor)
    {
        target = playerTransform;
        this.baseData = data;
        this.originalScale = transform.localScale;

        float baseHp = 1f;
        float baseDamage = 1f;
        float baseDefense = 1f;
        float baseSpeed = 2.0f;

        if (data != null)
        {
            baseHp = data.maxHp;
            baseDamage = data.attackPower;
            baseDefense = data.defense;
            baseSpeed = data.moveSpeed;
        }

        float hpMult = stageInfo ? stageInfo.hpMultiplier : 1f;
        float dmgMult = stageInfo ? stageInfo.damageMultiplier : 1f;
        float defMult = stageInfo ? stageInfo.defenseMultiplier : 1f;

        currentHp = baseHp * hpMult;
        currentDamage = baseDamage * dmgMult;
        currentDefense = baseDefense * defMult;
        this.moveSpeed = baseSpeed;

        if (useTint) originalTintColor = tintColor;
        else originalTintColor = Color.white;

        ApplyColor(originalTintColor);
    }

    private void ApplyColor(Color color)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers == null || renderers.Length == 0) return;
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = color;
        }
    }

    public float GetCurrentDamage() { return currentDamage; }

    private void Update()
    {
        if (target != null && !isKnockingBack)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private bool CheckCriticalHit()
    {
        if (PlayerStats.Instance == null) return false;
        float critRate = PlayerStats.Instance.critRate;
        return Random.Range(0f, 100f) < critRate;
    }

    public void TakeDamage(float damage)
    {
        // 1. 치명타 여부 확인 및 데미지 계산
        bool isCritical = CheckCriticalHit();
        float incomingDamage = isCritical ? damage * 2f : damage;

        // 2. 방어력 적용 최종 데미지
        float finalDamage = incomingDamage - currentDefense;
        if (finalDamage <= 0) finalDamage = 0.5f;

        currentHp -= finalDamage;

        // 3. 생명력 흡수 (Life Steal) 로직 실행
        ExecuteLifeSteal(finalDamage);

        // 4. 피격 이펙트
        if (gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine(isCritical));
        }

        if (currentHp <= 0) Die();
    }

    // --- 생명력 흡수 함수 분리 ---
    private void ExecuteLifeSteal(float actualDamageDealt)
    {
        // PlayerStats와 GameManager가 존재하는지 확인
        if (PlayerStats.Instance != null && PlayerStats.Instance.lifeSteal > 0)
        {
            // 공식: 가한 데미지 * (생명력 흡수율 / 100)
            float healAmount = actualDamageDealt * (PlayerStats.Instance.lifeSteal / 100f);

            if (GameManager.Instance != null && GameManager.Instance.player != null)
            {
                var hpComp = GameManager.Instance.player.GetComponent<HealthSystemComponent>();
                if (hpComp != null)
                {
                    // 플레이어 체력 회복
                    hpComp.GetHealthSystem().Heal(healAmount);
                    // Debug.Log($"[LifeSteal] {healAmount:F2} HP 회복됨 (데미지: {actualDamageDealt})");
                }
            }
        }
    }

    public void ApplyKnockback(Vector2 direction)
    {
        if (gameObject.activeInHierarchy)
        {
            if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction));
        }
    }

    IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockingBack = true;
        float timer = 0f;
        while (timer < knockbackDuration)
        {
            transform.position += (Vector3)direction * knockbackForce * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        isKnockingBack = false;
    }

    IEnumerator FlashRoutine(bool isCritical)
    {
        Color targetFlashColor = isCritical ? critHitColor : hitColor;
        ApplyColor(targetFlashColor);
        if (isCritical) transform.localScale = originalScale * critScaleMultiplier;
        yield return new WaitForSeconds(flashDuration);
        ApplyColor(originalTintColor);
        if (isCritical) transform.localScale = originalScale;
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            int exp = (baseData != null) ? baseData.expReward : 1;
            GameManager.Instance.AddExp(exp);
        }
        if (LootManager.Instance != null) LootManager.Instance.SpawnLoot(transform.position);
        if (StageManager.Instance != null) StageManager.Instance.OnEnemyKilled();
        transform.localScale = originalScale;
        Destroy(gameObject);
    }
}