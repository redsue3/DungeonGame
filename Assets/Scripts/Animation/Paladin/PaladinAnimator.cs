using UnityEngine;

// 성기사 전용 애니메이터
public class PaladinAnimator : CharacterAnimator
{
    [Header("성기사 이펙트")]
    public GameObject holyLightEffect;   // 성스러운 일격 빛 이펙트
    public GameObject shieldGlowEffect;  // 방패 강타 충격파
    public GameObject divineBeamEffect;  // 심판 빔

    public void OnHolyStrike()
    {
        if (holyLightEffect != null)
            Instantiate(holyLightEffect, transform.position + Vector3.right, Quaternion.identity);
    }

    public void OnShieldBash()
    {
        if (shieldGlowEffect != null)
            Instantiate(shieldGlowEffect, transform.position, Quaternion.identity);
    }

    public void OnDivineJudgment()
    {
        if (divineBeamEffect != null)
            Instantiate(divineBeamEffect, transform.position + Vector3.up, Quaternion.identity);
    }
}
