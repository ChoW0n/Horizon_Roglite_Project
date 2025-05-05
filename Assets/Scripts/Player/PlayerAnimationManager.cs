using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    public Animator animator;                           // 애니메이션을 관리하는 오브젝트
    public PlayerStateMachine stateMachine;             // 사용자가 정리한 상태 정의

    // 애니메이션 파라미터 이름들을 상수로 정의
    private const string PARAM_IS_MOVING = "IsWalking";
    private const string PARAM_IS_RUNNING = "IsRunning";
    private const string PARAM_IS_LONGJUMPING = "IsLongJumping";
    private const string PARAM_IS_SHORTJUMPING = "IsShortJumping";
    //private const string PARAM_IS_FALLING = "IsFalling";

    void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        // 현재 상태에 따라 애니메이션 파라미터 설정
        if (stateMachine.currentState != null)
        {
            // 모든 bool 파라미터를 초기화
            ResetAllBoolParameters();

            // 현재 상태에 따라 해당하는 애니메이션 파라미터를 설정
            switch (stateMachine.currentState)
            {
                case IdleState:
                    // Idle 상태 는 모든 파라미터가 false인 상태
                    break;
                case MovingState:
                    animator.SetBool(PARAM_IS_MOVING, true);
                    // 달리기 입력 확인
                    if (InputManager.RunIsHeld)
                    {
                        animator.SetBool(PARAM_IS_RUNNING, true);
                    }
                    break;
                case LongJumpingState:
                    animator.SetBool(PARAM_IS_LONGJUMPING, true);
                    break;
                case ShortJumpingState:
                    animator.SetBool(PARAM_IS_SHORTJUMPING, true);
                    break;
                /*case FallingState:
                    animator.SetBool(PARAM_IS_FALLING, true);
                    break;*/
            }
        }
    }

    // 공격 애니메이션 트리거
    public void TriggerAttack() { }

    // 모든 bool 파라미터를 초기화 해주는 함수
    private void ResetAllBoolParameters()
    {
        animator.SetBool(PARAM_IS_MOVING, false);
        animator.SetBool(PARAM_IS_RUNNING, false);
        animator.SetBool(PARAM_IS_LONGJUMPING, false);
        animator.SetBool(PARAM_IS_SHORTJUMPING, false);
        //animator.SetBool(PARAM_IS_FALLING, false);
    }
}