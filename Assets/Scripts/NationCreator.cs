using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;

public class NationCreator : MonoBehaviour
{
    [Header("Server Config")]
    private string createNationUrl = "http://112.169.189.87:3000/api/nation/create";

    [Header("UI Inputs")]
    public TMP_InputField nameInput;        // 국가 이름 입력창
    public TMP_InputField descInput;        // 소개글 입력창
    public TMP_Dropdown joinTypeDropdown;   // 가입 방식 (0: 자유, 1: 승인) - 없으면 생략 가능

    [Header("Logo Preview UI")]
    public Image previewBase;   // 배경 이미지
    public Image previewSymbol; // 문양 이미지

    [Header("Logo Assets")]
    public List<Sprite> baseSprites;    // 배경 스프라이트 리스트
    public List<Sprite> symbolSprites;  // 문양 스프라이트 리스트
    public List<Color> presetColors;    // 색상 리스트

    // 현재 선택된 로고 데이터
    private LogoData currentData = new LogoData();

    // 탭 전환용 데이터
    private int currentTab = 0; // 0:모양, 1:문양, 2:색상

    [System.Serializable]
    public class LogoData
    {
        public int baseIndex = 0;
        public int symbolIndex = 0;
        public string colorHex = "#FFFFFF";
    }

    [Header("Selection UI")]
    public Transform contentArea;         // 아이템들이 나열될 그리드 Content
    public GameObject selectionBtnPrefab; // 생성할 버튼 프리팹

    void Start()
    {
        // 초기화: 첫 번째 탭(모양) 선택
        UpdatePreview();
        OnTabSelected(0);
    }

    // --- 탭 버튼 연결 함수 ---
    public void OnTabBaseShape() { OnTabSelected(0); }
    public void OnTabSymbolShape() { OnTabSelected(1); }
    public void OnTabColor() { OnTabSelected(2); }

    void OnTabSelected(int tabIndex)
    {
        currentTab = tabIndex;
        // 기존 버튼 삭제
        foreach (Transform child in contentArea) Destroy(child.gameObject);

        // 탭에 따라 버튼 생성
        switch (tabIndex)
        {
            case 0: // 배경
                GenerateShapeButtons(baseSprites, (idx) => {
                    currentData.baseIndex = idx;
                    UpdatePreview();
                });
                break;
            case 1: // 문양
                GenerateShapeButtons(symbolSprites, (idx) => {
                    currentData.symbolIndex = idx;
                    UpdatePreview();
                });
                break;
            case 2: // 색상
                GenerateColorButtons((col) => {
                    currentData.colorHex = "#" + ColorUtility.ToHtmlStringRGB(col);
                    UpdatePreview();
                });
                break;
        }
    }

    void UpdatePreview()
    {
        if (baseSprites != null && baseSprites.Count > currentData.baseIndex)
            previewBase.sprite = baseSprites[currentData.baseIndex];

        if (symbolSprites != null && symbolSprites.Count > currentData.symbolIndex)
            previewSymbol.sprite = symbolSprites[currentData.symbolIndex];

        if (ColorUtility.TryParseHtmlString(currentData.colorHex, out Color color))
            previewBase.color = color;

        previewSymbol.color = Color.white; // 문양은 흰색 고정
    }

    // --- 버튼 생성 헬퍼 ---
    void GenerateShapeButtons(List<Sprite> sprites, System.Action<int> action)
    {
        if (sprites == null) return;
        for (int i = 0; i < sprites.Count; i++)
        {
            int idx = i;
            GameObject btn = Instantiate(selectionBtnPrefab, contentArea);
            btn.GetComponent<Image>().sprite = sprites[i];
            btn.GetComponent<Button>().onClick.AddListener(() => action(idx));
        }
    }

    void GenerateColorButtons(System.Action<Color> action)
    {
        if (presetColors == null) return;
        foreach (var col in presetColors)
        {
            GameObject btn = Instantiate(selectionBtnPrefab, contentArea);
            btn.GetComponent<Image>().color = col;
            btn.GetComponent<Button>().onClick.AddListener(() => action(col));
        }
    }

    // --- ★ [핵심] 창설 버튼 클릭 시 실행 ---
    public void OnClickCreateNation()
    {
        string name = nameInput.text;
        string desc = descInput.text;

        // 드롭다운이 있으면 값 읽기, 없으면 기본값 "FREE"
        string joinType = "FREE";
        if (joinTypeDropdown != null)
        {
            joinType = joinTypeDropdown.value == 0 ? "FREE" : "APPROVAL";
        }

        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("국가 이름을 입력해주세요.");
            return;
        }

        // 로고 데이터 JSON 변환
        string logoJson = JsonUtility.ToJson(currentData);

        StartCoroutine(Co_CreateNation(name, desc, joinType, logoJson));
    }

    IEnumerator Co_CreateNation(string name, string desc, string joinType, string logoJson)
    {
        WWWForm form = new WWWForm();
        string myUuid = "";

        if (ProfileManager.Instance != null) myUuid = ProfileManager.Instance.uuid;
        else myUuid = SystemInfo.deviceUniqueIdentifier; // 테스트용

        form.AddField("uuid", myUuid);
        form.AddField("name", name);
        form.AddField("description", desc);
        form.AddField("joinType", joinType);
        form.AddField("logoData", logoJson);

        Debug.Log("국가 창설 요청 중...");

        using (UnityWebRequest www = UnityWebRequest.Post(createNationUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("성공: " + www.downloadHandler.text);

                // 성공 후 처리: 탭 이동 (내 국가 정보 탭으로)
                TabManager tm = FindObjectOfType<TabManager>();
                if (tm != null) tm.ShowTab(2); // 2번 탭(내 정보)으로 이동

                // (선택) ProfileManager 정보 갱신 요청
                if (ProfileManager.Instance != null)
                {
                    // 재화 정보 갱신 등을 위해 다시 로드
                    // ProfileManager.Instance.ReloadData(); // 해당 함수가 있다면 호출
                }
            }
            else
            {
                Debug.LogError("실패: " + www.downloadHandler.text);
                // 에러 메시지 파싱해서 팝업 띄우기 등
            }
        }
    }
}