using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static InventoryManager Instance;

    [Header("Server Config")]
    [SerializeField] private string baseUrl = "http://112.169.189.87:3000";

    [Header("UI References")]
    public Transform contentArea;       // ScrollView > Viewport > Content 트랜스폼
    public GameObject itemPrefab;       // 생성할 아이템 UI 프리팹

    [Header("Default Settings")]
    public int defaultCharId = 1001;    // ProfileManager가 없을 때 사용할 기본 캐릭터 ID

    // 아이템 데이터를 담기 위한 임시 클래스
    private class ItemData
    {
        public string technicalName; // 서버에서 온 이름 (예: AttackPower)
        public string displayName;   // 표시될 이름 (예: 공격력 증가)
        public int count;            // 개수
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 씬 시작 시 인벤토리 로드
        LoadInventory();
    }

    public void LoadInventory()
    {
        string targetUuid = "";
        int targetCharId = defaultCharId;

        if (ProfileManager.Instance != null)
        {
            targetUuid = ProfileManager.Instance.uuid;
            targetCharId = ProfileManager.Instance.charId;
        }
        else
        {
            targetUuid = SystemInfo.deviceUniqueIdentifier;
        }

        StartCoroutine(Co_GetInventory(targetUuid, targetCharId));
    }

    IEnumerator Co_GetInventory(string uuid, int charId)
    {
        string url = $"{baseUrl}/api/character/inventory/{uuid}/{charId}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[Inventory] 데이터 수신: {www.downloadHandler.text}");
                ClearInventoryUI();
                ParseAndPopulateUI(www.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Inventory] 서버 통신 실패: {www.error}");
            }
        }
    }

    void ClearInventoryUI()
    {
        if (contentArea == null) return;
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }
    }

    void ParseAndPopulateUI(string json)
    {
        try
        {
            if (!json.Contains("\"inventory\":{")) return;

            // 1. JSON 문자열에서 인벤토리 부분만 추출
            int startIndex = json.IndexOf("\"inventory\":{") + 12;
            int endIndex = json.LastIndexOf("}");
            string inventoryPart = json.Substring(startIndex, endIndex - startIndex);

            // 2. 특수문자 제거 ({, }, ")
            string cleanPart = inventoryPart.Replace("\"", "").Replace("{", "").Replace("}", "").Trim();
            if (string.IsNullOrEmpty(cleanPart)) return;

            // 3. 콤마로 분리하여 리스트에 수집
            string[] itemsRaw = cleanPart.Split(',');
            List<ItemData> itemList = new List<ItemData>();

            foreach (string raw in itemsRaw)
            {
                string[] pair = raw.Split(':');
                if (pair.Length < 2) continue;

                string techName = pair[0].Trim();
                if (int.TryParse(pair[1].Trim(), out int count) && count > 0)
                {
                    itemList.Add(new ItemData
                    {
                        technicalName = techName,
                        displayName = GetKoreanName(techName),
                        count = count
                    });
                }
            }

            // 4. 정렬 (이름순 혹은 골드 우선 등 원하는 로직 적용 가능)
            itemList.Sort((a, b) => a.displayName.CompareTo(b.displayName));

            // 5. UI 생성
            foreach (var item in itemList)
            {
                CreateItemUI(item);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Inventory] 파싱 오류: {e.Message}");
        }
    }

    // 기술적인 영문 이름을 한국어로 매핑하는 함수
    string GetKoreanName(string techName)
    {
        switch (techName)
        {
            case "Gold": return "골드";
            case "Diamond": return "다이아몬드";
            case "AttackPower": return "공격력 증가";
            case "Defense": return "방어력 증가";
            case "AttackSpeed": return "공격 속도 증가";
            case "CritRate": return "치명타 확률 증가";
            case "HpRegen": return "체력 재생 증가";
            case "LifeSteal": return "생명력 흡수 증가";
            case "MaxHp": return "최대 체력 증가";
            case "MoveSpeed": return "이동 속도 증가";
            case "NuckBack": return "넉백 확률 증가";
            case "SkillCooldown": return "스킬 재사용 대기시간 감소";
            case "SkillDamage": return "스킬 데미지 증가";
            default: return techName; // 매핑되지 않은 경우 원래 이름 반환
        }
    }

    void CreateItemUI(ItemData item)
    {
        if (itemPrefab == null || contentArea == null) return;

        GameObject newItem = Instantiate(itemPrefab, contentArea);

        // 자식 오브젝트에서 텍스트와 이미지 컴포넌트 찾기
        TMP_Text nameText = newItem.transform.Find("ItemName")?.GetComponent<TMP_Text>();
        TMP_Text countText = newItem.transform.Find("ItemCount")?.GetComponent<TMP_Text>();
        Image iconImage = newItem.transform.Find("Icon")?.GetComponent<Image>();

        if (nameText != null) nameText.text = item.displayName;
        if (countText != null) countText.text = "x" + string.Format("{0:N0}", item.count); // 천단위 콤마 추가

        // Resources/Icons 폴더에서 원본 영문 이름과 일치하는 이미지 로드
        if (iconImage != null)
        {
            Sprite icon = Resources.Load<Sprite>($"Icons/{item.technicalName}");
            if (icon != null) iconImage.sprite = icon;
        }
    }
}