using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;             // 스킬 아이콘 표시용
    public Image cooldownOverlayImage;  // 쿨타임 반투명 이미지

    // private으로 변경하여 인스펙터 실수를 방지
    private AbilityBase linkedAbility;

    // ★ 외부에서 스킬을 꽂아주는 함수
    public void SetAbility(AbilityBase ability)
    {
        this.linkedAbility = ability;

        // 아이콘 설정 (데이터에 아이콘이 있다면)
        if (ability != null && ability.data != null && iconImage != null)
        {
            iconImage.sprite = ability.data.icon;
            iconImage.enabled = true; // 아이콘 보이게
        }
        else if (iconImage != null)
        {
            // 스킬이 없거나 아이콘이 없으면 투명하게 처리
            iconImage.enabled = false;
        }
    }

    void Update()
    {
        if (linkedAbility == null)
        {
            if (cooldownOverlayImage != null) cooldownOverlayImage.fillAmount = 0f;
            return;
        }

        if (cooldownOverlayImage != null)
        {
            if (linkedAbility.IsOnCooldown())
            {
                cooldownOverlayImage.fillAmount = linkedAbility.GetCooldownRatio();
            }
            else
            {
                cooldownOverlayImage.fillAmount = 0f;
            }
        }
    }
}