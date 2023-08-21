using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class UnitAnimationManager : NetworkBehaviour
{

    [SerializeField] Animator animator;
    private int currentState;

    #region Animation States
    int UNIT_WALK = Animator.StringToHash("Walk");
    int UNIT_ATTACK = Animator.StringToHash("Attack");
    int UNIT_IDLE = Animator.StringToHash("Idle");

    [SerializeField] float animationLength;
    #endregion

    public void WalkingAnim(float transition)
    {
        if (animator == null) return;
        
        ChangeAnimationState(UNIT_WALK, transition);
    }

    public void AttackingAnim(float transition)
    {
        if (animator == null) return;

        ChangeAnimationState(UNIT_ATTACK, transition);
    }

    public void IdleAnim(float transition)
    {
        if (animator == null) return;

        ChangeAnimationState(UNIT_IDLE, transition);
    }

    public float GetAttackAnimLength()
    {
        return animationLength;
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
