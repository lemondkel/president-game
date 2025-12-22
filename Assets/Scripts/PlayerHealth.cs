using UnityEngine;
using CodeMonkey.HealthSystemCM;
using System;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    private HealthSystemComponent healthSystemComponent;

    [Header("UI Reference")]
    public TextMeshProUGUI hpText;

    [Header("Settings")]
    public float hitCooldown = 0.5f; // 무적 시간
    private float lastHitTime;

    void Start()
    {
        healthSystemComponent = GetComponent<HealthSystemComponent>();

        if (healthSystemComponent != null)
        {
            // 이벤트 구독
            healthSystemComponent.GetHealthSystem().OnHealthChanged += PlayerHealth_OnHealthChanged;
            healthSystemComponent.GetHealthSystem().OnDead += PlayerHealth_OnDead;

            UpdateText(); // 초기 갱신
        }
    }

    // 체력 변경 시 호출
    private void PlayerHealth_OnHealthChanged(object sender, EventArgs e)
    {
        UpdateText();
    }

    // 사망 시 호출
    private void PlayerHealth_OnDead(object sender, EventArgs e)
    {
        if (StageManager.Instance != null) StageManager.Instance.RestartStage();
    }

    // 텍스트 갱신 로직
    private void UpdateText()
    {
        if (hpText != null && healthSystemComponent != null)
        {
            float currentHp = healthSystemComponent.GetHealthSystem().GetHealth();
            float maxHp = healthSystemComponent.GetHealthSystem().GetHealthMax();
            hpText.text = $"{(int)currentHp} / {(int)maxHp}";
        }
    }

    // ★ [핵심] 몬스터와 충돌 감지 (물리)
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TryTakeDamageFrom(collision.gameObject);
        }
    }

    // ★ [핵심] 몬스터와 겹침 감지 (트리거)
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            TryTakeDamageFrom(collision.gameObject);
        }
    }

    // 데미지 처리 공통 로직
    private void TryTakeDamageFrom(GameObject enemyObject)
    {
        // 무적 시간 체크
        if (Time.time > lastHitTime + hitCooldown)
        {
            // 1. 부딪힌 적의 스크립트 가져오기
            EnemyBehavior enemy = enemyObject.GetComponent<EnemyBehavior>();

            if (enemy != null)
            {
                // 2. 적의 실제 공격력 가져오기 (Initialize에서 계산된 값)
                float damage = enemy.GetCurrentDamage();

                // 0뎀이면 무시
                if (damage > 0)
                {
                    TakeDamage(damage);
                    // Debug.Log($"[피격] {enemy.name}에게 {damage} 데미지를 입었습니다.");
                }
            }
            else
            {
                // (비상용) EnemyBehavior가 없는 'Enemy' 태그라면 스테이지 기본 데미지 적용
                if (StageManager.Instance != null && StageManager.Instance.currentStageData != null)
                {
                    TakeDamage(StageManager.Instance.currentStageData.damageMultiplier);
                }
            }

            // 맞은 시간 갱신
            lastHitTime = Time.time;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (healthSystemComponent != null)
        {
            healthSystemComponent.GetHealthSystem().Damage(damageAmount);
        }
    }
}