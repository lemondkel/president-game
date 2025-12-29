using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;

    // 초기화 함수: 방향과 속도만 받음
    public void Initialize(Vector3 dir, float spd)
    {
        this.direction = dir.normalized;
        this.speed = spd;

        // [시각적 처리] 총알이 날아가는 방향 바라보기
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 5초 뒤 소멸
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        // 로컬 좌표계 기준 오른쪽(앞)으로 이동
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyBehavior enemy = collision.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                // 1. 데미지 계산
                float currentDamage = (PlayerStats.Instance != null) ? PlayerStats.Instance.attack : 10f;
                enemy.TakeDamage(currentDamage);

                // 2. 넉백 처리 (확률 체크)
                if (PlayerStats.Instance != null)
                {
                    float chance = PlayerStats.Instance.knockbackChance; // 예: 10.0f 이면 10%
                    float randomValue = Random.Range(0f, 100f);

                    if (randomValue < chance)
                    {
                        // 총알이 날아가던 방향(direction)으로 넉백 발생
                        enemy.ApplyKnockback(direction);
                    }
                }
            }

            // 총알 소멸
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}