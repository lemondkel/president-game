using UnityEngine;
using TMPro;
using System.Collections;

public class StageClearUI : MonoBehaviour
{
    public static StageClearUI Instance;

    [Header("UI Objects")]
    public GameObject panelObj;
    public Animator successAnim;

    [Header("Stage Text Panels")]
    public TextMeshProUGUI textCurrent;

    void Awake()
    {
        if (Instance == null) Instance = this;
        panelObj.SetActive(false);
    }

    public void ShowClearSequence(int clearedStageNum)
    {
        // ★ [핵심 해결] 코루틴을 시작하기 전에 오브젝트를 먼저 켜야 합니다!
        // (만약 이 스크립트가 panelObj에 붙어있다면, 이게 꺼져있어서 코루틴이 불발된 것임)
        if (panelObj != null)
        {
            panelObj.SetActive(true);
        }

        // 이제 켜졌으니 코루틴 실행 가능
        StartCoroutine(SequenceRoutine(clearedStageNum));
    }

    IEnumerator SequenceRoutine(int clearedStage)
    {
        // [삭제] 시간을 멈추는 코드 제거! -> 이제 이펙트가 계속 움직입니다.
        // Time.timeScale = 0f; 

        // 텍스트 세팅
        textCurrent.text = $"Stage {clearedStage+1}";

        // UI 켜기
        panelObj.SetActive(true);
        if (successAnim != null) successAnim.SetTrigger("Play");

        // [삭제] 2초 대기 제거! -> 바로 넘어갑니다.
        // yield return new WaitForSecondsRealtime(3.0f); 

        // (선택) UI가 번쩍하고 사라지는 게 싫다면 0.5초 정도만 짧게 줘도 됩니다.
        // yield return new WaitForSeconds(0.5f); 

        // 정리
        panelObj.SetActive(false);

        // ★ [수정 전] 수학 계산이 꼬여서 인덱스 2(3탄)를 부르고 있었음
        // int nextStage = clearedStage + 1;
        // if (HasStageData(nextStage)) { StageManager.Instance.LoadStage(nextStage); }

        // ★ [수정 후] 그냥 매니저한테 "다음 거 내놔" 라고 위임 (가장 안전)
        // 매니저가 알아서 (CurrentIndex + 1)을 로드함
        if (StageManager.Instance != null)
        {
            StageManager.Instance.LoadNextStage();
        }

        yield return null;
    }

    bool HasStageData(int stageNum)
    {
        if (StageManager.Instance == null) return false;
        // stageNum은 1부터 시작, 리스트 인덱스는 0부터 시작
        int index = stageNum - 1;
        return index >= 0 && index < StageManager.Instance.serverStageList.Count;
    }
}