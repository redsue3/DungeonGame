using System.Collections;
using UnityEngine;

// Animator Parameter 이름 상수
public static class AnimParam
{
    public const string Idle    = "Idle";
    public const string Move    = "Move";
    public const string Attack  = "Attack";
    public const string Special = "Special";
}

[RequireComponent(typeof(Animator))]
public class CharacterAnimator : MonoBehaviour
{
    private Animator anim;
    private bool isPlaying = false; // 공격/특수 재생 중 중복 방지

    void Awake() => anim = GetComponent<Animator>();

    // ─── 상태 전환 메서드 ───

    public void PlayIdle()
    {
        if (isPlaying) return;
        anim.SetTrigger(AnimParam.Idle);
    }

    public void PlayMove(bool isMoving)
    {
        if (isPlaying) return;
        anim.SetBool(AnimParam.Move, isMoving);
    }

    // 공격: 재생 후 자동으로 Idle 복귀
    public void PlayAttack()
    {
        if (isPlaying) return;
        StartCoroutine(PlayAndReturn(AnimParam.Attack));
    }

    // 특수: 재생 후 자동으로 Idle 복귀
    public void PlaySpecial()
    {
        if (isPlaying) return;
        StartCoroutine(PlayAndReturn(AnimParam.Special));
    }

    private IEnumerator PlayAndReturn(string triggerName)
    {
        isPlaying = true;
        anim.SetTrigger(triggerName);

        // 현재 애니메이션 끝날 때까지 대기
        yield return null; // 1프레임 대기 후 애니메이션 상태 감지
        yield return new WaitUntil(() =>
        {
            var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(triggerName) && stateInfo.normalizedTime >= 1f;
        });

        isPlaying = false;
        anim.SetTrigger(AnimParam.Idle); // Idle로 복귀
    }
}
