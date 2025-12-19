using UnityEngine;

// (����) EnemyBehavior�� �⺻���� ���¸� �����Ͽ� �ۼ��߽��ϴ�.
// ���� ������Ʈ�� EnemyBehavior�� �� ������ �����Ͻø� �˴ϴ�.
public class EnemyBehavior : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer; // ������ ������ ��������Ʈ ������

    [Header("Movement")]
    public float moveSpeed = 2.0f; // �� �̵� �ӵ� (�⺻��)

    // ���� ��Ÿ�� ����
    private float currentHp;
    private float currentDamage;
    private float currentDefense;

    private Transform target;
    private EnemyData baseData; // ���� ������ ����

    // �� �ʱ�ȭ �Լ� (Spawner���� ȣ��)
    // �� ���� ���� �Ķ����(useTint, tintColor)�� �߰��Ǿ����ϴ�.
    public void Initialize(EnemyData data, Transform playerTransform, StageData stageInfo, bool useTint, Color tintColor)
    {
        target = playerTransform;
        this.baseData = data;

        // 1. ���� ��� (�⺻ ������ * �������� ����ġ)
        currentHp = data.maxHp * stageInfo.hpMultiplier;
        currentDamage = data.attackPower * stageInfo.damageMultiplier;
        currentDefense = data.defense * stageInfo.defenseMultiplier;

        // 2. �� ���� ���� ����
        ApplyTintColor(useTint, tintColor);

        // �߰����� �ʱ�ȭ ���� (��: AI ���� ��)
        // ...
    }

    // �� [�߰�] �� �����Ӹ��� �÷��̾ ���� �̵��ϴ� ����
    private void Update()
    {
        if (target != null)
        {
            // �÷��̾� ���� ���� ���
            Vector3 direction = (target.position - transform.position).normalized;

            // �̵�
            transform.position += direction * moveSpeed * Time.deltaTime;

            // (���� ����) ���� �÷��̾ �ٶ󺸰� �Ϸ��� �Ʒ� �ּ� ����
            // if (direction.x != 0) spriteRenderer.flipX = direction.x < 0;
        }
    }

    // ������ ��������Ʈ�� �����ϴ� ���� �Լ�
    // ★ [수정됨] 자신 및 모든 하위 요소의 SpriteRenderer 색상을 변경하는 함수
    private void ApplyTintColor(bool useTint, Color tintColor)
    {
        // 자신을 포함한 모든 자식 오브젝트에서 SpriteRenderer 컴포넌트들을 찾습니다.
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        if (renderers == null || renderers.Length == 0)
        {
            // Debug.LogWarning($"{gameObject.name}: 하위 요소에서 SpriteRenderer를 찾을 수 없습니다.");
            return;
        }

        foreach (SpriteRenderer renderer in renderers)
        {
            if (useTint)
            {
                // 설정된 색상(알파값 포함) 적용
                renderer.color = tintColor;
            }
            else
            {
                // 색상을 사용하지 않으면 기본 흰색(원본)으로 복구
                renderer.color = Color.white;
            }
        }
    }

    // (����) ���� �ǰ� ���� �� ���
    public void TakeDamage(float damage)
    {
        // ���� ���� ���� ����
        float finalDamage = Mathf.Max(1, damage - currentDefense);
        currentHp -= finalDamage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
        {
            // 1. [���� �ǽ�] ����ġ ���� (�� ���� ��� ����ġ�� �� �ö��� �̴ϴ�)
            if (GameManager.Instance != null)
            {
                // data�� EnemyData ��ũ���ͺ� ������Ʈ
                GameManager.Instance.AddExp(baseData.expReward);
            }

            // 2. ������ ��� (�Ʊ� �߰��� ����)
            if (LootManager.Instance != null)
            {
                LootManager.Instance.SpawnLoot(transform.position);
            }

            // 3. �������� ų ī��Ʈ ����
            if (StageManager.Instance != null)
            {
                StageManager.Instance.OnEnemyKilled();
            }

            // 4. ����
            Destroy(gameObject);
        }
}
