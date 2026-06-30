using UnityEngine;

// 전사 전용 애니메이터
// Sprite 슬롯은 Unity Inspector에서 연결
public class WarriorAnimator : CharacterAnimator
{
    [Header("전사 이펙트")]
    public GameObject slashEffect;    // 공격 시 칼날 이펙트
    public GameObject chargeEffect;   // 특수 차지 이펙트
    public GameObject impactEffect;   // 특수 착지 이펙트

    // 공격 애니메이션 이벤트에서 호출 (Animation Event)
    public void OnAttackHit()
    {
        if (slashEffect != null)
            Instantiate(slashEffect, transform.position, Quaternion.identity);
    }

    // 특수기 애니메이션 이벤트에서 호출
    public void OnSpecialCharge()
    {
        if (chargeEffect != null)
            Instantiate(chargeEffect, transform.position, Quaternion.identity);
    }

    public void OnSpecialImpact()
    {
        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);
    }
}
