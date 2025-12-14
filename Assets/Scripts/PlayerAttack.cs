using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject bulletPrefab; // 총알 프리팹
    public float attackRange = 10f; // 적 감지 범위
    public float fireRate = 0.5f;   // 공격 속도 (초)
    private float fireTimer = 0f;

    void Update()
    {
        // 쿨타임 계산
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            // 가장 가까운 적 찾기
            Transform target = FindNearestEnemy();

            // 적이 사거리 안에 있으면 발사
            if (target != null)
            {
                Fire(target);
                fireTimer = 0f; // 타이머 초기화
            }
        }
    }

    Transform FindNearestEnemy()
    {
        // 1. 사거리 내의 모든 적을 찾음 (LayerMask를 쓰면 더 최적화 가능)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            // 사거리 안이고, 지금까지 찾은 적보다 더 가까우면 갱신
            if (distance <= attackRange && distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    void Fire(Transform target)
    {
        // 레벨 가져오기
        int level = GameManager.instance.currentLevel;

        // 적을 향하는 방향 계산
        Vector2 direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // 스프라이트가 위쪽이 앞이라고 가정 시 -90

        // === [레벨별 탄환 발사 로직] ===

        // 레벨 1~2: 기본 1발 발사
        if (level < 3)
        {
            SpawnBullet(angle);
        }
        // 레벨 3~5: 2발 발사 (갈래샷)
        else if (level < 6)
        {
            SpawnBullet(angle - 10f); // 왼쪽 10도
            SpawnBullet(angle + 10f); // 오른쪽 10도
        }
        // 레벨 6 이상: 3발 발사 (부채꼴)
        else
        {
            SpawnBullet(angle - 20f);
            SpawnBullet(angle);
            SpawnBullet(angle + 20f);
        }
    }

    void SpawnBullet(float angle)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Instantiate(bulletPrefab, transform.position, rotation);
    }

    // 공격 범위 시각적으로 보기 (기즈모)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}