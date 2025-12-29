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
    public string uuid;
    public int charId = 1001;

    [Header("Server Config")]
    private string baseUrl = "http://112.169.189.87:3000";
    private string saveUserInfoUrl => $"{baseUrl}/api/user/save-info";
    private string loadDataUrl => $"{baseUrl}/getUserData";
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

    [Header("Profile Display")]
    public RawImage profileDisplay;

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
        public string profileImage; // ★ 서버에서 추가된 컬럼 반영

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
            uuid = SystemInfo.deviceUniqueIdentifier;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        StartCoroutine(Co_LoadGameData());
    }

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
                if (response != null && response.result && response.@params != null)
                {
                    ApplyServerData(response.@params);
                }
            }
        }
    }

    void ApplyServerData(UserCharData data)
    {
        // ... (기타 스탯 동기화 로직 동일) ...
        hpInput.text = "체력: " + data.maxHp.ToString("0");
        defenseInput.text = "방어력: " + data.defense.ToString("0");
        attackInput.text = "공격력: " + data.attack.ToString("0");
        nickInput.text = data.nickname;
        stageInput.text = "Stage " + data.stageNumber.ToString("0");
        levelInput.text = "Lv." + data.level.ToString("0");

        // ★ 프로필 이미지 URL이 있다면 서버에서 다운로드해서 표시
        if (!string.IsNullOrEmpty(data.profileImage))
        {
            StartCoroutine(DownloadProfileImage(data.profileImage));
        }
    }

    // 서버 URL로부터 프로필 이미지 로드
    IEnumerator DownloadProfileImage(string relativeUrl)
    {
        string fullUrl = baseUrl + relativeUrl; // 예: http://...:3000/uploads/xxx.jpg
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(fullUrl))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(www);
                profileDisplay.texture = tex;
            }
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