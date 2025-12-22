using UnityEngine;
using UnityEngine.Networking; // 서버 통신용
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System; // Action 사용을 위해 추가
using FreewrokGame;
using CodeMonkey.HealthSystemCM; // ★ 체력 시스템 사용을 위해 추가

// ==========================================
// 1. 서버 데이터 수신용 DTO 클래스들
// ==========================================
[System.Serializable]
public class StageResponse
{
    public bool result;
    public List<StageDTO> data;
}

[System.Serializable]
public class StageDTO
{
    public int stageNumber;
    public string stageName;
    public int maxEnemyCount;
    public float spawnInterval;
    public float hpMultiplier;
    public float damageMultiplier;
    public float defenseMultiplier;
    public List<EnemyDTO> enemyList;
}

[System.Serializable]
public class EnemyDTO
{
    public string name;
    public string prefabId; // "Enemy_Basic" 같은 문자열 ID
    public bool useTintColor;
    public ColorDTO tintColor;
}

[System.Serializable]
public class ColorDTO
{
    public float r;
    public float g;
    public float b;
    public float a;
}

// ==========================================
// 2. StageManager 메인 클래스
// ==========================================
public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Config")]
    public string serverUrl = "http://localhost:3000/api/admin/stages";
    public bool useServerData = true; // 서버 데이터 사용 여부

    [Header("Data Source")]
    public List<StageData> localStages; // 인스펙터에 넣은 로컬 데이터 (백업용)
    public List<StageData> serverStageList = new List<StageData>(); // 서버에서 받아 변환한 데이터

    [Header("Current State")]
    public int currentStageIndex = 0;
    public StageData currentStageData; // 현재 플레이 중인 데이터

    [Header("Stage Number UI")]
    public TextMeshProUGUI stageNumberText;

    public GameUI gameUI;

    public int spawnedCount = 0;
    public int killCount = 0;

    public Transform playerTransform;
    public bool isClearing = false;

    // ★ [추가] 스테이지 데이터 로드 완료 이벤트
    public event Action OnStageDataLoaded;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. 서버 데이터 사용 시, 먼저 다운로드 시도
        if (useServerData)
        {
            StartCoroutine(FetchStageData());
        }
        else
        {
            // 로컬 데이터 사용
            if (localStages.Count > 0)
            {
                // 로컬 데이터 로드 시에도 동일하게 처리
                serverStageList = new List<StageData>(localStages); // 로컬 데이터를 서버 리스트로 복사 (통일성 위해)
                LoadStage(currentStageIndex);
                OnStageDataLoaded?.Invoke(); // ★ 이벤트 발생
            }
        }
    }

    // ★ [추가] 서버에서 스테이지 목록 가져오기
    IEnumerator FetchStageData()
    {
        Debug.Log("서버에서 스테이지 데이터를 불러오는 중...");
        UnityWebRequest request = UnityWebRequest.Get(serverUrl);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log($"데이터 수신 성공: {json}");

            try
            {
                // JSON 파싱
                StageResponse response = JsonUtility.FromJson<StageResponse>(json);

                if (response != null && response.result && response.data != null)
                {
                    serverStageList.Clear();

                    // DTO -> ScriptableObject(Runtime Instance) 변환
                    foreach (var dto in response.data)
                    {
                        StageData newData = ConvertToStageData(dto);
                        serverStageList.Add(newData);
                    }

                    Debug.Log($"총 {serverStageList.Count}개의 스테이지 로드 완료.");

                    // 로드 완료 후 1스테이지 시작
                    LoadStage(0);
                    OnStageDataLoaded?.Invoke();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSON 파싱 에러: {e.Message}");
                // 실패 시 로컬 데이터 사용
                LoadStage(currentStageIndex);
                OnStageDataLoaded?.Invoke();
            }
        }
        else
        {
            Debug.LogError($"서버 통신 실패: {request.error}");
            // 실패 시 로컬 데이터 사용
            LoadStage(currentStageIndex);
            OnStageDataLoaded?.Invoke();
        }
    }

    // ★ [추가] DTO를 Unity StageData 포맷으로 변환
    StageData ConvertToStageData(StageDTO dto)
    {
        // 런타임에 ScriptableObject 인스턴스 생성
        StageData data = ScriptableObject.CreateInstance<StageData>();

        data.stageNumber = dto.stageNumber;
        data.stageName = dto.stageName;
        data.maxEnemyCount = dto.maxEnemyCount;
        data.spawnInterval = dto.spawnInterval;
        data.hpMultiplier = dto.hpMultiplier;
        data.damageMultiplier = dto.damageMultiplier;
        data.defenseMultiplier = dto.defenseMultiplier;

        data.enemyList = new List<StageData.StageEnemyEntry>();

        if (dto.enemyList != null)
        {
            foreach (var enemyDto in dto.enemyList)
            {
                StageData.StageEnemyEntry entry = new StageData.StageEnemyEntry();
                entry.name = enemyDto.name;

                // ★ [수정] 프리팹과 데이터를 모두 매니저에서 가져옵니다.
                if (PrefabManager.Instance != null)
                {
                    entry.prefab = PrefabManager.Instance.GetPrefab(enemyDto.prefabId);

                    // ★ 서버에서 데이터 ID를 따로 안 보내주므로, 
                    // 프리팹 ID와 짝지어진 기본 데이터를 사용합니다.
                    entry.data = PrefabManager.Instance.GetData(enemyDto.prefabId);
                }

                entry.useTintColor = enemyDto.useTintColor;
                if (enemyDto.tintColor != null)
                {
                    entry.tintColor = new Color(enemyDto.tintColor.r, enemyDto.tintColor.g, enemyDto.tintColor.b, enemyDto.tintColor.a);
                }
                else
                {
                    entry.tintColor = Color.white;
                }

                data.enemyList.Add(entry);
            }
        }

        return data;
    }


    // ==========================================
    // 기존 로직 유지
    // ==========================================

    public void LoadStage(int index)
    {
        // 사용할 리스트 결정 (서버 데이터가 있으면 그것을, 없으면 로컬 사용)
        List<StageData> targetList = (serverStageList.Count > 0) ? serverStageList : localStages;

        if (index >= targetList.Count)
        {
            Debug.Log("🏆 모든 스테이지 클리어! 게임 엔딩!");
            return;
        }

        if (index > 0)
        {
            // 이전 몹 정리 등 (필요 시)
        }

        currentStageIndex = index;
        currentStageData = targetList[index];

        spawnedCount = 0;
        killCount = 0;

        UpdateRemainUI();

        Debug.Log($"=== Stage {currentStageData.stageNumber} Start! ===");
        Debug.Log($"목표: {currentStageData.maxEnemyCount}마리 처치, 간격: {currentStageData.spawnInterval}초");
    }

    public void OnEnemyKilled()
    {
        if (isClearing) return;

        killCount++;
        UpdateRemainUI();

        if (killCount >= currentStageData.maxEnemyCount)
        {
            // 클리어 UI 표시
            if (StageClearUI.Instance != null)
            {
                StageClearUI.Instance.ShowClearSequence(currentStageIndex + 1);
            }

            // 서버 저장 요청
            if (GameManager.Instance != null)
            {
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

        // 청소
        isClearing = true;
        EnemyBehavior[] enemies = FindObjectsOfType<EnemyBehavior>();
        foreach (var enemy in enemies) Destroy(enemy.gameObject);
        DroppedItem[] items = FindObjectsOfType<DroppedItem>();
        foreach (var item in items) Destroy(item.gameObject);
        yield return null;
        isClearing = false;

        // 리셋
        if (playerTransform != null)
        {
            playerTransform.position = Vector3.zero;

            // CodeMonkey HealthSystem 컴포넌트 찾아서 완전 회복
            var healthComponent = playerTransform.GetComponent<HealthSystemComponent>();
            if (healthComponent != null)
            {
                healthComponent.GetHealthSystem().HealComplete();
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
    }

    void SetPlayerControl(bool isActive)
    {
        if (playerTransform == null) return;
        // 기존 컨트롤 제어 로직 유지...
    }
}