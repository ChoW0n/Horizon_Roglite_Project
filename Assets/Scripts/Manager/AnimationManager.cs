using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public Animator animator;

    public int currentState;

    public enum PlayerAnimationState
    {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Dash = 3,
        StartJump = 4,
        Fall = 5,
        EndJump = 6,
        Jump = 7,
        Land = 8,
        Attack = 9
    }

    public void ChangeAnimationState(PlayerAnimationState newState)
    {
        int stateValue = (int)newState;
        if (stateValue == currentState) return;

        currentState = stateValue;
        animator.SetInteger("State", currentState);
    }
}
