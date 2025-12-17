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

    // ★ 추가: UI 스크립트 연결 (Inspector에서 GameUI 오브젝트 드래그)
    public GameUI gameUI;

    // 현재 스테이지 진행 상황
    public int spawnedCount = 0;
    public int killCount = 0;

    public Transform playerTransform; // 플레이어 위치 이동용 (Inspector에서 연결)
    void Awake()
    {
        if (Instance == null) Instance = this;
        // 나중에 여기서 서버 통신: 
        // currentStageIndex = Server.GetUserInfo().lastClearedStage + 1;
    }

    void Start()
    {
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

        // 시작하자마자 UI 갱신 (10 / 10)
        UpdateRemainUI();

        Debug.Log($"=== Stage {currentStageData.stageNumber} Start! ===");
        Debug.Log($"목표: {currentStageData.maxEnemyCount}마리 처치 / 방어력 보정: {currentStageData.defenseMultiplier * 100}%");
    }

    // 몬스터가 죽을 때마다 호출 (EnemyBehavior에서 호출 예정)
    public void OnEnemyKilled()
    {
        killCount++;

        // 죽을 때마다 UI 갱신
        UpdateRemainUI();

        if (killCount >= currentStageData.maxEnemyCount)
        {
            // 1. 현재 인덱스(0) 기준으로 "1탄 깼음"을 알림
            // currentStageIndex가 0이면 +1 해서 "1"을 넘김
            StageClearUI.Instance.ShowClearSequence(currentStageIndex + 1);
        }
    }

    // UI에서 연출이 끝난 뒤 부를 함수 분리
    public void LoadNextStage()
    {
        LoadStage(currentStageIndex + 1);
    }

    // 계산 & UI 호출 헬퍼 함수
    void UpdateRemainUI()
    {
        if (gameUI != null && currentStageData != null)
        {
            int remain = currentStageData.maxEnemyCount - killCount;
            // 음수 방지 (0까지만 표시)
            remain = Mathf.Max(0, remain);

            gameUI.UpdateRemainText(remain, currentStageData.maxEnemyCount);
        }
    }

    IEnumerator RestartRoutine()
    {
        Debug.Log("💀 플레이어 사망! 스테이지를 재시작합니다.");
        GameManager.Instance.isPlayerDead = true;
        // 1. ★ 조작 차단 (공격, 스킬, 이동 멈춤)
        SetPlayerControl(false);

        // 1. 조작 차단 (선택 사항)
        // playerController.enabled = false;

        // 2. 2초 정도 대기 (비장한 BGM이나 화면 붉어짐 효과 등)
        yield return new WaitForSeconds(2.0f);


        // 1. 맵 상의 모든 적 삭제
        // (EnemyBehavior 스크립트가 붙은 모든 객체를 찾아서 삭제)
        EnemyBehavior[] enemies = FindObjectsOfType<EnemyBehavior>();
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        // 2. 떨어진 아이템 모두 삭제
        // (DroppedItem 스크립트가 붙은 모든 객체 삭제)
        DroppedItem[] items = FindObjectsOfType<DroppedItem>();
        foreach (var item in items)
        {
            Destroy(item.gameObject);
        }

        // 3. 플레이어 위치 & 상태 리셋
        if (playerTransform != null)
        {
            playerTransform.position = Vector3.zero; // 맵 중앙(0,0)으로 이동

            // 체력 리셋 (Code Monkey 에셋 사용 시)
            // GetComponent<HealthSystemComponent>()를 찾아서 풀피로 만듦
            var healthComp = playerTransform.GetComponent<CodeMonkey.HealthSystemCM.HealthSystemComponent>();
            if (healthComp != null)
            {
                healthComp.GetHealthSystem().HealComplete(); // 체력 100% 회복 함수
            }
        }

        // 4. 스테이지 진행 상황 리셋
        spawnedCount = 0;
        killCount = 0;

        // UI 갱신 (남은 몹 수)
        if (gameUI != null)
        {
            // UpdateRemainText 함수가 public이어야 함
            gameUI.UpdateRemainText(currentStageData.maxEnemyCount, currentStageData.maxEnemyCount);
        }

        // 5. 스포너 코루틴 재시작이 필요하다면?
        // EnemySpawner는 Update에서 StageManager 상태를 보고 알아서 다시 뽑기 시작할 것임.
        // 하지만 만약 코루틴이 멈췄다면 다시 켜줘야 함. (지금 구조에선 괜찮음)
        
        // 5. ★ 조작 복구 (다시 움직이고 쏠 수 있게)
        SetPlayerControl(true);
        GameManager.Instance.isPlayerDead = false;
        Debug.Log("부활!");
    }

    public void RestartStage()
    {
        StartCoroutine(RestartRoutine());
       
    }

    void SetPlayerControl(bool isActive)
    {
        if (playerTransform == null) return;

        // 1. 기본 슈팅/조준 스크립트 끄기
        // (사용하시는 스크립트 이름이 'PlayerAimWeapon'이 아니라면 그 이름으로 바꾸세요!)
        // 예: PlayerController, ShootingController 등
        var aimScript = playerTransform.GetComponent<PlayerMovementAndAnimation>();
        if (aimScript != null) aimScript.enabled = isActive;

        // 2. 모든 스킬(AbilityBase) 끄기
        // (MeteorStrike 등 AbilityBase를 상속받은 모든 스킬이 꺼집니다)
        var abilities = playerTransform.GetComponents<AbilityBase>();
        foreach (var ability in abilities)
        {
            ability.enabled = isActive;
        }

        // (선택사항) 3. 이동까지 멈추고 싶다면?
        // var moveScript = playerTransform.GetComponent<PlayerMovement>();
        // if (moveScript != null) moveScript.enabled = isActive;

        // (선택사항) 4. 물리력(Rigidbody) 멈추기 (밀려남 방지)
        var rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (!isActive) rb.velocity = Vector2.zero; // 죽으면 멈춤
            rb.simulated = isActive; // 물리 연산 자체를 끄거나 켜기
        }
    }
}