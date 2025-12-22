using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

public class EnemyBehavior : MonoBehaviour
{
    [Header("References")]
    // public SpriteRenderer spriteRenderer; 

    [Header("Movement")]
    public float moveSpeed = 2.0f;

    [Header("Visual Effects")]
    public Color hitColor = Color.red; // 피격 시 변할 색상 (흰색이나 빨간색 추천)
    public float flashDuration = 0.1f; // 번쩍이는 시간

    // 실제 런타임 스탯
    private float currentHp;
    private float currentDamage;
    private float currentDefense;

    private Transform target;
    private EnemyData baseData; // 원본 데이터 참조 (null일 수 있음)

    // 원래 색상 저장용 (틴트 포함)
    private Color originalTintColor = Color.white;
    private Coroutine flashCoroutine;

    public void Initialize(EnemyData data, Transform playerTransform, StageData stageInfo, bool useTint, Color tintColor)
    {
        target = playerTransform;
        this.baseData = data; // 데이터 저장 (null일 수도 있음)

        // ★ [수정] baseData에 직접 쓰는 게 아니라, 계산용 지역 변수를 사용합니다.
        float baseHp = 1f;      // 기본 체력
        float baseDamage = 1f;   // 기본 공격력
        float baseDefense = 1f;  // 기본 방어력
        float baseSpeed = 2.0f;  // ★ [추가] 기본 이동 속도

        if (data != null)
        {
            // 데이터가 있으면 그 값을 사용
            baseHp = data.maxHp;
            baseDamage = data.attackPower;
            baseDefense = data.defense;
            baseSpeed = data.moveSpeed; // ★ [추가] 데이터에서 이동 속도 가져옴
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

        // ★ [추가] 최종 이동 속도 적용
        this.moveSpeed = baseSpeed;

        // 색상 적용 및 원본 색상 저장
        if (useTint) originalTintColor = tintColor;
        else originalTintColor = Color.white;

        ApplyColor(originalTintColor);
    }

    // --- 색상 변경 헬퍼 ---
    private void ApplyColor(Color color)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers == null || renderers.Length == 0) return;
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = color;
        }
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
        // ★ [수정] 방어력 공식 변경 (최소 데미지 0.5)
        // 기존: Mathf.Max(1, ...) -> 변경: 0.5
        float finalDamage = damage - currentDefense;
        if (finalDamage <= 0) finalDamage = 0.5f;

        currentHp -= finalDamage;

        // ★ [추가] 피격 이펙트 재생
        if (gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // ★ [추가] 번쩍임 코루틴
    IEnumerator FlashRoutine()
    {
        // 1. 피격 색상으로 변경
        ApplyColor(hitColor);

        // 2. 잠시 대기
        yield return new WaitForSeconds(flashDuration);

        // 3. 원래 색상(틴트 포함)으로 복구
        ApplyColor(originalTintColor);
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