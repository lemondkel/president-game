using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 이동을 위해 필수!

public class HeaderManager : MonoBehaviour
{
    // 이동하고 싶은 씬 이름을 인스펙터에서 적으세요 (예: "LobbyScene", "MainScene")
    public string targetSceneName = "InGameScene";

    // 버튼의 OnClick 이벤트에 연결할 함수
    public void LoadTargetScene()
    {
        // ★ 중요: 게임 도중에 일시정지(TimeScale 0) 상태로 나가는 경우를 대비해
        // 시간을 다시 흐르게 만들어줘야 다음 씬이 멈추지 않습니다.
        Time.timeScale = 1.0f;

        // 씬 로드
        SceneManager.LoadScene(targetSceneName);

        Debug.Log($"[Scene] {targetSceneName} 로드 중...");
    }

    // (옵션) 버튼마다 다른 씬으로 가고 싶다면 이걸 연결하세요
    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneName);
    }
}