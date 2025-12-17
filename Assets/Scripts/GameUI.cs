using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    // 어디서든 접근 가능하게 싱글톤 처리 (선택사항이나 편의상 추천)
    public static GameUI Instance;

    [Header("Exp & Level")]
    public Slider expSlider;          // 경험치 바 (Slider)
    public TextMeshProUGUI levelText; // 레벨 텍스트 (Lv.1)

    [Header("New Stage UI")]
    public TextMeshProUGUI remainEnemyText; // ★ 추가: 남은 몬스터 수 텍스트

    [Header("골드 UI")]
    public TextMeshProUGUI currentGoldText; // ★ Gold Text

    [Header("다이아 UI")]
    public TextMeshProUGUI currentDiamondText; // ★ Diamond Text

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // ★ 핵심: 경험치와 레벨을 받아서 UI 갱신
    public void UpdateExpUI(int currentExp, int maxExp, int level)
    {
        // 1. 슬라이더 비율 계산 (0.0 ~ 1.0)
        // 정수끼리 나누면 0이 되므로 반드시 (float) 캐스팅 필요
        float ratio = (float)currentExp / maxExp;

        if (expSlider != null)
        {
            expSlider.value = ratio;
        }

        // 2. 레벨 텍스트 갱신
        if (levelText != null)
        {
            levelText.text = $"Lv.{level}";
        }
    }

    // 남은 몬스터 수 갱신 (아까 추가한 기능)
    public void UpdateRemainText(int count, int max)
    {
        if (remainEnemyText != null)
        {
            remainEnemyText.text = $"{count}";
        }
    }

    public void UpdateGoldText(long val)
    {
        if (currentGoldText != null)
        {
            // 그냥 val.ToString() 대신 포맷팅 함수 호출
            currentGoldText.text = GetUnitNumber(val);
        }
    }

    public void UpdateDiamondText(long val)
    {
        if (currentDiamondText != null)
        {
            // 그냥 val.ToString() 대신 포맷팅 함수 호출
            currentDiamondText.text = GetUnitNumber(val);
        }
    }

    // 숫자를 K, M, B, T 단위로 바꿔주는 헬퍼 함수
    string GetUnitNumber(long value)
    {
        // 1,000 미만은 그대로 표시
        if (value < 1000)
        {
            return value.ToString();
        }

        // T (Trillion, 조) : 1,000,000,000,000
        if (value >= 1000000000000)
        {
            // 0.### : 소수점 3자리까지 보여주되, 0이면 생략 (예: 1.5T, 1.234T)
            return (value / 1000000000000d).ToString("0.###") + "T";
        }

        // B (Billion, 십억) : 1,000,000,000
        if (value >= 1000000000)
        {
            return (value / 1000000000d).ToString("0.###") + "B";
        }

        // M (Million, 백만) : 1,000,000
        if (value >= 1000000)
        {
            return (value / 1000000d).ToString("0.###") + "M";
        }

        // K (Thousand, 천) : 1,000
        // 마지막 조건은 else로 처리해도 됨
        return (value / 1000d).ToString("0.###") + "K";
    }
}