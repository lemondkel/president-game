using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    // ��𼭵� ���� �����ϰ� �̱��� ó�� (���û����̳� ���ǻ� ��õ)
    public static GameUI Instance;

    [Header("Exp & Level")]
    public Slider expSlider;          // ����ġ �� (Slider)
    public TextMeshProUGUI levelText; // ���� �ؽ�Ʈ (Lv.1)

    [Header("���� �� UI")]
    public TextMeshProUGUI remainEnemyText; // �� �߰�: ���� ���� �� �ؽ�Ʈ

    [Header("��� UI")]
    public TextMeshProUGUI currentGoldText; // �� Gold Text

    [Header("���̾� UI")]
    public TextMeshProUGUI currentDiamondText; // �� Diamond Text

    [Header("Stage UI")]
    public TextMeshProUGUI currentStageText;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // �� �ٽ�: ����ġ�� ������ �޾Ƽ� UI ����
    public void UpdateExpUI(int currentExp, int maxExp, int level)
    {
        // 1. �����̴� ���� ��� (0.0 ~ 1.0)
        // �������� ������ 0�� �ǹǷ� �ݵ�� (float) ĳ���� �ʿ�
        float ratio = (float)currentExp / maxExp;

        if (expSlider != null)
        {
            expSlider.value = ratio;
        }

        // 2. ���� �ؽ�Ʈ ����
        if (levelText != null)
        {
            levelText.text = $"Lv.{level}";
        }
    }

    // ���� ���� �� ���� (�Ʊ� �߰��� ���)
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
            // �׳� val.ToString() ��� ������ �Լ� ȣ��
            currentGoldText.text = GetUnitNumber(val);
        }
    }

    public void UpdateDiamondText(long val)
    {
        if (currentDiamondText != null)
        {
            // �׳� val.ToString() ��� ������ �Լ� ȣ��
            currentDiamondText.text = GetUnitNumber(val);
        }
    }

    public void UpdateStageText(long val)
    {
        if (currentStageText != null)
        {
            currentStageText.text = "Stage " + val.ToString("0");
        }
    }

    // ���ڸ� K, M, B, T ������ �ٲ��ִ� ���� �Լ�
    string GetUnitNumber(long value)
    {
        // 1,000 �̸��� �״�� ǥ��
        if (value < 1000)
        {
            return value.ToString();
        }

        // T (Trillion, ��) : 1,000,000,000,000
        if (value >= 1000000000000)
        {
            // 0.### : �Ҽ��� 3�ڸ����� �����ֵ�, 0�̸� ���� (��: 1.5T, 1.234T)
            return (value / 1000000000000d).ToString("0.###") + "T";
        }

        // B (Billion, �ʾ�) : 1,000,000,000
        if (value >= 1000000000)
        {
            return (value / 1000000000d).ToString("0.###") + "B";
        }

        // M (Million, �鸸) : 1,000,000
        if (value >= 1000000)
        {
            return (value / 1000000d).ToString("0.###") + "M";
        }

        // K (Thousand, õ) : 1,000
        // ������ ������ else�� ó���ص� ��
        return (value / 1000d).ToString("0.###") + "K";
    }
}
