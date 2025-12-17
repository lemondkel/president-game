using UnityEngine;

public class MagicWand : AbilityBase
{
    [Header("Link")]
    public Transform projectileParent;

    // AbilityBase의 TryActivate를 오버라이드 (Update에서 타이머 찰 때마다 호출됨)
    protected override bool TryActivate()
    {
        Transform target = GetNearestEnemy();
        if (target == null) return false; // 적이 없으면 발사 안 하고 쿨타임 유지

        // 데이터에서 프리팹 가져오기
        GameObject prefab = data.projectilePrefab;

        // 투사체 생성
        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity, projectileParent);

        // 방향 계산
        Vector3 dir = (target.position - transform.position).normalized;

        // 투사체 초기화 (부모의 GetCurrentDamage() 사용)
        obj.GetComponent<Projectile>().Initialize(dir, GetCurrentDamage(), 15f);

        // 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        obj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        return true; // 발사 성공 -> 쿨타임 초기화
    }

    Transform GetNearestEnemy()
    {
        // 기존 탐색 로직 동일 (최적화된 버전)
        int layerMask = 1 << LayerMask.NameToLayer("Enemy");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, data.range, layerMask);

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.gameObject.activeInHierarchy) continue;
            float dist = (hit.transform.position - transform.position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }
        return nearest;
    }
}