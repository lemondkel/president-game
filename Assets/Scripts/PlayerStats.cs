using UnityEngine;
// Networking 네임스페이스 제거 (더 이상 통신 안 함)
using CodeMonkey.HealthSystemCM;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Player Stats (Read Only)")]
    // 초기값은 1.0f가 맞지만, 실제 게임 시작 시 GameManager가 서버 값을 덮어씌움
    public float attack = 1f;
    public float defense = 1f;
    public float cooldownReduction = 1f;
    public float knockbackChance = 1f;
    public float attackSpeed = 1f;
    public float hpRegen = 1f;
    public float moveSpeed = 1f;
    public float critRate = 1f;
    public float lifeSteal = 1f;
    public float skillDamage = 1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // UUID 생성 로직도 GameManager로 이관했으므로 삭제
    }

    // ==========================================
    // 스탯 조작 함수들 (아이템 획득 시 호출됨)
    // ==========================================

    public void AddAttack(float amount)
    {
        attack += amount;
        Debug.Log($"공격력 증가! 현재: {attack}");
    }

    public void AddDefense(float amount)
    {
        defense += amount;
        Debug.Log($"방어력 증가! 현재: {defense}");
    }
}