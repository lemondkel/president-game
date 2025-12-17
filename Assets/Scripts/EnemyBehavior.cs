using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    // 런타임에 변하는 상태값
    private float currentHp;
    private Transform targetTransform;
    private EnemyData data; // 원본 데이터 참조
    private Rigidbody2D rb; // 리지드바디 참조 추가
    // 내부에서 쓸 실제 방어력 변수 추가
    private float currentDefense;

    // 외부(Spawner)에서 초기화해주는 함수
    public void Initialize(EnemyData baseStats, Transform target, StageData modifier = null)
    {
        this.data = baseStats;
        this.targetTransform = target;

        float hpMult = 1f;
        float defMult = 1f;

        // 보정치가 있으면 적용
        if (modifier != null)
        {
            hpMult = modifier.hpMultiplier;
            defMult = modifier.defenseMultiplier;
        }

        // 최종 스탯 계산 (기본값 * 배율)
        // 주의: data.maxHp를 직접 바꾸면 원본 파일이 바뀌므로, 로컬 변수 currentHp에만 적용
        this.currentHp = baseStats.maxHp * hpMult;

        // 방어력 같은 건 따로 저장해둬야 함 (TakeDamage 계산용)
        // 간단하게 하려면 여기서 임시 변수를 클래스 멤버로 승격시켜야 합니다.
        this.currentDefense = baseStats.defense * defMult;
    }

    void Start() // 혹은 Initialize
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update 대신 FixedUpdate 사용 (물리 연산은 FixedUpdate가 국룰)
    void FixedUpdate()
    {
        if (targetTransform == null) return;

        // 1. 방향 구하기
        Vector2 direction = (targetTransform.position - transform.position).normalized;

        // 2. 물리 이동 (MovePosition)
        // 현재 위치에서 다음 프레임 위치를 계산해서 요청함
        Vector2 nextPos = rb.position + (direction * data.moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPos);
    }

    private void MoveTowardsTarget()
    {
        // 1. 방향 벡터 구하기 (Target - Me)
        Vector2 direction = (targetTransform.position - transform.position).normalized;

        // 2. 이동 (데이터에 있는 moveSpeed 사용)
        // 리지드바디를 쓴다면 MovePosition을 써야 하지만, 간단한 이동은 Translate로 충분
        transform.Translate(direction * data.moveSpeed * Time.deltaTime);

        // *참고: 만약 적이 서로 겹치지 않게 하려면 Rigidbody2D(Dynamic) + Collider 추가 필요
    }

    // 예시: 데미지 받는 함수
    public void TakeDamage(float damage)
    {
        // [기존 코드] 
        // float finalDamage = Mathf.Max(1, damage - data.defense); // 무조건 1이 들어감

        // [수정 코드]
        // 방어력을 뺀 값을 계산하되, 0보다 작아지면(음수면 힐이 되니까) 0으로 고정
        float finalDamage = Mathf.Max(0, damage - currentDefense);

        // 데미지가 0이면 아무 일도 안 일어남 (로그 확인용)
        if (finalDamage <= 0)
        {
            Debug.Log($"[Defense] 방어함! (Dmg: {damage} - Def: {data.defense} = 0)");
            return;
        }

        currentHp -= finalDamage;

        // 로그로 현재 체력 확인
        Debug.Log($"[Hit] 남은 체력: {currentHp} / {data.maxHp} (받은 피해: {finalDamage})");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // EnemyBehavior 클래스 내부의 Die 함수 수정

    private void Die()
    {
        // 1. [누락 의심] 경험치 지급 (이 줄이 없어서 경험치가 안 올랐을 겁니다)
        if (GameManager.Instance != null)
        {
            // data는 EnemyData 스크립터블 오브젝트
            GameManager.Instance.AddExp(data.expReward);
        }

        // 2. 아이템 드랍 (아까 추가한 로직)
        if (LootManager.Instance != null)
        {
            LootManager.Instance.SpawnLoot(transform.position);
        }

        // 3. 스테이지 킬 카운트 집계
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnEnemyKilled();
        }

        // 4. 삭제
        Destroy(gameObject);
    }
}