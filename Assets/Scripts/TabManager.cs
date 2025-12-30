using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabManager : MonoBehaviour
{
    // ★ [핵심] 다른 씬에서 값을 설정할 수 있는 정적 변수
    // -1로 초기화하여 "요청 없음" 상태를 구분합니다.
    public static int RequestedTabIndex = -1;

    [Header("Settings")]
    [Tooltip("이 씬을 바로 실행하거나, 요청 없이 들어왔을 때 보여줄 기본 탭 번호")]
    public int defaultTabIndex = 0;

    [Header("Tab Panels (순서대로 연결)")]
    public GameObject[] tabPanels; // 0: 생성, 1: 검색, 2: 정보 등

    [Header("Tab Buttons")]
    public Button[] tabButtons;

    [Header("Tab Button Sprites")] // ★ 이미지 교체용 변수 추가
    public Sprite activeSprite;   // 활성화 시 이미지 (Bar_Fill_01)
    public Sprite inactiveSprite; // 비활성화 시 이미지 (Bar_Fill_02)

    void Start()
    {
        // 1. 다른 씬에서 요청이 있었는지 확인 (-1이 아니면 요청 있음)
        if (RequestedTabIndex != -1)
        {
            // 요청된 탭 열기
            ShowTab(RequestedTabIndex);

            // 사용 후 다시 "요청 없음(-1)" 상태로 초기화 (재진입 시 오작동 방지)
            RequestedTabIndex = -1;
        }
        else
        {
            // 2. 요청이 없으면 인스펙터에서 설정한 기본 탭 열기
            ShowTab(defaultTabIndex);
        }
    }

    // 버튼 OnClick 이벤트에 연결할 함수 (인자: 0, 1, 2...)
    public void ShowTab(int tabIndex)
    {
        // 범위 안전 체크
        if (tabPanels == null || tabPanels.Length == 0) return;
        if (tabIndex < 0 || tabIndex >= tabPanels.Length) tabIndex = 0;

        // 1. 모든 패널 비활성화 후 타겟만 활성화
        for (int i = 0; i < tabPanels.Length; i++)
        {
            if (tabPanels[i] != null)
            {
                bool isActive = (i == tabIndex);
                tabPanels[i].SetActive(isActive);
            }
        }

        // 2. 버튼 이미지 변경 (선택된 탭 강조)
        UpdateTabButtons(tabIndex);
    }

    private void UpdateTabButtons(int activeIndex)
    {
        if (tabButtons == null || tabButtons.Length == 0) return;

        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i] != null)
            {
                var image = tabButtons[i].GetComponent<Image>();
                if (image != null)
                {
                    // ★ 스프라이트가 설정되어 있다면 상태에 따라 이미지 교체
                    if (activeSprite != null && inactiveSprite != null)
                    {
                        image.sprite = (i == activeIndex) ? activeSprite : inactiveSprite;
                    }
                }

                // (선택 사항) 텍스트 색상도 바꾸고 싶다면 아래 주석 해제
                /*
                var text = tabButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null) text.color = (i == activeIndex) ? Color.black : Color.white;
                */
            }
        }
    }
}