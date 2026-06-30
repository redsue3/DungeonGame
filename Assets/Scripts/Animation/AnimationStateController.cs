using UnityEngine;

// BattleManager에서 캐릭터 애니메이션 호출하는 브릿지
// 사용: AnimationStateController.Play(character, AnimAction.Attack)
public enum AnimAction { Idle, Move, StopMove, Attack, Special }

public static class AnimationStateController
{
    public static void Play(CharacterAnimator animator, AnimAction action)
    {
        if (animator == null) return;

        switch (action)
        {
            case AnimAction.Idle:     animator.PlayIdle();          break;
            case AnimAction.Move:     animator.PlayMove(true);      break;
            case AnimAction.StopMove: animator.PlayMove(false);     break;
            case AnimAction.Attack:   animator.PlayAttack();        break;
            case AnimAction.Special:  animator.PlaySpecial();       break;
        }
    }
}
