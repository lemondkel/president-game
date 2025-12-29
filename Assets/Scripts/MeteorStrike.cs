using UnityEngine;

public class MeteorStrike : AbilityBase
{
    protected override bool TryActivate()
    {
        // 1. 적 감지
        int layerMask = 1 << LayerMask.NameToLayer("Enemy");
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 3f, layerMask);

        if (enemies.Length == 0) return false;

        // 2. 데미지 계산 (skillDamage 반영됨)
        float damageToDeal = GetCurrentDamage();

        // ★ 현재 적용 중인 실시간 쿨타임 정보 로그 출력 (디버깅용)
        // Debug.Log($"[Meteor] 시전! (현재 쿨타임: {GetFinalCooldown():F2}s)");

        // 3. 모든 적 타격
        foreach (var enemyCollider in enemies)
        {
            EnemyBehavior enemy = enemyCollider.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageToDeal);
            }

            if (data.projectilePrefab != null)
            {
                Instantiate(data.projectilePrefab, enemyCollider.transform.position, Quaternion.identity);
            }
        }

        return true;
    }
}