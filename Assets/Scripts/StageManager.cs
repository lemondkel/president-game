using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using FreewrokGame;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Config")]
    public List<StageData> allStages; // Inspector에서 Stage1, Stage2 데이터 연결

    [Header("Current State")]
    public int currentStageIndex = 0; // 0부터 시작 (List Index)
    public StageData currentStageData;

    // UI 스크립트 연결 (Inspector에서 GameUI 오브젝트 드래그)
    public GameUI gameUI;

    // 현재 스테이지 진행 상황
    public int spawnedCount = 0;
    public int killCount = 0;

    public Transform playerTransform; // 플레이어 위치 이동용

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // 게임 시작 시, GameManager가 서버에서 로드한 스테이지 인덱스가 있다면 
        // GameManager가 이 변수(currentStageIndex)를 덮어씌운 뒤 LoadStage를 호출하거나
        // 여기서 로드합니다. (타이밍 이슈가 있다면 GameManager.ApplyServerData에서 LoadStage를 호출하는 것이 가장 정확합니다)
        LoadStage(currentStageIndex);
    }

    public void LoadStage(int index)
    {
        if (index >= allStages.Count)
        {
            Debug.Log("🏆 모든 스테이지 클리어! 게임 엔딩!");
            return;
        }

        currentStageIndex = index;
        currentStageData = allStages[index];

        // 상태 초기화
        spawnedCount = 0;
        killCount = 0;

        // 시작하자마자 UI 갱신
        UpdateRemainUI();

        Debug.Log($"=== Stage {currentStageData.stageNumber} Start! ===");
        Debug.Log($"목표: {currentStageData.maxEnemyCount}마리 처치");
    }

    // 몬스터가 죽을 때마다 호출 (EnemyBehavior에서 호출)
    public void OnEnemyKilled()
    {
        killCount++;

        // UI 갱신
        UpdateRemainUI();

        // 클리어 체크
        if (killCount >= currentStageData.maxEnemyCount)
        {
            // 1. 클리어 연출 (현재 인덱스 + 1 스테이지 클리어)
            if (StageClearUI.Instance != null)
            {
                StageClearUI.Instance.ShowClearSequence(currentStageIndex + 1);
            }

            // 2. ★ [수정됨] 서버로 데이터 전송 (GameManager에게 요청)
            if (GameManager.Instance != null)
            {
                Debug.Log("스테이지 클리어! 서버에 데이터 저장 요청...");
                GameManager.Instance.SaveStageData();
            }
        }
    }

    // UI에서 연출이 끝난 뒤 부를 함수
    public void LoadNextStage()
    {
        LoadStage(currentStageIndex + 1);
    }

    // UI 갱신 헬퍼
    void UpdateRemainUI()
    {
        if (gameUI != null && currentStageData != null)
        {
            int remain = currentStageData.maxEnemyCount - killCount;
            remain = Mathf.Max(0, remain); // 음수 방지

            gameUI.UpdateRemainText(remain, currentStageData.maxEnemyCount);
        }
    }

    // ==========================================
    // 사망 및 재시작 로직
    // ==========================================
    public void RestartStage()
    {
        StartCoroutine(RestartRoutine());
    }

    IEnumerator RestartRoutine()
    {
        Debug.Log("💀 플레이어 사망! 스테이지를 재시작합니다.");

        if (GameManager.Instance != null)
            GameManager.Instance.isPlayerDead = true;

        // 1. 조작 차단
        SetPlayerControl(false);

        // 2. 대기 (연출)
        yield return new WaitForSeconds(2.0f);

        // 3. 맵 상의 모든 적 삭제
        EnemyBehavior[] enemies = FindObjectsOfType<EnemyBehavior>();
        foreach (var enemy in enemies) Destroy(enemy.gameObject);

        // 4. 떨어진 아이템 삭제
        DroppedItem[] items = FindObjectsOfType<DroppedItem>();
        foreach (var item in items) Destroy(item.gameObject);

        // 5. 플레이어 위치 & 체력 리셋
        if (playerTransform != null)
        {
            playerTransform.position = Vector3.zero;

            var healthComp = playerTransform.GetComponent<CodeMonkey.HealthSystemCM.HealthSystemComponent>();
            if (healthComp != null)
            {
                healthComp.GetHealthSystem().HealComplete();
            }
        }

        // 6. 스테이지 진행 상황 리셋
        spawnedCount = 0;
        killCount = 0;

        // UI 갱신
        if (gameUI != null)
        {
            gameUI.UpdateRemainText(currentStageData.maxEnemyCount, currentStageData.maxEnemyCount);
        }

        // 7. 조작 복구
        SetPlayerControl(true);

        if (GameManager.Instance != null)
            GameManager.Instance.isPlayerDead = false;

        Debug.Log("부활!");
    }

    void SetPlayerControl(bool isActive)
    {
        if (playerTransform == null) return;

        // 조준/애니메이션 스크립트
        var aimScript = playerTransform.GetComponent<PlayerMovementAndAnimation>();
        if (aimScript != null) aimScript.enabled = isActive;

        // 스킬 스크립트
        var abilities = playerTransform.GetComponents<AbilityBase>();
        foreach (var ability in abilities) ability.enabled = isActive;

        // 물리 연산 제어
        var rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (!isActive) rb.velocity = Vector2.zero;
            rb.simulated = isActive;
        }
    }
}