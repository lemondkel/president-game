using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using FreewrokGame;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Config")]
    public List<StageData> allStages;

    [Header("Current State")]
    public int currentStageIndex = 0;
    public StageData currentStageData;

    [Header("Stage Number UI")]
    public TextMeshProUGUI stageNumberText;

    public GameUI gameUI;

    public int spawnedCount = 0;
    public int killCount = 0;

    public Transform playerTransform;

    // ★ [추가] "청소 중"인지 확인하는 깃발
    public bool isClearing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // GameManager가 로드 후 실행해주겠지만, 안전장치로
        if (GameManager.Instance == null)
        {
            LoadStage(currentStageIndex);
        }
    }

    public void LoadStage(int index)
    {
        if (index >= allStages.Count)
        {
            Debug.Log("🏆 모든 스테이지 클리어! 게임 엔딩!");
            return;
        }

        // ★ [중요] 스테이지 넘어갈 때도 기존 몬스터가 있다면 지워줘야 겹치지 않습니다.
        // 만약 스테이지 이동 시에는 몬스터를 유지하고 싶다면 이 부분은 생략 가능합니다.
        // 하지만 보통은 새 스테이지 가면 이전 몹은 지웁니다.
        if (index > 0)
        {
            // StartCoroutine(ClearEnemiesWithoutReward()); // 필요하면 추가
        }

        currentStageIndex = index;
        currentStageData = allStages[index];

        spawnedCount = 0;
        killCount = 0;

        UpdateRemainUI();

        Debug.Log($"=== Stage {currentStageData.stageNumber} Start! ===");
        Debug.Log($"목표: {currentStageData.maxEnemyCount}마리 처치");
    }

    public void OnEnemyKilled()
    {
        // ★ [추가] 청소 중(재시작 중)이면 카운트도 올리지 않고, 클리어 체크도 안 함
        if (isClearing) return;

        killCount++;
        UpdateRemainUI();

        if (killCount >= currentStageData.maxEnemyCount)
        {
            if (StageClearUI.Instance != null)
            {
                StageClearUI.Instance.ShowClearSequence(currentStageIndex + 1);
            }

            if (GameManager.Instance != null)
            {
                Debug.Log("스테이지 클리어! 서버에 데이터 저장 요청...");
                GameManager.Instance.SaveStageData();
            }
        }
    }

    public void LoadNextStage()
    {
        LoadStage(currentStageIndex + 1);
    }

    public void UpdateRemainUI()
    {
        if (gameUI != null && currentStageData != null)
        {
            if (currentStageData.stageNumber != 1)
            {
                if (stageNumberText != null)
                    stageNumberText.text = "Stage " + currentStageData.stageNumber.ToString("0");
            }
            int remain = currentStageData.maxEnemyCount - killCount;
            remain = Mathf.Max(0, remain);

            gameUI.UpdateRemainText(remain, currentStageData.maxEnemyCount);
        }
    }

    public void RestartStage()
    {
        StartCoroutine(RestartRoutine());
    }

    IEnumerator RestartRoutine()
    {
        Debug.Log("💀 플레이어 사망! 스테이지를 재시작합니다.");

        if (GameManager.Instance != null)
            GameManager.Instance.isPlayerDead = true;

        SetPlayerControl(false);

        yield return new WaitForSeconds(2.0f);

        // ==========================================
        // ★ [수정] 청소 모드 ON (아이템 드랍 방지)
        // ==========================================
        isClearing = true;

        // 3. 맵 상의 모든 적 삭제
        EnemyBehavior[] enemies = FindObjectsOfType<EnemyBehavior>();
        foreach (var enemy in enemies) Destroy(enemy.gameObject);

        // 4. 떨어진 아이템 삭제
        DroppedItem[] items = FindObjectsOfType<DroppedItem>();
        foreach (var item in items) Destroy(item.gameObject);

        // Destroy가 완전히 처리될 때까지 1프레임 대기
        yield return null;

        // ★ 청소 모드 OFF
        isClearing = false;
        // ==========================================


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

        spawnedCount = 0;
        killCount = 0;

        if (gameUI != null)
        {
            gameUI.UpdateRemainText(currentStageData.maxEnemyCount, currentStageData.maxEnemyCount);
        }

        SetPlayerControl(true);

        if (GameManager.Instance != null)
            GameManager.Instance.isPlayerDead = false;

        Debug.Log("부활!");
    }

    void SetPlayerControl(bool isActive)
    {
        if (playerTransform == null) return;

        var aimScript = playerTransform.GetComponent<PlayerMovementAndAnimation>();
        if (aimScript != null) aimScript.enabled = isActive;

        var abilities = playerTransform.GetComponents<AbilityBase>();
        foreach (var ability in abilities) ability.enabled = isActive;

        var rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (!isActive) rb.velocity = Vector2.zero;
            rb.simulated = isActive;
        }
    }
}