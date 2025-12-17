using UnityEngine;

public class MeteorStrike : AbilityBase
{
    // 광역기 이펙트 프리팹 (큰 폭발 같은 거)
    // 데이터(SO)의 projectilePrefab에 폭발 이펙트를 넣으세요.

    protected override bool TryActivate()
    {
        // 1. 화면 내 모든 적 찾기 (카메라 뷰포트 기준 혹은 넓은 범위)
        int layerMask = 1 << LayerMask.NameToLayer("Enemy");
        // 범위 3f 정도면 화면 전체 커버
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 3f, layerMask);

        if (enemies.Length == 0) return false; // 적이 없으면 아끼기

        Debug.Log($"[Meteor] 쾅! {enemies.Length}마리의 적을 타격합니다.");

        // 2. 모든 적에게 데미지
        foreach (var enemyCollider in enemies)
        {
            EnemyBehavior enemy = enemyCollider.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                // 부모의 데미지 계산 함수 사용
                enemy.TakeDamage(GetCurrentDamage());

                // (선택) 넉백 추가: 적들을 바깥으로 밀어내기
                // Vector2 pushDir = (enemy.transform.position - transform.position).normalized;
                // enemy.Knockback(pushDir, 5f);
            }

            // 3. 적 위치마다 폭발 이펙트 생성 (옵션)
            if (data.projectilePrefab != null)
            {
                Instantiate(data.projectilePrefab, enemy.transform.position, Quaternion.identity);
            }
        }

        return true; // 스킬 사용 완료 -> 1분 쿨타임 시작
    }
}