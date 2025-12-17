using UnityEngine;

namespace CodeMonkey.HealthSystemCM
{

    /// <summary>
    /// Adds a HealthSystem to a Game Object (Modified for Level & Item scaling)
    /// </summary>
    public class HealthSystemComponent : MonoBehaviour, IGetHealthSystem
    {

        [Header("Base Stats")]
        [Tooltip("1레벨일 때 기본 체력")]
        [SerializeField] private float baseHealth = 1f;

        [Tooltip("레벨업 할 때마다 오르는 체력량 (고정값)")]
        [SerializeField] private float healthPerLevel = 1f;

        // 내부 상태값 (State)
        private int currentLevel = 1;
        private float itemBonusFlat = 0f;    // 아이템으로 얻은 추가 체력 (+)
        private float itemBonusPercent = 0f; // 아이템으로 얻은 퍼센트 체력 (%)

        private HealthSystem healthSystem;

        private void Awake()
        {
            // 초기 체력 계산 (1레벨 기준)
            float initialMaxHealth = CalculateMaxHealth();

            // Health System 생성
            healthSystem = new HealthSystem(initialMaxHealth);

            // 시작 시 풀피로 설정
            healthSystem.SetHealth(initialMaxHealth);
        }

        /// <summary>
        /// 현재 스탯들을 종합하여 최대 체력을 다시 계산합니다.
        /// 공식: (기본 + (레벨성장치 * (레벨-1)) + 아이템합) * (1 + 아이템퍼센트)
        /// </summary>
        private float CalculateMaxHealth()
        {
            // 1레벨일 때 baseHealth여야 하므로 (currentLevel - 1)
            float levelBonus = (currentLevel - 1) * healthPerLevel;

            float flatTotal = baseHealth + levelBonus + itemBonusFlat;
            float multiplier = 1.0f + itemBonusPercent; // 예: 0.1(10%) -> 1.1

            return flatTotal * multiplier;
        }

        /// <summary>
        /// 스탯이 변경되었을 때 호출하여 최대 체력을 갱신합니다.
        /// </summary>
        public void UpdateStats()
        {
            if (healthSystem == null) return;

            float oldMax = healthSystem.GetHealthMax();
            float newMax = CalculateMaxHealth();

            // 최대 체력 변경 (CodeMonkey HealthSystem에 SetHealthMax가 있다고 가정)
            // true 파라미터는 "최대 체력이 늘어난 만큼 현재 체력도 채워줄 것인가?" 입니다.
            // 보통 레벨업하면 늘어난 통만큼은 채워주는 게 국룰입니다.
            healthSystem.SetHealthMax(newMax, true);

            Debug.Log($"[HP Update] Lv.{currentLevel} | MaxHP: {oldMax} -> {newMax}");
        }

        // ==========================================
        // 외부 호출용 API (Public Methods)
        // ==========================================

        /// <summary>
        /// 레벨업 시 호출 (GameManager에서 호출)
        /// </summary>
        public void SetLevel(int newLevel)
        {
            this.currentLevel = newLevel;
            UpdateStats();
        }

        /// <summary>
        /// 아이템 획득 시 호출 (+값)
        /// </summary>
        public void AddFlatHealth(float amount)
        {
            itemBonusFlat += amount;
            UpdateStats();
        }

        /// <summary>
        /// 아이템 획득 시 호출 (%값, 0.1f = 10%)
        /// </summary>
        public void AddPercentHealth(float percent)
        {
            itemBonusPercent += percent;
            UpdateStats();
        }

        public HealthSystem GetHealthSystem()
        {
            return healthSystem;
        }
    }
}