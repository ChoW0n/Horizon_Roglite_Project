using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerState ��� �÷��̾� ������ �⺻�� �Ǵ� �߻� Ŭ����
public abstract class PlayerState
{
    protected PlayerStateMachine stateMachine;         
    protected PlayerController playerController;   
    protected PlayerSO playerSO;
    protected PlayerJump playerJump;
    protected PlayerAnimationManager animationManager; 

    // ������ : ���� �ӽŰ� �÷��̾� ��Ʈ�ѷ� ���� �ʱ�ȭ
    public PlayerState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.playerController = stateMachine.PlayerController;
        this.playerSO = stateMachine.PlayerSO;
        this.playerJump = stateMachine.PlayerJump;
        this.animationManager = stateMachine.GetComponent<PlayerAnimationManager>();
    }

    // ���� �޼���� : ���� Ŭ�������� �ʿ信 ���� �������̵�
    public virtual void Enter() { }             // ���� ���� �� ȣ��
    public virtual void Exit() { }              // ���� ���� �� ȣ��  
    public virtual void Update() { }            // �� ������ ȣ��
    public virtual void FixedUpdate() { }       // ���� �ð� �������� ȣ�� (���� �����)

    // ���� ��ȣ�� ������ üũ�ϴ� �޼���
    protected void CheckTransitions()
    {
        if (playerController._isGrounded)
        {
            // ���� ���� ���� ���� ��ȯ ����
            if (InputManager.JumpWasPressed)        // �����̽��� ��������
            {
                stateMachine.TransitionToState(new LongJumpingState(stateMachine));
            }
            else if (Mathf.Abs(InputManager.Movement.x) >= playerController.PlayerSO.MoveThreshold) // �̵�Ű�� ������ ��
            {
                stateMachine.TransitionToState(new MovingState(stateMachine));
            }
            else
            {
                stateMachine.TransitionToState(new IdleState(stateMachine));
            }
        }
        else
        {
            // ���߿� ���� �� ���� ��ȯ ����
            if (playerJump.VerticalVelocity > 0)
            {
                if (playerJump._isFastFalling)
                    stateMachine.TransitionToState(new ShortJumpingState(stateMachine));
                else
                    stateMachine.TransitionToState(new LongJumpingState(stateMachine));
            }
            else if (playerJump._isJumping && playerJump.VerticalVelocity <= 0f)  // ���� ��ȯ ����
            {
                // stateMachine.TransitionToState(new FallingState(stateMachine));
            }
        }
    }
}

// IdleState : �÷��̾ ������ �ִ� ����
public class IdleState : PlayerState
{
    public IdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Update()
    {
        CheckTransitions();                 // �� �����Ӹ��� ���� ��ȯ ���� üũ
    }
}

// MoveState : �÷��̾ �����̰� �ִ� ����
public class MovingState : PlayerState
{
    private bool isRunning;
    public MovingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Update()
    {
        // �޸��� �Է� Ȯ��
        isRunning = InputManager.RunIsHeld;

        CheckTransitions();                 // �� �����Ӹ��� ���� ��ȯ ���� üũ
    }

    public override void FixedUpdate()
    {
        playerController.Move(playerSO.GroundAcceleration, playerSO.GroundDeceleration, InputManager.Movement);  // ���� ��� �̵� ó��
    }
}

// JumpingState : �÷��̾ �����ϴ� ����
public class LongJumpingState : PlayerState
{
    public LongJumpingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Update()
    {
        playerJump.JumpChecks();
        CheckTransitions();
    }

    public override void FixedUpdate()
    {
        playerController.Move(playerSO.GroundAcceleration, playerSO.GroundDeceleration, InputManager.Movement);
    }
}

public class ShortJumpingState : PlayerState
{
    public ShortJumpingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Update()
    {
        playerJump.JumpChecks();
        CheckTransitions();
    }

    public override void FixedUpdate()
    {
        playerController.Move(playerSO.GroundAcceleration, playerSO.GroundDeceleration, InputManager.Movement);
    }
}

// FallingState : �÷��̾ ���� ���϶�
/*public class FallingState : PlayerState
{
    public FallingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Update()
    {
        CheckTransitions();                 // �� �����Ӹ��� ���� ��ȯ ���� üũ
    }

    public override void FixedUpdate()
    {
        playerController.HandleMovement();  // ���� ��� �̵� ó��
    }
}*/