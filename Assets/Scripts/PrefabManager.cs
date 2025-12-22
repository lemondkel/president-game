using UnityEngine;
using System.Collections.Generic;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance;

    [System.Serializable]
    public struct PrefabEntry
    {
        public string id;           // ID (예: "Enemy_Basic")
        public GameObject prefab;     // 프리팹
        public EnemyData defaultData; // ★ [추가] 이 몬스터의 기본 스탯 데이터
    }

    [Header("Registered Prefabs")]
    public List<PrefabEntry> enemyPrefabs;

    // ID로 검색하기 위한 딕셔너리
    private Dictionary<string, PrefabEntry> entryDict;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        entryDict = new Dictionary<string, PrefabEntry>();
        foreach (var entry in enemyPrefabs)
        {
            if (!entryDict.ContainsKey(entry.id))
            {
                entryDict.Add(entry.id, entry);
            }
        }
    }

    // ID로 프리팹 가져오기
    public GameObject GetPrefab(string id)
    {
        if (entryDict.ContainsKey(id)) return entryDict[id].prefab;
        Debug.LogWarning($"Prefab ID '{id}' not found!");
        return null;
    }

    // ★ [추가] ID로 데이터 가져오기
    public EnemyData GetData(string id)
    {
        if (entryDict.ContainsKey(id)) return entryDict[id].defaultData;
        return null;
    }
}