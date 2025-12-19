// UserData.cs (새 파일 생성)
using System;

[Serializable] // JSON 직렬화를 위해 필수
public class UserData
{
    public string uuid;             // 기기 UUID
    public int charId;              // ★ [추가] 현재 캐릭터 ID (PlayerStats에서 참조 중)
    public int selectedCharId;      // ★ [추가] 선택된 캐릭터 ID (PlayerStats에서 참조 중)

    public int currentExp;          // 현재 경험치
    public int stageNumber;         // 현재 스테이지
    public float currentHp;         // 현재 HP
    public float maxHp;             // 최대 HP
    public float attack;            // 공격력
    public float defense;           // 방어력
    public float cooldownReduction; // 쿨타임 감소
    public float knockbackChance;   // 넉백 확률
    public long gold;               // 돈
    public long diamond;             // 다이아
    public float attackSpeed;       // 공격 속도
    public float hpRegen;           // 체력 재생
    public float moveSpeed;         // 이동 속도
    public float critRate;          // 치명타율
    public float lifeSteal;         // 생명력 흡수
    public float skillDamage;         // 스킬 대미지
}