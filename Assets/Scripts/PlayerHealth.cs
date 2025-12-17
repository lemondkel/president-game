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
    public float hitCooldown = 0.5f; // 맞고 나서 0.5초 동안은 무적 (연속 피격 방지)
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

    // ★ [핵심] 몬스터와 계속 닿아있을 때
    private void OnCollisionStay2D(Collision2D collision)
    {
        // 태그 확인: 몬스터인지?
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 무적 시간 체크 (쿨타임)
            if (Time.time > lastHitTime + hitCooldown)
            {
                // 1. 부딪힌 놈(Enemy)의 스크립트 가져오기
                // (EnemyBehavior 스크립트 안에 damageAmount가 있다고 가정)
                EnemyBehavior enemy = collision.gameObject.GetComponent<EnemyBehavior>();

                if (enemy != null)
                {
                    // 2. 그 적의 공격력(Damage)을 가져와서 적용
                    float finalDamage = StageManager.Instance.currentStageData.damageMultiplier;

                    // (만약 ScriptableObject인 'EnemyData'를 따로 쓰고 계시다면 아래처럼)
                    // float finalDamage = enemy.enemyData.attack; 

                    TakeDamage(finalDamage);

                    // 디버그용 (확인하고 지우세요)
                    // Debug.Log($"아야! {finalDamage} 데미지 입음!");
                }

                // 맞은 시간 갱신
                lastHitTime = Time.time;
            }
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