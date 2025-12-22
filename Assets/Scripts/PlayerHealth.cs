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
    public float hitCooldown = 0.5f;
    private float lastHitTime;

    private bool isDead = false;

    // ★ [추가] 체력 재생용 타이머
    private float regenTimer = 0f;

    void Start()
    {
        healthSystemComponent = GetComponent<HealthSystemComponent>();
        if (healthSystemComponent != null)
        {
            healthSystemComponent.GetHealthSystem().OnHealthChanged += PlayerHealth_OnHealthChanged;
            healthSystemComponent.GetHealthSystem().OnDead += PlayerHealth_OnDead;
            UpdateText();
        }
    }

    // ★ [추가] 매 프레임 체력 재생 체크
    void Update()
    {
        if (isDead) return; // 죽었으면 재생 안 함

        HandleHpRegen();
    }

    void HandleHpRegen()
    {
        if (healthSystemComponent == null) return;

        // PlayerStats가 없으면 기본값 0
        float regenAmount = (PlayerStats.Instance != null) ? PlayerStats.Instance.hpRegen : 0f;

        if (regenAmount <= 0) return;

        // 1초마다 회복
        regenTimer += Time.deltaTime;
        if (regenTimer >= 1.0f)
        {
            healthSystemComponent.GetHealthSystem().Heal(regenAmount);
            regenTimer = 0f; // 타이머 초기화 (또는 -= 1.0f 로 오차 보정 가능)
        }
    }

    public void Revive()
    {
        isDead = false;
        regenTimer = 0f; // 부활 시 타이머 리셋
        UpdateText();
    }

    private void PlayerHealth_OnHealthChanged(object sender, EventArgs e)
    {
        UpdateText();
        if (isDead && healthSystemComponent.GetHealthSystem().GetHealth() > 0)
        {
            isDead = false;
        }
    }

    private void PlayerHealth_OnDead(object sender, EventArgs e)
    {
        if (isDead) return;
        isDead = true;

        if (StageManager.Instance != null)
        {
            StageManager.Instance.RestartStage();
        }
    }

    private void UpdateText()
    {
        if (hpText != null && healthSystemComponent != null)
        {
            float currentHp = healthSystemComponent.GetHealthSystem().GetHealth();
            float maxHp = healthSystemComponent.GetHealthSystem().GetHealthMax();
            hpText.text = $"{(int)currentHp} / {(int)maxHp}";
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Enemy")) TryTakeDamageFrom(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead) return;
        if (collision.CompareTag("Enemy")) TryTakeDamageFrom(collision.gameObject);
    }

    private void TryTakeDamageFrom(GameObject enemyObject)
    {
        if (Time.time > lastHitTime + hitCooldown)
        {
            EnemyBehavior enemy = enemyObject.GetComponent<EnemyBehavior>();
            float incomingDamage = 0f;

            if (enemy != null)
            {
                incomingDamage = enemy.GetCurrentDamage();
            }
            else
            {
                if (StageManager.Instance != null && StageManager.Instance.currentStageData != null)
                    incomingDamage = StageManager.Instance.currentStageData.damageMultiplier;
            }

            float playerDefense = 0f;
            if (PlayerStats.Instance != null)
            {
                playerDefense = PlayerStats.Instance.defense;
            }

            float finalDamage = incomingDamage - playerDefense;

            if (finalDamage <= 0) finalDamage = 0.5f;

            if (finalDamage > 0)
            {
                TakeDamage(finalDamage);
            }

            lastHitTime = Time.time;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        if (healthSystemComponent != null)
        {
            healthSystemComponent.GetHealthSystem().Damage(damageAmount);
        }
    }
}