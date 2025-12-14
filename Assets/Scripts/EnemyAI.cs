using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float moveSpeed = 3f;
    public int maxHealth = 3; // 적 체력
    private int currentHealth;

    private Transform targetPlayer;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            targetPlayer = playerObj.transform;
            Debug.Log("플레이어를 찾았습니다: " + targetPlayer.gameObject.name);
        }
        else
        {
            // ⭐ 이 로그가 콘솔에 뜨면 태그 문제 또는 플레이어 부재가 확실합니다.
            Debug.LogError("플레이어 오브젝트(Tag: Player)를 찾을 수 없습니다! 적이 움직이지 않습니다.");
        }
    }

    void FixedUpdate()
    {
        if (targetPlayer == null) return;

        Vector2 direction = (targetPlayer.position - transform.position).normalized;

        // 계산된 이동 거리 (매 FixedUpdate 프레임당)
        Vector2 movement = direction * moveSpeed * Time.fixedDeltaTime;

        // ============== [디버깅 코드 추가 시작] ==============

        // 1. 방향 벡터와 이동 속도 로깅
        // - direction이 (0, 0)에 가깝다면 문제가 있는 것.
        // - moveSpeed가 0이 아닌지 확인.
        Debug.Log($"Direction: {direction.x:F3}, {direction.y:F3} | Speed: {moveSpeed}");

        // 2. 실제로 이동하려는 거리 로깅
        // - movement가 (0, 0)에 가깝다면 (0.0001 등) moveSpeed가 너무 작다는 뜻.
        Debug.Log($"Movement Per Step: {movement.x:F5}, {movement.y:F5}");

        // 3. (선택적) 플레이어와의 거리 로깅
        // - 거리가 줄어들고 있는지 확인
        float distance = Vector2.Distance(transform.position, targetPlayer.position);
        Debug.Log($"Distance to Player: {distance:F2}");

        // ============== [디버깅 코드 추가 끝] ==============

        rb.MovePosition(rb.position + movement);

        if (direction.x > 0) spriteRenderer.flipX = false;
        else if (direction.x < 0) spriteRenderer.flipX = true;
    }

    // === [추가된 부분: 데미지 처리] ===
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // 피격 효과 (색상 깜빡임 등)를 넣을 수 있는 곳

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 게임 매니저에게 킬 수 증가 요청
        if (GameManager.instance != null)
        {
            GameManager.instance.AddKill();
        }

        // 사망 이펙트 생성 가능
        Destroy(gameObject);
    }
}