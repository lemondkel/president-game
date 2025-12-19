using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.IO;
public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    [Header("User Info")]
    public string uuid; // 기기 고유 ID (자동 할당)
    public int charId = 1001; // 기본 캐릭터 ID

    [Header("Server Config")]
    // ★ 실제 배포 시에는 서버 IP로 변경 필요 (예: http://192.168.0.10:3000)
    private string baseUrl = "http://112.169.189.87:3000";
    private string saveUserInfoUrl => $"{baseUrl}/api/user/save-info";
    private string loadDataUrl => $"{baseUrl}/getUserData";

    private string uploadUrl => $"{baseUrl}/upload"; // 본인 서버 주소

    [Header("Player Status")]
    public int level = 1;
    public int currentExp = 0;
    public int maxExp = 100; // 초기값 (서버 데이터 로드 시 덮어씌워짐)
    public long currentGold = 0;
    public long currentDiamond = 0;

    // 이번 스테이지에서 획득한 누적 경험치 (저장 전 임시 보관)
    private int accumulatedExpInStage = 0;

    public bool isPlayerDead = false;

    [Header("캐릭터 정보")]
    public TextMeshProUGUI hpInput;
    public TextMeshProUGUI defenseInput;
    public TextMeshProUGUI hpRatioInput;
    public TextMeshProUGUI attackInput;
    public TextMeshProUGUI skillReduceInput;
    public TextMeshProUGUI speedInput;
    public TextMeshProUGUI criticalInput;
    public TextMeshProUGUI lifeStealInput;

    public TextMeshProUGUI nickInput;
    public TextMeshProUGUI stageInput;
    public TextMeshProUGUI levelInput;

    // ==========================================
    // DTO (Data Transfer Object) 클래스 정의
    // ==========================================
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
        // User Info
        public long gold;
        public long diamond;
        public int selectedCharId;
        public string nickname;

        // Character Info
        public int level;
        public int currentExp;
        public int maxExp;
        public int stageNumber;

        // Stats
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


    public RawImage profileDisplay; // 프로필이 표시될 RawImage (Image도 가능)

    void Awake()
    {
        Application.targetFrameRate = 60;

        // 싱글톤 패턴 설정
        if (Instance == null)
        {
            Instance = this;

            // UUID 생성/할당 (기기 고유 ID 사용)
            // 에디터 테스트 시 매번 바뀌는 것을 방지하려면 PlayerPrefs 사용 권장
            // 여기서는 SystemInfo.deviceUniqueIdentifier를 그대로 사용
            uuid = SystemInfo.deviceUniqueIdentifier;

            Debug.Log($"[ProfileManager] Init UUID: {uuid}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 게임 시작 시 서버에서 데이터 로드 (Login 개념)
        StartCoroutine(Co_LoadGameData());
    }

    // ==========================================
    // 1. 데이터 로드 (Load)
    // ==========================================
    IEnumerator Co_LoadGameData()
    {
        Debug.Log($"[Load] 데이터 불러오기 시도... (UUID: {uuid})");

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
                        // 서버 데이터로 게임 상태 동기화
                        ApplyServerData(response.@params);
                    }
                    else
                    {
                        Debug.LogWarning("[Load] 서버 응답은 성공했으나 데이터가 유효하지 않습니다. (신규 유저)");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Load] JSON 파싱 에러: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"[Load] 통신 실패: {www.error}. 로컬 기본값으로 시작합니다.");
            }
        }
    }

    // 서버 데이터를 각 시스템에 분배
    void ApplyServerData(UserCharData data)
    {
        // 1. ProfileManager 상태 갱신
        this.level = data.level;
        this.currentExp = data.currentExp;
        this.maxExp = data.maxExp;
        this.currentGold = data.gold;
        this.currentDiamond = data.diamond;
        this.charId = data.selectedCharId;

        // 2. StageManager 갱신
        if (StageManager.Instance != null)
        {
            // 서버 저장은 1부터, 배열 인덱스는 0부터이므로 -1 처리
            int stageIndex = Mathf.Max(0, data.stageNumber - 1);
            StageManager.Instance.currentStageIndex = stageIndex;
        }

        // 3. PlayerStats (전투 능력치) 갱신
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

        hpInput.text = "체력: " + data.maxHp.ToString("0");
        defenseInput.text = "방어력: " + data.defense.ToString("0");
        attackInput.text = "공격력: " + data.attack.ToString("0");
        speedInput.text = "이동 속도: " + data.moveSpeed.ToString("0") + "%";
        hpRatioInput.text = "체력 재생률: " + data.hpRegen.ToString("0") + "%";
        skillReduceInput.text = "스킬 쿨타임: " + data.cooldownReduction.ToString("0") + "%";
        criticalInput.text = "치명타: " + data.critRate.ToString("0") + "%";
        lifeStealInput.text = "생명력 흡수: " + data.lifeSteal.ToString("0") + "%";

        nickInput.text = data.nickname;
        stageInput.text = "Stage " + data.stageNumber.ToString("0");
        levelInput.text = "Lv." + data.level.ToString("0");

    Debug.Log("[ProfileManager] 모든 데이터 동기화 완료.");
    }

    // ==========================================
    // 4. 데이터 저장
    // ==========================================
    public void OnClickSaveConfirm()
    {
        StartCoroutine(Co_RequestSaveStageData(accumulatedExpInStage));
    }

    IEnumerator Co_RequestSaveStageData(int amount)
    {
        WWWForm form = new WWWForm();
        form.AddField("uuid", uuid);
        form.AddField("charId", charId);
        form.AddField("gainedExp", amount);

        // ★ 현재 상태 스냅샷 전송 (DB 덮어쓰기용)
        form.AddField("level", level);
        form.AddField("currentExp", currentExp);
        form.AddField("maxExp", maxExp);
        form.AddField("gold", currentGold.ToString());
        form.AddField("diamond", currentDiamond.ToString());

        if (StageManager.Instance != null)
        {
            form.AddField("stageNumber", StageManager.Instance.currentStageIndex + 1);
        }

        // PlayerStats의 값 수집
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

        using (UnityWebRequest www = UnityWebRequest.Post(saveUserInfoUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 응답 처리
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                if (response != null && response.result)
                {
                    // 저장 성공 시 누적 경험치 초기화
                    accumulatedExpInStage = 0;
                    Debug.Log("데이터 저장 완료 (Saved)");
                }
            }
            else
            {
                Debug.LogError($"데이터 저장 실패: {www.error}");
            }
        }
    }

    // 버튼 OnClick에 이 함수를 연결하세요
    public void OnClickChangeProfile()
    {
        // NativeGallery를 사용하여 이미지 선택
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                // 1. 화면에 표시 (NativeGallery의 유틸리티 사용)
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 512); // 최대 512 해상도로 로드
                if (texture != null)
                {
                    profileDisplay.texture = texture;

                    // 2. 서버로 전송 시작
                    StartCoroutine(UploadImageCoroutine(texture));
                }
            }
        }, "프로필 사진 선택", "image/*");
    }

    IEnumerator UploadImageCoroutine(Texture2D tex)
    {
        // 이미지를 JPG 바이트 배열로 변환
        byte[] imageData = tex.EncodeToJPG(80);

        // 머티리얼 폼 데이터 생성
        WWWForm form = new WWWForm();
        form.AddField("uuid", uuid);
        form.AddBinaryData("profile_image", imageData, "user_profile.jpg", "image/jpeg");

        using (UnityWebRequest www = UnityWebRequest.Post(uploadUrl, form))
        {
            Debug.Log("서버 업로드 중...");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("업로드 실패: " + www.error);
            }
            else
            {
                Debug.Log("업로드 완료! 서버 응답: " + www.downloadHandler.text);
            }
        }
    }

    void UpdateProfileImage(string filePath)
    {
        // 파일을 바이트 배열로 읽기
        byte[] fileData = File.ReadAllBytes(filePath);

        // 새로운 텍스트처 생성
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            // RawImage에 적용
            profileDisplay.texture = tex;

            // 만약 UI Image(Sprite)를 사용한다면 아래 주석 해제
            /*
            Rect rect = new Rect(0, 0, tex.width, tex.height);
            profileDisplay.GetComponent<Image>().sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
            */
        }
    }
}