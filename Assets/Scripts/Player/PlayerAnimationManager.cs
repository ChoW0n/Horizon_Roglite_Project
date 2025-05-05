using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    public Animator animator;                           // �ִϸ��̼��� �����ϴ� ������Ʈ
    public PlayerStateMachine stateMachine;             // ����ڰ� ������ ���� ����

    // �ִϸ��̼� �Ķ���� �̸����� ����� ����
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
        // ���� ���¿� ���� �ִϸ��̼� �Ķ���� ����
        if (stateMachine.currentState != null)
        {
            // ��� bool �Ķ���͸� �ʱ�ȭ
            ResetAllBoolParameters();

            // ���� ���¿� ���� �ش��ϴ� �ִϸ��̼� �Ķ���͸� ����
            switch (stateMachine.currentState)
            {
                case IdleState:
                    // Idle ���� �� ��� �Ķ���Ͱ� false�� ����
                    break;
                case MovingState:
                    animator.SetBool(PARAM_IS_MOVING, true);
                    // �޸��� �Է� Ȯ��
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

    // ���� �ִϸ��̼� Ʈ����
    public void TriggerAttack() { }

    // ��� bool �Ķ���͸� �ʱ�ȭ ���ִ� �Լ�
    private void ResetAllBoolParameters()
    {
        animator.SetBool(PARAM_IS_MOVING, false);
        animator.SetBool(PARAM_IS_RUNNING, false);
        animator.SetBool(PARAM_IS_LONGJUMPING, false);
        animator.SetBool(PARAM_IS_SHORTJUMPING, false);
        //animator.SetBool(PARAM_IS_FALLING, false);
    }
}