using UnityEngine;
using System.Collections;
using CodeMonkey.HealthSystemCM;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.0f;

    [Header("Combat")]
    public float attackDamage = 10f; // ★ 적의 공격력 (조폭이 맞을 때 필요)
    public float attackInterval = 1.0f; // 공격 주기

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

    private Transform target; // 현재 추적 중인 대상 (본체 or 조폭)
    private EnemyData baseData;
    private Rigidbody2D rb;

    private Color originalTintColor = Color.white;
    private Vector3 originalScale;
    private Coroutine flashCoroutine;
    private Coroutine knockbackCoroutine;

    private bool isKnockingBack = false;
    private float lastAttackTime; // 공격 타이머

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(EnemyData data, Transform playerTransform, StageData stageInfo, bool useTint, Color tintColor)
    {
        // 초기 타겟은 플레이어 본체
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
        this.attackDamage = currentDamage; // 공격력 설정

        if (useTint) originalTintColor = tintColor;
        else originalTintColor = Color.white;

        ApplyColor(originalTintColor);

        // ★ [핵심] 주기적으로 가장 가까운 타겟(조폭 포함) 탐색 시작
        StartCoroutine(UpdateTargetRoutine());
    }

    // ★ [타겟 변경 AI] 0.5초마다 가장 가까운 'Player' 태그 찾기
    IEnumerator UpdateTargetRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            FindClosestTarget();
        }
    }

    void FindClosestTarget()
    {
        // 1. 씬 내의 모든 'Player' 태그 오브젝트 찾기 (본체 + 조폭)
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        float closestDist = float.MaxValue;
        Transform bestTarget = null;

        foreach (GameObject t in potentialTargets)
        {
            if (!t.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, t.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                bestTarget = t.transform;
            }
        }

        if (bestTarget != null)
        {
            target = bestTarget;
        }
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
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (target != null && !isKnockingBack)
        {
            // 이동 로직
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    // ★ [충돌 공격] 조폭이나 플레이어와 부딪히면 데미지 주기
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 쿨타임 체크
        if (Time.time < lastAttackTime + attackInterval) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // 1. 조폭(GangUnit)인 경우
            GangUnit gang = collision.gameObject.GetComponent<GangUnit>();
            if (gang != null)
            {
                gang.TakeDamage(currentDamage);
                lastAttackTime = Time.time;
                return;
            }

            // 2. 플레이어 본체인 경우 (HealthSystemComponent 사용 시)
            var healthComp = collision.gameObject.GetComponent<HealthSystemComponent>();
            if (healthComp != null)
            {
                healthComp.GetHealthSystem().Damage(currentDamage);
                lastAttackTime = Time.time;
            }
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