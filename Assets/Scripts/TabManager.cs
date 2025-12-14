using UnityEngine;
using System.Collections.Generic;

public class TabManager : MonoBehaviour
{
    // 인스펙터에서 드래그하여 할당합니다.
    public List<GameObject> panels;
    public List<UnityEngine.UI.Button> tabButtons; // 또는 Toggle
    private int currentTabIndex = -1;

    void Start()
    {
        // 초기 설정: 첫 번째 탭을 활성화합니다.
        if (panels.Count > 0)
        {
            SwitchTab(0);
        }

        // 버튼 이벤트 리스너 연결 (Optional: 인스펙터에서 연결해도 됩니다)
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int tabIndex = i; // 클로저 이슈 방지를 위해 로컬 변수 사용
            tabButtons[i].onClick.AddListener(() => SwitchTab(tabIndex));
        }
    }

    // 탭 전환 메인 함수
    public void SwitchTab(int newTabIndex)
    {
        if (currentTabIndex == newTabIndex || newTabIndex < 0 || newTabIndex >= panels.Count)
        {
            return; // 이미 활성화된 탭이거나 유효하지 않은 인덱스
        }

        // 1. 이전 패널 비활성화
        if (currentTabIndex != -1)
        {
            panels[currentTabIndex].SetActive(false);
            // 선택된 버튼 상태 업데이트 로직 추가 가능
        }

        // 2. 새 패널 활성화
        currentTabIndex = newTabIndex;
        panels[currentTabIndex].SetActive(true);

        // 3. 새 버튼 상태 업데이트 (Toggle이 아닌 Button일 경우 강조 효과 등)
        // 예: tabButtons[currentTabIndex].GetComponent<Image>().color = Color.gray;
    }
}