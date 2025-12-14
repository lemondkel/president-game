using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 3f; // 3초 뒤 자동 삭제

    void Start()
    {
        // 일정 시간 뒤 삭제 (메모리 관리)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 총알이 바라보는 방향(위쪽)으로 계속 이동
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    // 적과 부딪혔을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 적에게 데미지 주기
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // 총알 삭제 (관통하려면 이 줄을 지우세요)
            Destroy(gameObject);
        }
    }
}