using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class EnemyAnimationManager : NetworkBehaviour
{

    [SerializeField] Animator animator;
    private int currentState;

    #region Animation States
    int UNIT_WALK = Animator.StringToHash("Walk");
    int UNIT_ATTACK = Animator.StringToHash("Attack");
    int UNIT_IDLE = Animator.StringToHash("Idle");



    #endregion

    public void WalkingAnim(float transition)
    {
        ChangeAnimationState(UNIT_WALK, transition);
    }

    public void AttackingAnim(float transition)
    {
        ChangeAnimationState(UNIT_ATTACK, transition);
    }

    public void ChangeAnimationState(int newState)
    {
        // stops the same animation from interrupting itself
        if (currentState == newState) return;

        // plays the animation
        AnimatorPlay(newState);

        //reassigns the current state
        currentState = newState;
    }

    // TRANSITION METHOD
    public void ChangeAnimationState(int newState, float transitionTime)
    {
        // stops the same animation from interrupting itself
        if (currentState == newState) return;

        // plays the animation / transitionTime determines how long the transition will take
        AnimatorPlay(newState, transitionTime);

        //reassigns the current state
        currentState = newState;
    }

    [ClientRpc]
    void AnimatorPlay(int newState)
    {
        animator.Play(newState);
    }

    [ClientRpc]
    void AnimatorPlay(int newState, float transitionTime)
    {
        animator.CrossFadeInFixedTime(newState, transitionTime);
    }
}
