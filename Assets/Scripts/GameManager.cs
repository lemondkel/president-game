using UnityEngine;
using System; // Action 사용을 위해 필요
using CodeMonkey.HealthSystemCM;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 인스턴스

    [Header("Audio")]
    public AudioClip itemPickupClip; // ★ 여기에 MP3 연결
    private AudioSource audioSource;

    [Header("Player Status")]
    public int level = 1;
    public int currentExp = 0;

    // ★ 초기값 1로 설정 (1레벨에서 2레벨 갈 때 필요한 경험치)
    public int maxExp = 1;
    public long currentGold = 0;
    public long currentDiamond = 0;

    public bool isPlayerDead = false;

    // UI나 다른 시스템이 구독할 이벤트 (옵저버 패턴)
    public event Action<int, int> OnExpChange; // 현재경험치, 최대경험치
    public event Action<int> OnLevelUp;        // 바뀐 레벨

    [Header("References")]
    public Transform playerTransform; // ★ 플레이어 위치를 알아야 함
    public GameObject levelUpVfxPrefab; // ★ 생성할 이펙트 프리팹
    // ★ 선언: 여기에 플레이어를 넣을 거야
    public GameObject player;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>(); // 컴포넌트 가져오기
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 초기 UI 세팅을 위해 한번 호출
        NotifyExpChange();
    }

    void NotifyExpChange()
    {
        // UI에게 현재 상태 알림
        OnExpChange?.Invoke(currentExp, maxExp);
    }

    public void ApplyItemEffect(ItemType type)
    {
        // 1. 획득 사운드 재생 (PlayOneShot은 소리가 겹쳐도 끊기지 않음)
        if (audioSource != null && itemPickupClip != null)
        {
            Debug.Log("띠링!");
            // 피치(음정)를 살짝 랜덤으로 주면, 연속으로 먹을 때 '띠링, 띠링, 띠로롱' 하는 느낌이 남
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(itemPickupClip);
        }

        switch (type)
        {
            case ItemType.AttackPower:
                // 공격력 +1 로직
                Debug.Log("공격력 +1");
                break;
            case ItemType.Resurrection:
                // 부활권 +1
                break;
            case ItemType.Defense:
                // 방어력 증가
                break;
            case ItemType.Gold:
                // 돈 획득
                currentGold += ((StageManager.Instance.currentStageIndex + 1) + 1);
                GameUI.Instance.UpdateGoldText(currentGold);
                break;
            case ItemType.Diamond:
                // 다이아몬드 획득
                currentDiamond += 1;
                break;
            case ItemType.Magnet:
                // 자석 아이템 획득 (화면 내 모든 아이템 끌어오기 등)
                break;

                // ... 나머지 스탯들 % 증가 로직 ...
                // 예: PlayerStats.Instance.attackSpeed += 0.01f;
        }
    }

    // ★ 경험치 획득 메커니즘
    public void AddExp(int amount)
    {
        // ★ [가드 절] 죽었으면 경험치 로직 수행 안 함
        if (isPlayerDead) return;

        Debug.Log($"경험치 획득 ${amount}! , maxExp: ${maxExp}");
        currentExp += amount;

        // 경험치가 꽉 찼다면 레벨업 (반복문 사용: 한 번에 여러 레벨 업 가능)
        while (currentExp >= maxExp)
        {
            // 1. 현재 경험치 소모
            currentExp -= maxExp;

            // 2. 레벨 증가
            level++;

            // 3. ★ 핵심 로직 변경: 필요 경험치 2배로 증가 (1 -> 2 -> 4 -> 8 ...)
            maxExp *= 2;

            // (선택) 레벨업 효과음이나 이펙트
            Debug.Log($"Level Up! Lv.{level} (Next Exp: {maxExp})");

            // 플레이어의 컴포넌트 가져오기
            var hpComp = player.GetComponent<HealthSystemComponent>();

            // 레벨 설정 (자동으로 MaxHP 재계산됨)
            hpComp.SetLevel(level);

            // ★ 이펙트 생성 함수 호출
            PlayLevelUpEffect();
        }

        // ★ [UI 갱신 호출] 계산이 끝난 데이터를 UI에게 전달
        // GameUI가 싱글톤이면 이렇게 호출:
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateExpUI(currentExp, maxExp, level);
        }
    }

    void PlayLevelUpEffect()
    {
        if (levelUpVfxPrefab != null && playerTransform != null)
        {
            // 1. Z값 보정해서 생성
            Vector3 spawnPos = playerTransform.position;
            // spawnPos.z = 1500;
            GameObject vfx = Instantiate(levelUpVfxPrefab, spawnPos, Quaternion.identity);

            // 2. 부모 설정
            vfx.transform.SetParent(playerTransform);

            // ★ 파티클 시스템 내부 설정도 강제로 덮어쓰기 (알맹이 크기 조절)
            ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                // 알맹이 하나 크기를 0.2로 (기본값 1.0은 너무 큼)
                // main.startSize = 0.2f;
            }
        }
    }
}