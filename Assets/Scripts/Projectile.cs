using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;

    // 초기화 함수: 방향과 속도만 받음 (데미지는 충돌 시 PlayerStats에서 가져옴)
    public void Initialize(Vector3 dir, float spd)
    {
        this.direction = dir.normalized;
        this.speed = spd;

        // [시각적 처리] 총알이 날아가는 방향 바라보기
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 5초 뒤 소멸 (오브젝트 풀링 사용 시 Return 로직으로 변경 필요)
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
            // ★ [변경] PlayerStats의 attack 값을 현재 데미지로 사용
            float currentDamage = 0f;

            if (PlayerStats.Instance != null)
            {
                currentDamage = PlayerStats.Instance.attack;
            }
            else
            {
                Debug.LogWarning("PlayerStats 인스턴스를 찾을 수 없습니다! 기본 데미지 10 적용");
                currentDamage = 10f;
            }

            // 적에게 데미지 전달
            collision.GetComponent<EnemyBehavior>()?.TakeDamage(currentDamage);

            // 총알 소멸
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}