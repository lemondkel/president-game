using UnityEngine;
using System.Collections;
using CodeMonkey.HealthSystemCM;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.0f;

    [Header("Visual Effects")]
    public Color hitColor = Color.red;
    public Color critHitColor = Color.yellow;
    public float flashDuration = 0.1f;
    public float critScaleMultiplier = 1.3f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    // 실제 런타임 스탯
    private float currentHp;
    private float currentDamage;
    private float currentDefense;

    private Transform target;
    private EnemyData baseData;
    private Rigidbody2D rb; // ★ 물리 제어를 위한 컴포넌트

    private Color originalTintColor = Color.white;
    private Vector3 originalScale;
    private Coroutine flashCoroutine;
    private Coroutine knockbackCoroutine;

    private bool isKnockingBack = false;

    private void Awake()
    {
        // ★ Rigidbody2D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
    }

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
        // ★ [핵심 수정] 물리 엔진에 의한 미끄러짐 방지
        // Transform으로 이동하므로, 충돌로 인해 생긴 물리 속도(Velocity)를 매 프레임 제거해야 함
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

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
        bool isCritical = CheckCriticalHit();
        float incomingDamage = isCritical ? damage * 2f : damage;

        float finalDamage = incomingDamage - currentDefense;
        if (finalDamage <= 0) finalDamage = 0.5f;

        currentHp -= finalDamage;

        ExecuteLifeSteal(finalDamage);

        if (gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine(isCritical));
        }

        if (currentHp <= 0) Die();
    }

    private void ExecuteLifeSteal(float actualDamageDealt)
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.lifeSteal > 0)
        {
            float healAmount = actualDamageDealt * (PlayerStats.Instance.lifeSteal / 100f);

            if (GameManager.Instance != null && GameManager.Instance.player != null)
            {
                var hpComp = GameManager.Instance.player.GetComponent<HealthSystemComponent>();
                if (hpComp != null)
                {
                    hpComp.GetHealthSystem().Heal(healAmount);
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
            // ★ 넉백 시에도 Transform을 쓰므로, 물리 속도가 개입하지 않도록 주의
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
