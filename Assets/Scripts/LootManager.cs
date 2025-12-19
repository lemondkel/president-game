using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [Header("Resources")]
    public GameObject itemPrefab;
    public GameObject goldPrefab;   // 골드 전용 프리팹

    public Transform lootContainer;

    // 인스펙터 순서는 Enum ItemType 순서와 동일해야 함
    public List<Sprite> itemSprites;

    [Header("Bonus Drop Settings")]
    [Range(0, 100)] public float bonusGoldChance = 65f; // 65% 골드 추가 드랍 확률

    // 확률 테이블
    private Dictionary<ItemType, int> dropTable = new Dictionary<ItemType, int>()
    {
        { ItemType.AttackPower, 5 },
        { ItemType.NuckBack, 5 },
        { ItemType.Defense, 5 },
        { ItemType.Gold, 23 },
        { ItemType.Diamond, 23 },
        { ItemType.AttackSpeed, 5 },
        { ItemType.HpRegen, 5 },
        { ItemType.SkillCooldown, 12 },
        { ItemType.MoveSpeed, 5 },
        { ItemType.CritRate, 5 },
        { ItemType.LifeSteal, 5 },
        { ItemType.SkillDamage, 1 },
        { ItemType.MaxHp, 1 }
    };

    private int totalWeight;

    void Awake()
    {
        if (Instance == null) Instance = this;
        totalWeight = dropTable.Sum(x => x.Value);
    }

    public void SpawnLoot(Vector3 position)
    {
        // 1. 메인 아이템 1개 뽑기
        int randomValue = Random.Range(0, totalWeight);
        int currentSum = 0;
        ItemType selectedType = ItemType.Gold; // 기본값

        foreach (var item in dropTable)
        {
            currentSum += item.Value;
            if (randomValue < currentSum)
            {
                selectedType = item.Key;
                break;
            }
        }

        CreateItemObject(selectedType, position);

        // 2. 보너스 골드 드랍 (65%)
        if (Random.value <= (bonusGoldChance / 100f))
        {
            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            CreateItemObject(ItemType.Gold, position + randomOffset);
        }
    }

    // ★ [수정됨] 아이템 생성 및 초기화 로직
    void CreateItemObject(ItemType type, Vector3 pos)
    {
        GameObject prefabToUse = itemPrefab;

        // 골드면 전용 프리팹 사용
        if (type == ItemType.Gold && goldPrefab != null)
        {
            prefabToUse = goldPrefab;
        }

        if (prefabToUse == null) return;

        GameObject lootObj = Instantiate(prefabToUse, pos, Quaternion.identity, lootContainer);
        lootObj.transform.localScale = Vector3.one;

        DroppedItem itemScript = lootObj.GetComponent<DroppedItem>();

        if (itemScript != null)
        {
            Sprite icon = null;

            // 골드가 아니면 스프라이트 목록에서 이미지 찾기
            if (type != ItemType.Gold)
            {
                int spriteIndex = (int)type;
                if (spriteIndex >= 0 && spriteIndex < itemSprites.Count)
                {
                    icon = itemSprites[spriteIndex];
                }
            }

            // ★ [핵심 수정] 골드여도 Initialize를 반드시 호출해야 함!
            // 그래야 itemScript.type이 ItemType.Gold로 올바르게 설정됨.
            // icon이 null이면 이미지는 바뀌지 않으므로 골드 프리팹 원본 이미지가 유지됨.
            itemScript.Initialize(type, icon);
        }
    }
}