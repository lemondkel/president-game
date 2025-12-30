using UnityEngine;
using System.Collections;

public class GangUnit : MonoBehaviour
{
    [Header("Stats")]
    public float hp;
    public float damage;
    public float moveSpeed = 3.5f;
    public float attackRange = 1.2f; // 공격 사거리
    public float attackCooldown = 1.0f;

    // ★ [추가] 활동 반경 (플레이어로부터 이 거리 이상 멀어지면 복귀)
    // 화면 크기를 고려해 약 8~10 정도가 적당합니다.
    public float chaseLimitRange = 10.0f;

    private float lastAttackTime;
    private Transform playerTransform;
    private Transform currentTarget;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    public void Initialize(Transform player, float ownerMaxHp, float ownerAttack)
    {
        this.playerTransform = player;
        this.rb = GetComponent<Rigidbody2D>();
        this.spriteRenderer = GetComponent<SpriteRenderer>();

        this.hp = ownerMaxHp * 0.4f;
        this.damage = ownerAttack * 0.8f;

        Debug.Log($"[GangUnit] 생성됨! HP:{hp}, ATK:{damage}");
    }

    void Update()
    {
        if (playerTransform == null) return;

        // ★ [핵심] 본체와의 거리 체크 (화면 이탈 방지)
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. 거리가 활동 반경을 넘어가면 강제 복귀
        if (distToPlayer > chaseLimitRange)
        {
            currentTarget = null; // 쫓던 적 포기
            MoveTowards(playerTransform.position);
            return; // 아래의 적 탐색/공격 로직은 건너뜀
        }

        // 2. 타겟 탐색 (적이 없거나 비활성화된 경우)
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            FindClosestEnemy();
        }

        // 3. 행동 결정 (추적 및 공격)
        if (currentTarget != null)
        {
            float distToEnemy = Vector2.Distance(transform.position, currentTarget.position);

            if (distToEnemy <= attackRange)
            {
                // 공격 사거리 안 -> 공격
                rb.velocity = Vector2.zero;
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack(currentTarget);
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                // 공격 사거리 밖 -> 적 추적
                MoveTowards(currentTarget.position);
            }
        }
        else
        {
            // 적이 없으면 본체 주변(2m) 유지하며 따라다니기
            if (distToPlayer > 2.0f)
            {
                MoveTowards(playerTransform.position);
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    void MoveTowards(Vector3 dest)
    {
        Vector2 dir = (dest - transform.position).normalized;
        rb.velocity = dir * moveSpeed;

        if (dir.x != 0 && spriteRenderer != null)
            spriteRenderer.flipX = dir.x < 0;
    }

    void FindClosestEnemy()
    {
        int layerIndex = LayerMask.NameToLayer("Enemy");
        if (layerIndex == -1) return;

        int layerMask = 1 << layerIndex;
        // 탐색 범위도 활동 반경에 맞춰 제한
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, chaseLimitRange, layerMask);

        float closestDist = float.MaxValue;
        currentTarget = null;

        foreach (var enemyCollider in enemies)
        {
            if (enemyCollider.gameObject == gameObject) continue;

            float dist = Vector2.Distance(transform.position, enemyCollider.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                currentTarget = enemyCollider.transform;
            }
        }
    }

    void Attack(Transform target)
    {
        EnemyBehavior enemy = target.GetComponent<EnemyBehavior>();
        if (enemy != null)
        {
            enemy.TakeDamage(this.damage);
            StartCoroutine(AttackEffect());
        }
    }

    public void TakeDamage(float amount)
    {
        this.hp -= amount;
        StartCoroutine(HitEffect());
        if (this.hp <= 0) Die();
    }

    IEnumerator AttackEffect()
    {
        if (spriteRenderer) spriteRenderer.color = Color.grey;
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer) spriteRenderer.color = Color.white;
    }

    IEnumerator HitEffect()
    {
        if (spriteRenderer) spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer) spriteRenderer.color = Color.white;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}