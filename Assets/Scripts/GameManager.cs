using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using CodeMonkey.HealthSystemCM;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("User Info")]
    public string uuid; // 기기 고유 ID (자동 할당)
    public int charId = 1001; // 기본 캐릭터 ID

    [Header("Server Config")]
    private string baseUrl = "http://112.169.189.87:3000";
    private string gainExpUrl => $"{baseUrl}/api/character/gain-exp";
    private string loadDataUrl => $"{baseUrl}/getUserData";

    [Header("Audio")]
    public AudioClip itemPickupClip;
    private AudioSource audioSource;

    [Header("Player Status")]
    public int level = 1;
    public int currentExp = 0;
    public int maxExp = 1;
    public long currentGold = 0;
    public long currentDiamond = 0;
    public long currentStageNumber = 1;

    private int accumulatedExpInStage = 0;

    public bool isPlayerDead = false;

    public event Action<int, int> OnExpChange;
    public event Action<int> OnLevelUp;

    [Header("References")]
    public Transform playerTransform;
    public GameObject levelUpVfxPrefab;
    public GameObject player;

    [Serializable]
    public class ServerResponse
    {
        public bool result;
        public string msg;
        public UserCharData @params;
    }

    [Serializable]
    public class UserCharData
    {
        public long gold;
        public long diamond;
        public int selectedCharId;
        public int level;
        public int currentExp;
        public int maxExp;
        public int stageNumber;
        public float attack;
        public float defense;
        public float currentHp;
        public float maxHp;
        public float attackSpeed;
        public float moveSpeed;
        public float hpRegen;
        public float critRate;
        public float lifeSteal;
        public float skillDamage;
        public float cooldownReduction;
        public float knockbackChance;
    }

    void Awake()
    {
        Application.targetFrameRate = 60;
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            uuid = SystemInfo.deviceUniqueIdentifier;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(Co_LoadGameData());
    }

    IEnumerator Co_LoadGameData()
    {
        Debug.Log($"[Load] 데이터 불러오기... (UUID: {uuid})");
        WWWForm form = new WWWForm();
        form.AddField("uuid", uuid);
        form.AddField("charId", charId);

        using (UnityWebRequest www = UnityWebRequest.Post(loadDataUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
				string json = www.downloadHandler.text;
				Debug.Log($"[Load] 서버 응답: {json}");
                try
                {
                    ServerResponse response = JsonUtility.FromJson<ServerResponse>(json);
                    if (response != null && response.result && response.@params != null)
                    {
                        ApplyServerData(response.@params);
                    }
                    else
                    {
                        Debug.LogWarning("신규 유저 혹은 데이터 없음");
                        UpdateAllUI();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON 파싱 에러: {e.Message}");
                    UpdateAllUI();
                }
            }
            else
            {
                Debug.LogError($"통신 실패: {www.error}");
                UpdateAllUI();
            }
        }
    }

    // ★ [핵심] 서버 데이터 적용 로직
    void ApplyServerData(UserCharData data)
    {
        // 1. 변수 갱신
        this.level = data.level;
        this.currentExp = data.currentExp;
        this.currentStageNumber = data.stageNumber;
        StageClearUI.Instance.ShowClearSequence(data.stageNumber - 1);
        this.maxExp = data.maxExp;
        this.currentGold = data.gold;
        this.currentDiamond = data.diamond;
        this.charId = data.selectedCharId;

        // 2. StageManager 갱신 및 ★스테이지 로드 호출★
        if (StageManager.Instance != null)
        {
            int stageIndex = Mathf.Max(0, data.stageNumber - 1);
            StageManager.Instance.currentStageIndex = stageIndex;
            StageManager.Instance.LoadStage(stageIndex);

            Debug.Log($"[GameManager] 스테이지 로드 요청: {data.stageNumber} (Index: {stageIndex})");
        }

        // 3. PlayerStats 갱신
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.attack = data.attack;
            PlayerStats.Instance.defense = data.defense;
            PlayerStats.Instance.attackSpeed = data.attackSpeed;
            PlayerStats.Instance.moveSpeed = data.moveSpeed;
            PlayerStats.Instance.critRate = data.critRate;
            PlayerStats.Instance.hpRegen = data.hpRegen;
            PlayerStats.Instance.lifeSteal = data.lifeSteal;
            PlayerStats.Instance.skillDamage = data.skillDamage;
            PlayerStats.Instance.cooldownReduction = data.cooldownReduction;
            PlayerStats.Instance.knockbackChance = data.knockbackChance;
        }

        // 4. 체력 갱신
        if (player != null)
        {
            var hpComp = player.GetComponent<HealthSystemComponent>();
            if (hpComp != null)
            {
                var hs = hpComp.GetHealthSystem();
                hpComp.SetLevel(this.level); // 레벨 먼저
                hs.SetHealthMax(data.maxHp, false); // 최대 체력
                hs.SetHealth(data.currentHp); // 현재 체력
            }
        }
        UpdateAllUI();
        StageManager.Instance.UpdateRemainUI();
    }

    public void UpdateAllUI()
    {
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateExpUI(currentExp, maxExp, level);
            GameUI.Instance.UpdateGoldText(currentGold);
            GameUI.Instance.UpdateDiamondText(currentDiamond);
            GameUI.Instance.UpdateStageText(currentStageNumber);
        }
        NotifyExpChange();
    }

    void NotifyExpChange()
    {
        OnExpChange?.Invoke(currentExp, maxExp);
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateExpUI(currentExp, maxExp, level);
        }
    }

    public void AddExp(int amount)
    {
        if (isPlayerDead) return;
        accumulatedExpInStage += amount;
        currentExp += amount;
        CheckLocalLevelUp();
        NotifyExpChange();
    }

    private void CheckLocalLevelUp()
    {
        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            level++;
            maxExp *= 2;
            PerformLevelUpEffect();
        }
    }

    private void PerformLevelUpEffect()
    {
        if (player != null)
        {
            var hpComp = player.GetComponent<HealthSystemComponent>();
            if (hpComp != null) hpComp.SetLevel(level);
        }
        PlayLevelUpEffect();
        OnLevelUp?.Invoke(level);
    }

    void PlayLevelUpEffect()
    {
        if (levelUpVfxPrefab != null && playerTransform != null)
        {
            GameObject vfx = Instantiate(levelUpVfxPrefab, playerTransform.position, Quaternion.identity);
            vfx.transform.SetParent(playerTransform);
        }
    }

    public void SaveStageData()
    {
        if (accumulatedExpInStage <= 0) { }
        StartCoroutine(Co_RequestSaveStageData(accumulatedExpInStage));
    }

    IEnumerator Co_RequestSaveStageData(int amount)
    {
        WWWForm form = new WWWForm();
        form.AddField("uuid", uuid);
        form.AddField("charId", charId);
        form.AddField("gainedExp", amount);

        form.AddField("level", level);
        form.AddField("currentExp", currentExp);
        form.AddField("maxExp", maxExp);
        form.AddField("gold", currentGold.ToString());
        form.AddField("diamond", currentDiamond.ToString());

        if (StageManager.Instance != null)
            form.AddField("stageNumber", StageManager.Instance.currentStageIndex + 1);

        if (PlayerStats.Instance != null)
        {
            form.AddField("attack", PlayerStats.Instance.attack.ToString());
            form.AddField("defense", PlayerStats.Instance.defense.ToString());
            form.AddField("attackSpeed", PlayerStats.Instance.attackSpeed.ToString());
            form.AddField("moveSpeed", PlayerStats.Instance.moveSpeed.ToString());
            form.AddField("critRate", PlayerStats.Instance.critRate.ToString());
            form.AddField("hpRegen", PlayerStats.Instance.hpRegen.ToString());
            form.AddField("lifeSteal", PlayerStats.Instance.lifeSteal.ToString());
            form.AddField("skillDamage", PlayerStats.Instance.skillDamage.ToString());
            form.AddField("cooldownReduction", PlayerStats.Instance.cooldownReduction.ToString());
            form.AddField("knockbackChance", PlayerStats.Instance.knockbackChance.ToString());
        }

        if (player != null)
        {
            var hpComp = player.GetComponent<HealthSystemComponent>();
            if (hpComp != null)
            {
                var hs = hpComp.GetHealthSystem();
                form.AddField("currentHp", hs.GetHealth().ToString());
                form.AddField("maxHp", hs.GetHealthMax().ToString());
            }
        }

        using (UnityWebRequest www = UnityWebRequest.Post(gainExpUrl, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                accumulatedExpInStage = 0;
                Debug.Log("데이터 저장 완료 (Saved)");
            }
            else Debug.LogError($"저장 실패: {www.error}");
        }
    }

    public void ApplyItemEffect(ItemType type)
    {
        if (audioSource != null && itemPickupClip != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(itemPickupClip);
        }

        switch (type)
        {
            case ItemType.AttackPower: if (PlayerStats.Instance != null) PlayerStats.Instance.AddAttack(1f); break;
            case ItemType.Defense: if (PlayerStats.Instance != null) PlayerStats.Instance.AddDefense(1f); break;
            case ItemType.Gold:
                long goldAmount = (StageManager.Instance != null) ? (StageManager.Instance.currentStageIndex + 1) + 1 : 1;
                currentGold += goldAmount;
                if (GameUI.Instance != null) GameUI.Instance.UpdateGoldText(currentGold);
                break;
            case ItemType.Diamond:
                currentDiamond += 1;
                if (GameUI.Instance != null) GameUI.Instance.UpdateDiamondText(currentDiamond);
                break;
            case ItemType.AttackSpeed: if (PlayerStats.Instance != null) PlayerStats.Instance.attackSpeed += 1; break;
            case ItemType.CritRate: if (PlayerStats.Instance != null) PlayerStats.Instance.critRate += 1; break;
            case ItemType.HpRegen: if (PlayerStats.Instance != null) PlayerStats.Instance.hpRegen += 1; break;
            case ItemType.LifeSteal: if (PlayerStats.Instance != null) PlayerStats.Instance.lifeSteal += 1; break;
            case ItemType.MaxHp:
                if (PlayerStats.Instance != null && player != null)
                {
                    var hpComp = player.GetComponent<HealthSystemComponent>();
                    if (hpComp != null) hpComp.GetHealthSystem().SetHealthMax(hpComp.GetHealthSystem().GetHealthMax() + 1, false);
                }
                break;
            case ItemType.MoveSpeed: if (PlayerStats.Instance != null) PlayerStats.Instance.moveSpeed += 1; break;
            case ItemType.NuckBack: if (PlayerStats.Instance != null) PlayerStats.Instance.knockbackChance += 1; break;
            case ItemType.SkillCooldown: if (PlayerStats.Instance != null) PlayerStats.Instance.cooldownReduction += 1; break;
            case ItemType.SkillDamage: if (PlayerStats.Instance != null) PlayerStats.Instance.skillDamage += 1; break;
        }
    }
}
