using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;

    // 초기화 함수: 방향, 데미지, 속도를 받음
    public void Initialize(Vector3 dir, float dmg, float spd)
    {
        this.direction = dir.normalized; // 방향 정규화 (길이 1)
        this.damage = dmg;
        this.speed = spd;

        // [시각적 처리] 총알이 날아가는 방향을 바라보게 회전 (2D 회전 공식)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 5초 뒤 자동 소멸 (나중엔 Pool로 Return)
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        // 설정된 방향으로 계속 이동
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        // 주의: 위에서 이미 회전(rotation)을 시켰으므로, 로컬 좌표계 기준 '오른쪽(앞)'으로만 가면 됨
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 적에게 데미지 전달
            collision.GetComponent<EnemyBehavior>()?.TakeDamage(damage);
            Destroy(gameObject); // 총알 소멸
        }
        else if (collision.CompareTag("Wall")) // 벽에 닿아도 소멸
        {
            Destroy(gameObject);
        }
    }
}