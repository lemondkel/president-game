using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [Header("Resources")]
    public GameObject itemPrefab;
    public GameObject goldPrefab;   // ★ [추가] 골드 전용 프리팹

    public Transform lootContainer;

    // 인스펙터 순서는 Enum ItemType 순서와 동일해야 함
    public List<Sprite> itemSprites;

    [Header("Bonus Drop Settings")]
    [Range(0, 100)] public float bonusGoldChance = 65f; // 65% 골드 추가 드랍 확률

    // 확률 테이블
    private Dictionary<ItemType, int> dropTable = new Dictionary<ItemType, int>()
    {
        { ItemType.AttackPower, 5 },
        { ItemType.Resurrection, 5 },
        { ItemType.Defense, 5 },
        { ItemType.Gold, 23 },
        { ItemType.Diamond, 23 },
        { ItemType.AttackSpeed, 5 },
        { ItemType.HpRegen, 5 },
        { ItemType.SkillCooldown, 5 },
        { ItemType.MoveSpeed, 5 },
        { ItemType.CritRate, 5 },
        { ItemType.LifeSteal, 5 },
        { ItemType.TagCooldown, 2 },
        { ItemType.Magnet, 5 },
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
        // ==========================================
        // 1. [기존 로직] 테이블 돌려서 메인 아이템 1개 뽑기
        // ==========================================
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

        // 메인 아이템 생성
        CreateItemObject(selectedType, position);


        // ==========================================
        // 2. [추가 로직] 65% 확률로 골드 "보너스" 드랍
        // ==========================================
        if (Random.value <= (bonusGoldChance / 100f))
        {
            // 위치가 완전히 겹치면 안 예쁘니까 살짝(0.5f) 옆에 떨구기
            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

            // 골드 강제 생성
            CreateItemObject(ItemType.Gold, position + randomOffset);
        }
    }

    // ★ 코드 중복을 막기 위해 '생성하는 기능'만 따로 뺐습니다.
    void CreateItemObject(ItemType type, Vector3 pos)
    {
        // 1. 기본은 일반 프리팹
        GameObject prefabToUse = itemPrefab;

        // 2. ★ 골드 타입이면 골드 프리팹으로 교체
        if (type == ItemType.Gold && goldPrefab != null)
        {
            prefabToUse = goldPrefab;
        }

        if (prefabToUse == null) return;

        GameObject lootObj = Instantiate(prefabToUse, pos, Quaternion.identity, lootContainer);

        // ★ [추가] 크기가 작다면 여기서 강제로 키우세요!
        // (1, 1, 1)이 기본 크기이고, 더 크게 하고 싶으면 (1.5f, 1.5f, 1.5f) 등으로 설정
        lootObj.transform.localScale = Vector3.one;
        // lootObj.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); // 1.2배


        if (!(type == ItemType.Gold && goldPrefab != null))
        {
            DroppedItem itemScript = lootObj.GetComponent<DroppedItem>();
            // 골드가 아닌경우
            if (itemScript != null)
            {
                Sprite icon = null;
                int spriteIndex = (int)type;

                if (spriteIndex >= 0 && spriteIndex < itemSprites.Count)
                {
                    icon = itemSprites[spriteIndex];
                }

                // 초기화
                itemScript.Initialize(type, icon);
            }
        }
            
    }
}