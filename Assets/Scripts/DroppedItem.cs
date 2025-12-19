using UnityEngine;
using TMPro; // 텍스트 표시용 (선택)

public class DroppedItem : MonoBehaviour
{
    public ItemType type;
    public SpriteRenderer spriteRenderer;
    public TextMeshPro textLabel; // 아이템 이름 띄워주기용 (디버그)

    // ★ 자석 기능 변수
    private bool isAttracted = false;
    private Transform targetTransform;
    private float moveSpeed = 5.0f;     // 초기 날아오는 속도
    private float acceleration = 15.0f; // 가속도 (점점 빨라짐)

    public void Initialize(ItemType itemType, Sprite icon)
    {
        this.type = itemType;
        // 1. 이미지 교체 로직
        if (icon != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = icon;
        }

        // 2. 크기나 색상 초기화 (재사용될 수 있으므로)
        spriteRenderer.color = Color.white;
        transform.localScale = Vector3.one; // 기본 크기

        // (선택) 타입별로 색상이나 이미지를 바꿔주면 좋습니다.
        // 여기서는 텍스트로 무슨 아이템인지 표시
        if (textLabel != null) textLabel.text = itemType.ToString();

        // 예: 돈은 노란색, 다이아는 파란색
        if (itemType == ItemType.Gold) spriteRenderer.color = Color.yellow;
        else spriteRenderer.color = Color.white;
        // 초기화 시 변수 리셋
        isAttracted = false;
        moveSpeed = 5.0f;
    }

    void Update()
    {
        // ★ 자석 로직: 타겟이 생기면 그쪽으로 날아감
        if (isAttracted && targetTransform != null)
        {
            // 시간이 지날수록 점점 빨라지게 (가속도)
            moveSpeed += acceleration * Time.deltaTime;

            // MoveTowards: 현재위치 -> 목표위치로 이동
            transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 자석 범위에 닿았을 때 (끌려가기 시작)
        if (collision.CompareTag("Magnet"))
        {
            isAttracted = true;
            // MagnetArea의 부모(Player)를 타겟으로 잡음
            targetTransform = collision.transform.parent;
        }

        // 2. 플레이어 몸체에 닿았을 때 (진짜 획득)
        // 주의: 자석 범위(Magnet)가 아니라 실제 플레이어(Player)와 닿아야 함
        if (collision.CompareTag("Player"))
        {
            ApplyEffect();
            Destroy(gameObject);
        }
    }

    void ApplyEffect()
    {
        // GameManager 혹은 PlayerStats에 접근하여 능력치 적용
        // 여기서는 로그만 찍고, 실제 구현은 아래 GameManager에 추가하세요.
        Debug.Log($"[Item Acquired] {type} 획득!");
        if (GameManager.Instance != null)
        {
            // ★ 핵심: 매니저에게 "이 타입의 아이템을 먹었다"고 알림
            // (이 함수 안에서 소리도 나고, 효과도 적용됨)
            GameManager.Instance.ApplyItemEffect(this.type);
        }
    }
}