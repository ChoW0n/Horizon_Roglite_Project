using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public int currentState;

    public enum PlayerAnimationState
    {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Dash = 3,
        Jump = 4
    }

    public void ChangeAnimationState(PlayerAnimationState newState)
    {
        int stateValue = (int)newState;
        if (stateValue == currentState) return;

        currentState = stateValue;
        animator.SetInteger("State", currentState);
    }
}
