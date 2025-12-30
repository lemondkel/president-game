using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NationListItem : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI titleText;   // "[Name] Description" 형태
    public TextMeshProUGUI powerText;   // 전투력 합계
    public TextMeshProUGUI countText;   // 국민 수

    [Header("Buttons")]
    public Button joinButton;    // 즉시 가입 버튼 (FREE일 때 Active)
    public Button requestButton; // 가입 신청 버튼 (APPROVAL일 때 Active)

    private long nationId;

    // 데이터 세팅 함수
    public void SetInfo(long id, string name, string desc, int count, float totalPower, string joinType)
    {
        nationId = id;

        // 1. 타이틀 포맷: [{name}] {description}
        // 만약 태그를 쓰고 싶다면 desc 대신 tag를 인자로 받아야 합니다.
        // 여기서는 요청하신 대로 name과 desc를 조합합니다.
        if (titleText) titleText.text = $"[{name}] {desc}";

        // 2. 전투력 및 국민 수
        if (powerText) powerText.text = $"전투력: {totalPower:N0}"; // 천단위 콤마
        if (countText) countText.text = $"국민: {count}명";

        // 3. 버튼 상태 제어
        bool isFreeJoin = (joinType == "FREE");

        if (joinButton)
        {
            joinButton.gameObject.SetActive(isFreeJoin);
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() => OnClickAction("Join"));
        }

        if (requestButton)
        {
            requestButton.gameObject.SetActive(!isFreeJoin);
            requestButton.onClick.RemoveAllListeners();
            requestButton.onClick.AddListener(() => OnClickAction("Request"));
        }
    }

    void OnClickAction(string actionType)
    {
        StartCoroutine(Co_JoinNation(actionType));
    }

    IEnumerator Co_JoinNation(string actionType)
    {
        string url = "http://112.169.189.87:3000/api/nation/join";
        WWWForm form = new WWWForm();

        string myUuid = SystemInfo.deviceUniqueIdentifier;
        if (ProfileManager.Instance != null) myUuid = ProfileManager.Instance.uuid;

        form.AddField("uuid", myUuid);
        form.AddField("nationId", nationId.ToString());
        form.AddField("message", actionType == "Join" ? "즉시 가입" : "가입 신청합니다.");

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"{actionType} 요청 성공: " + www.downloadHandler.text);
                // 가입 성공 후 처리 (예: 내 정보 탭으로 이동 or 팝업)
            }
            else
            {
                Debug.LogError($"{actionType} 요청 실패: " + www.downloadHandler.text);
            }
        }
    }
}