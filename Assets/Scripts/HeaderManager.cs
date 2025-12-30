using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 이동을 위해 필수!

public class HeaderManager : MonoBehaviour
{
    // 이동하고 싶은 씬 이름을 인스펙터에서 적으세요 (예: "LobbyScene", "ClanCreateScene")
    public string targetSceneName = "InGameScene";

    // ==========================================
    // 1. 기존 단순 씬 이동 기능
    // ==========================================

    // 버튼의 OnClick 이벤트에 연결할 함수 (단순 이동)
    public void LoadTargetScene()
    {
        // ★ 중요: 게임 도중에 일시정지(TimeScale 0) 상태로 나가는 경우를 대비
        Time.timeScale = 1.0f;

        SceneManager.LoadScene(targetSceneName);
        Debug.Log($"[Scene] {targetSceneName} 로드 중...");
    }

    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneName);
    }

    // ==========================================
    // 2. 동적 탭 이동 기능 (확장됨)
    // ==========================================

    // ★ [동적] 인스펙터 버튼 연결용 함수
    // 사용법: 버튼 OnClick에 연결하고, 숫자(0, 1, 2...)를 입력하세요.
    // 이동할 씬은 위 'targetSceneName' 변수에 적힌 곳으로 갑니다.
    public void LoadTargetSceneWithTab(int tabIndex)
    {
        LoadSceneWithTab(targetSceneName, tabIndex);
    }

    // ★ [동적] 스크립트 호출용 완전 동적 함수
    // 사용법: HeaderManager.Instance.LoadSceneWithTab("ClanCreateScene", 2);
    public void LoadSceneWithTab(string sceneName, int tabIndex)
    {
        Time.timeScale = 1.0f;

        // ★ [핵심] TabManager의 정적 변수(static)에 "도착하면 이 탭 열어줘"라고 메모 남기기
        TabManager.RequestedTabIndex = tabIndex;

        // 지정된 씬 로드
        SceneManager.LoadScene(sceneName);
        Debug.Log($"[Scene] {sceneName} 로드 (요청 탭: {tabIndex})");
    }

    // ==========================================
    // 3. 편의용 하드코딩 함수 (기존 유지)
    // ==========================================

    public void GoToClanCreate() { LoadSceneWithTab("ClanCreateScene", 0); }
    public void GoToClanSearch() { LoadSceneWithTab("ClanCreateScene", 1); }
}