using UnityEngine;

public class SkillUIManager : MonoBehaviour
{
    [Header("Target Player")]
    public GameObject player; // 스킬이 붙어있는 플레이어 오브젝트

    [Header("UI Slots")]
    public SkillSlotUI[] skillSlots; // UI에 만들어둔 슬롯들 (순서대로 연결)

    void Start()
    {
        if (player == null)
        {
            // 플레이어가 할당 안 되어있으면 태그로라도 찾기
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null) player = foundPlayer;
        }

        if (player != null)
        {
            ConnectSkillsToUI();
        }
    }

    void ConnectSkillsToUI()
    {
        // 1. 플레이어에 붙은 모든 스킬 컴포넌트(부모 포함) 가져오기
        AbilityBase[] skills = player.GetComponents<AbilityBase>();

        Debug.Log($"[SkillUI] 플레이어에게서 {skills.Length}개의 스킬을 발견했습니다.");

        // 2. 슬롯 개수와 스킬 개수 중 작은 만큼 반복
        int count = Mathf.Min(skills.Length, skillSlots.Length);

        for (int i = 0; i < count; i++)
        {
            // ★ i번째 슬롯에 i번째 스킬을 연결
            skillSlots[i].SetAbility(skills[i]);
            Debug.Log($"[SkillUI] 슬롯 {i}번에 {skills[i].data.skillName} 연결됨.");
        }

        // 남는 슬롯은 비워주기
        for (int i = count; i < skillSlots.Length; i++)
        {
            skillSlots[i].SetAbility(null);
        }
    }
}