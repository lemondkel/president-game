using UnityEngine;
using UnityEngine.UI; // ★ UI 관련 기능 필수
using TMPro; // 텍스트도 쓸 거면 필수

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image cooldownOverlayImage;  // 아까 만든 반투명 이미지

    [Header("Linked Ability")]
    // 이 슬롯이 보여줄 실제 스킬 스크립트 연결
    public AbilityBase linkedAbility;

    void Update()
    {
        // 연결된 스킬이 없으면 아무것도 안 함
        if (linkedAbility == null)
        {
            cooldownOverlayImage.fillAmount = 0f;
            return;
        }

        // 스킬이 쿨타임 중인지 확인
        if (linkedAbility.IsOnCooldown())
        {
            // 1. 오버레이 이미지 채우기 (비율에 맞춰서)
            // 쿨타임이 꽉 차서 시작 -> 점점 줄어듦 (1.0 -> 0.0)
            cooldownOverlayImage.fillAmount = linkedAbility.GetCooldownRatio();
        }
        else
        {
            // 쿨타임이 끝났으면 오버레이를 걷어냄
            cooldownOverlayImage.fillAmount = 0f;
        }
    }
}