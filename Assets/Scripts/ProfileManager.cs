using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    [Header("User Info")]
    public string uuid;
    public int charId = 1001;

    [Header("Server Config")]
    private string baseUrl = "http://112.169.189.87:3000";
    private string loadDataUrl => $"{baseUrl}/getUserData";
    private string updateProfileUrl => $"{baseUrl}/api/user/update-profile";
    private string uploadUrl => $"{baseUrl}/upload";

    [Header("Player Stats UI")]
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

    [Header("Profile UI - Display")]
    public TextMeshProUGUI nicknameDisplay; // 평상시 화면에 보여지는 닉네임 (예: "Babo")
    public Image flagDisplay;               // 국기 이미지를 보여줄 UI Image
    public RawImage profileDisplay;         // 아바타 이미지

    [Header("Profile UI - Edit Inputs (Can be Hidden)")]
    [Tooltip("이 입력창들은 투명하게 하거나 화면 밖에 두어도 됩니다.")]
    public TMP_InputField nicknameInput;    // 실제 키패드를 호출할 숨겨진 입력창
    public TMP_InputField countryCodeInput; // 실제 키패드를 호출할 숨겨진 국가코드창

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
        public string nickname;
        public string countryCode;
        public string profileImage;
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
        if (Instance == null) { Instance = this; uuid = SystemInfo.deviceUniqueIdentifier; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        StartCoroutine(Co_LoadGameData());

        // 입력이 끝났을 때 자동으로 저장하고 싶다면 아래 리스너를 활용할 수 있습니다.
        if (nicknameInput != null)
            nicknameInput.onEndEdit.AddListener(delegate { OnClickUpdateProfile(); });

        if (countryCodeInput != null)
            countryCodeInput.onEndEdit.AddListener(delegate { OnClickUpdateProfile(); });
    }

    // 1. 데이터 로드 및 초기화
    IEnumerator Co_LoadGameData()
    {
        WWWForm form = new WWWForm();
        form.AddField("uuid", uuid);
        form.AddField("charId", charId);

        using (UnityWebRequest www = UnityWebRequest.Post(loadDataUrl, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                if (response != null && response.result)
                {
                    ApplyProfileData(response.@params);
                }
            }
        }
    }

    void ApplyProfileData(UserCharData data)
    {
         // ... (기타 스탯 동기화 로직 동일) ...
         hpInput.text = "체력: " + data.maxHp.ToString("0");
         defenseInput.text = "방어력: " + data.defense.ToString("0");
         attackInput.text = "공격력: " + data.attack.ToString("0");
         nickInput.text = data.nickname;
         stageInput.text = "Stage " + data.stageNumber.ToString("0");
         levelInput.text = "Lv." + data.level.ToString("0");

       // 닉네임 텍스트 적용
        if (nicknameDisplay != null) nicknameDisplay.text = data.nickname;

        // 숨겨진 입력창 데이터 미리 세팅
        if (nicknameInput != null) nicknameInput.text = data.nickname;
        if (countryCodeInput != null) countryCodeInput.text = data.countryCode;

        // 국기 로드
        UpdateFlagImage(data.countryCode);

        // 프로필 이미지 로드
        if (!string.IsNullOrEmpty(data.profileImage))
            StartCoroutine(DownloadAvatar(data.profileImage));
    }

    // ★ [핵심] Edit 이미지 버튼에 연결할 함수
    public void StartEditNickname()
    {
        if (nicknameInput != null)
        {
            nicknameInput.Select();
            nicknameInput.ActivateInputField(); // 이 명령이 내려지면 모바일 키패드가 올라옵니다.
        }
    }

    public void StartEditCountry()
    {
        if (countryCodeInput != null)
        {
            countryCodeInput.Select();
            countryCodeInput.ActivateInputField();
        }
    }

    // 국가 코드로 국기 이미지 변경
    void UpdateFlagImage(string code)
    {
        if (flagDisplay == null) return;
        if (string.IsNullOrEmpty(code)) code = "KOR";

        Sprite flagSprite = Resources.Load<Sprite>($"Flags/{code.ToUpper()}");
        if (flagSprite != null)
        {
            flagDisplay.sprite = flagSprite;
        }
        else
        {
            Debug.LogWarning($"[Profile] {code}에 해당하는 국기를 찾을 수 없습니다.");
        }
    }

    // 2. 닉네임 및 국가 코드 업데이트 (저장)
    public void OnClickUpdateProfile()
    {
        string newNick = nicknameInput.text;
        string newCountry = countryCodeInput.text;

        if (string.IsNullOrEmpty(newNick)) return;

        StartCoroutine(Co_UpdateProfile(newNick, newCountry));
    }

    IEnumerator Co_UpdateProfile(string nick, string country)
    {
        WWWForm form = new WWWForm();
        form.AddField("uuid", uuid);
        form.AddField("nickname", nick);
        form.AddField("countryCode", country.ToUpper());

        using (UnityWebRequest www = UnityWebRequest.Post(updateProfileUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("프로필 업데이트 완료");
                if (nicknameDisplay != null) nicknameDisplay.text = nick;
                UpdateFlagImage(country);
            }
        }
    }

    IEnumerator DownloadAvatar(string path)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(baseUrl + path))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
                profileDisplay.texture = DownloadHandlerTexture.GetContent(www);
        }
    }

    // [버튼 연결] 이미지 선택 및 업로드
    public void OnClickChangeProfile()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                // ★ 핵심 수정: 세 번째 인자 markTextureNonReadable을 'false'로 설정해야 수정/인코딩이 가능함
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false);

                if (texture != null)
                {
                    profileDisplay.texture = texture;
                    StartCoroutine(UploadImageCoroutine(texture));
                }
            }
        }, "프로필 사진 선택", "image/*");
    }

    IEnumerator UploadImageCoroutine(Texture2D tex)
    {
        // 이제 'not readable' 에러가 나지 않습니다.
        byte[] imageData = tex.EncodeToJPG(80);

        WWWForm form = new WWWForm();
        form.AddField("uuid", uuid);
        form.AddBinaryData("profile_image", imageData, "user_profile.jpg", "image/jpeg");

        using (UnityWebRequest www = UnityWebRequest.Post(uploadUrl, form))
        {
            Debug.Log("프로필 업로드 시도 중...");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("업로드 실패: " + www.error);
            }
            else
            {
                Debug.Log("업로드 성공: " + www.downloadHandler.text);
                // 업로드 성공 후 서버가 보낸 경로를 다시 확인하거나 할 수 있습니다.
            }
        }
    }
}
