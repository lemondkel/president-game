using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // 어디서든 접근 가능하게 싱글톤 처리

    [Header("Game Stats")]
    public int currentLevel = 1;
    public int killCount = 0;
    public int killsForNextLevel = 10; // 레벨업에 필요한 킬 수

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void AddKill()
    {
        killCount++;

        // 10마리 잡을 때마다 레벨업
        if (killCount % killsForNextLevel == 0)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentLevel++;
        Debug.Log("Level Up! Current Level: " + currentLevel);
        // 여기에 레벨업 이펙트나 사운드 추가 가능

    }
}