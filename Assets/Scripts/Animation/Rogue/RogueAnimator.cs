using UnityEngine;

// 도적 전용 애니메이터
public class RogueAnimator : CharacterAnimator
{
    [Header("도적 이펙트")]
    public GameObject dashTrail;       // 이동 잔상
    public GameObject slashCombo;      // 연격 이펙트
    public GameObject shadowEffect;    // 특수 그림자 분신 이펙트

    public void OnDashStart()
    {
        if (dashTrail != null)
            Instantiate(dashTrail, transform.position, Quaternion.identity);
    }

    public void OnAttackHit()
    {
        if (slashCombo != null)
            Instantiate(slashCombo, transform.position, Quaternion.identity);
    }

    public void OnSpecialActivate()
    {
        if (shadowEffect != null)
            Instantiate(shadowEffect, transform.position, Quaternion.identity);
    }
}
