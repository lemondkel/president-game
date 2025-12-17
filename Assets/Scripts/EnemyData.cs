using UnityEngine;

// Project 창에서 우클릭 > Create > Game > EnemyData 로 데이터 파일을 생성할 수 있게 됩니다.
[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Base Stats")]
    public float maxHp = 1f;
    public float attackPower = 1f;
    public float defense = 0f;
    public float moveSpeed = 1f;

    [Header("Rewards")]
    public int expReward = 1; // ★ 추가: 이 몬스터 잡으면 주는 경험치 (CowData에서 1로 설정)

    [Header("Spawn Settings")]
    public float spawnInterval = 1.0f;
}