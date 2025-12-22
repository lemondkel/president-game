using UnityEngine;
using System.Collections;
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

    [Header("정보보기 UI")]
    public GameObject infoObj;

    [Header("아이템 UI")]
    public GameObject itemObj;

    [Header("TextMeshProUGUI - 캐릭터정보 UI")]
    public TextMeshProUGUI infoCharHpText;
    public TextMeshProUGUI infoCharDefenseText;
    public TextMeshProUGUI infoCharAttackText;
    public TextMeshProUGUI infoCharSkillText;
    public TextMeshProUGUI infoCharSpeedText;
    public TextMeshProUGUI infoCharCriticalText;
    public TextMeshProUGUI infoCharLifeStealText;
    public TextMeshProUGUI infoCharHpRegenText;
    public TextMeshProUGUI infoCharKnockbackText;

    [Header("TextMeshProUGUI - 몬스터 UI")]
    public TextMeshProUGUI infoMonHpText;
    public TextMeshProUGUI infoMonDefenseText;
    public TextMeshProUGUI infoMonAttackText;

    [Header("스테이지 클리어 UI")]
    public GameObject stageClearObj;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 버튼 등에 연결할 메인 함수
    public void ToggleInfoUI()
    {
        if (infoObj.activeSelf)
        {
            // 닫을 때: 작아진 후 비활성화
            StartCoroutine(ScaleAnimation(Vector3.zero, false, infoObj));
        }
        else
        {
            // 열 때: 활성화 후 커짐
            infoObj.SetActive(true);
            infoCharHpText.text = "체력: " + GameManager.Instance.getMaxHp().ToString("0");
            infoCharDefenseText.text = "방어력: " + PlayerStats.Instance.defense.ToString("0");
            infoCharAttackText.text = "공격력: " + PlayerStats.Instance.attack.ToString("0");
            infoCharSkillText.text = "스킬 쿨타임: " + PlayerStats.Instance.cooldownReduction.ToString("0") + "%";
            infoCharSpeedText.text = "이동 속도: " + PlayerStats.Instance.moveSpeed.ToString("0") + "%";
            infoCharCriticalText.text = "치명타율: " + PlayerStats.Instance.critRate.ToString("0") + "%";
            infoCharLifeStealText.text = "생명력 흡수: " + PlayerStats.Instance.lifeSteal.ToString("0") + "%";
            infoCharHpRegenText.text = "체력 재생: " + PlayerStats.Instance.hpRegen.ToString("0");
            infoCharKnockbackText.text = "넉백 확률: " + PlayerStats.Instance.knockbackChance.ToString("0") + "%";

            infoMonHpText.text = "체력: " + StageManager.Instance.currentStageData.hpMultiplier;
            infoMonDefenseText.text = "방어력: " + StageManager.Instance.currentStageData.defenseMultiplier;
            infoMonAttackText.text = "공격력: " + StageManager.Instance.currentStageData.damageMultiplier;

            // 시작 크기를 0으로 설정
            infoObj.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleAnimation(Vector3.one, true, infoObj));
        }
    }

    // 버튼 등에 연결할 메인 함수
    public void ToggleClearUI()
    {
        if (stageClearObj.activeSelf)
        {
            // 닫을 때: 작아진 후 비활성화
            StartCoroutine(ScaleAnimation(Vector3.zero, false, stageClearObj));
        }
        else
        {
            // 열 때: 활성화 후 커짐
            stageClearObj.SetActive(true);

            // 시작 크기를 0으로 설정
            stageClearObj.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleAnimation(Vector3.one, true, stageClearObj));
        }
    }

    public void ToggleItemUI()
    {
        if (itemObj.activeSelf)
        {
            // 닫을 때: 작아진 후 비활성화
            StartCoroutine(ScaleAnimation(Vector3.zero, false, itemObj));
        }
        else
        {
            // 열 때: 활성화 후 커짐
            itemObj.SetActive(true);

            // 시작 크기를 0으로 설정
            itemObj.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleAnimation(Vector3.one, true, itemObj));
        }
    }

    // "띠용" 느낌을 주는 애니메이션 코루틴
    IEnumerator ScaleAnimation(Vector3 targetScale, bool isOpen, GameObject obj)
    {
        float duration = 0.2f; // 애니메이션 속도
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float percent = timer / duration;

            // 매트릭스/띠용 느낌을 주는 탄성 공식 (Animation Curve 대용)
            // 서서히 커지다가 살짝 과하게 커지고 다시 돌아오는 계산식입니다.
            float curve = Mathf.Sin(percent * Mathf.PI * 1.2f) * (1 - percent) + percent;

            // 닫힐 때는 단순히 작아지게, 열릴 때만 띠용 효과
            float bounce = isOpen ? curve : (1 - percent);

            obj.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, bounce);
            yield return null;
        }

        obj.transform.localScale = targetScale;

        // 닫기 애니메이션이 끝났다면 오브젝트 비활성화
        if (!isOpen)
        {
            obj.SetActive(false);
        }
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
