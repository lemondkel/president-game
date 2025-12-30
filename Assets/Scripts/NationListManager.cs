using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class NationListManager : MonoBehaviour
{
    [Header("Server")]
    private string listUrl = "http://112.169.189.87:3000/api/nation/list";

    [Header("UI References")]
    public Transform contentArea;       // 리스트가 들어갈 ScrollView Content
    public GameObject nationItemPrefab; // 리스트 아이템 프리팹
    public TMP_InputField searchInput;  // (선택) 검색창

    // JSON 파싱용 클래스 정의
    [System.Serializable]
    public class NationData
    {
        public long id;
        public string name;
        public string description;
        public int memberCount;
        public float totalPower; // ★ 추가된 전투력 필드
        public string joinType;  // "FREE" or "APPROVAL"
    }

    [System.Serializable]
    public class NationListResponse
    {
        public bool result;
        public List<NationData> list;
    }

    // 탭이 활성화될 때마다 목록 갱신
    void OnEnable()
    {
        RefreshList();
    }

    public void OnClickSearch()
    {
        string keyword = searchInput != null ? searchInput.text : "";
        RefreshList(keyword);
    }

    public void RefreshList(string keyword = "")
    {
        StartCoroutine(Co_GetNationList(keyword));
    }

    IEnumerator Co_GetNationList(string keyword)
    {
        // 검색어가 있으면 파라미터 추가
        string url = listUrl;
        if (!string.IsNullOrEmpty(keyword))
        {
            url += "?keyword=" + UnityWebRequest.EscapeURL(keyword);
        }

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 기존 목록 삭제
                foreach (Transform child in contentArea) Destroy(child.gameObject);

                string json = www.downloadHandler.text;
                Debug.Log("[NationList] 수신: " + json);

                try
                {
                    // JSON 파싱
                    NationListResponse response = JsonUtility.FromJson<NationListResponse>(json);

                    if (response != null && response.result && response.list != null)
                    {
                        // 데이터 바인딩
                        foreach (var nation in response.list)
                        {
                            GameObject itemObj = Instantiate(nationItemPrefab, contentArea);
                            NationListItem itemScript = itemObj.GetComponent<NationListItem>();

                            if (itemScript != null)
                            {
                                // ★ 수정된 SetInfo 호출 (전투력, 가입방식 포함)
                                itemScript.SetInfo(
                                    nation.id,
                                    nation.name,
                                    nation.description,
                                    nation.memberCount,
                                    nation.totalPower,
                                    nation.joinType
                                );
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("국가 리스트가 비어있거나 파싱 실패");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("JSON 파싱 에러: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("목록 조회 실패: " + www.error);
            }
        }
    }
}