using UnityEngine;

// 마법사 전용 애니메이터
public class MageAnimator : CharacterAnimator
{
    [Header("마법사 이펙트")]
    public GameObject spellOrb;        // 화염구 발사체
    public GameObject lightningBolt;   // 번개 이펙트
    public GameObject manaAura;        // 특수기 마나 오라

    public void OnSpellCast()
    {
        if (spellOrb != null)
            Instantiate(spellOrb, transform.position + Vector3.right, Quaternion.identity);
    }

    public void OnLightningStrike()
    {
        if (lightningBolt != null)
            Instantiate(lightningBolt, transform.position, Quaternion.identity);
    }

    public void OnSpecialChannel()
    {
        if (manaAura != null)
            Instantiate(manaAura, transform.position, Quaternion.identity);
    }
}
