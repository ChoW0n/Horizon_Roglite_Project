using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerState 모든 플레이어 상태의 기본이 되는 추상 클래스
public abstract class PlayerState
{
    protected PlayerStateMachine stateMachine;         
    protected PlayerController playerController;   
    protected PlayerSO playerSO;
    protected PlayerJump playerJump;
    protected PlayerAnimationManager animationManager; 

    // 생성자 : 상태 머신과 플레이어 컨트롤러 참조 초기화
    public PlayerState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.playerController = stateMachine.PlayerController;
        this.playerSO = stateMachine.PlayerSO;
        this.playerJump = stateMachine.PlayerJump;
        this.animationManager = stateMachine.GetComponent<PlayerAnimationManager>();
    }

    // 가상 메서드들 : 하위 클래스에서 필요에 따라 오버라이드
    public virtual void Enter() { }             // 상태 진입 시 호출
    public virtual void Exit() { }              // 상태 종료 시 호출  
    public virtual void Update() { }            // 매 프레임 호출
    public virtual void FixedUpdate() { }       // 고정 시간 간격으로 호출 (물리 연산용)

    // 상태 전호나 조건을 체크하는 메서드
    protected void CheckTransitions()
    {
        if (playerController._isGrounded)
        {
            // 지상에 있을 때의 상태 전환 로직
            if (InputManager.JumpWasPressed)        // 스페이스를 눌렀을때
            {
                stateMachine.TransitionToState(new LongJumpingState(stateMachine));
            }
            else if (Mathf.Abs(InputManager.Movement.x) >= playerController.PlayerSO.MoveThreshold) // 이동키가 눌렸을 때
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
            // 공중에 있을 때 상태 전환 로직
            if (playerJump.VerticalVelocity > 0)
            {
                if (playerJump._isFastFalling)
                    stateMachine.TransitionToState(new ShortJumpingState(stateMachine));
                else
                    stateMachine.TransitionToState(new LongJumpingState(stateMachine));
            }
            else if (playerJump._isJumping && playerJump.VerticalVelocity <= 0f)  // 낙하 전환 조건
            {
                // stateMachine.TransitionToState(new FallingState(stateMachine));
            }
        }
    }
}

// IdleState : 플레이어가 정지해 있는 상태
public class IdleState : PlayerState
{
    public IdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Update()
    {
        CheckTransitions();                 // 매 프레임마다 상태 전환 조건 체크
    }
}

// MoveState : 플레이어가 움직이고 있는 상태
public class MovingState : PlayerState
{
    private bool isRunning;
    public MovingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Update()
    {
        // 달리기 입력 확인
        isRunning = InputManager.RunIsHeld;

        CheckTransitions();                 // 매 프레임마다 상태 전환 조건 체크
    }

    public override void FixedUpdate()
    {
        playerController.Move(playerSO.GroundAcceleration, playerSO.GroundDeceleration, InputManager.Movement);  // 물리 기반 이동 처리
    }
}

// JumpingState : 플레이어가 점프하는 상태
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

// FallingState : 플레이어가 낙하 중일때
/*public class FallingState : PlayerState
{
    public FallingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Update()
    {
        CheckTransitions();                 // 매 프레임마다 상태 전환 조건 체크
    }

    public override void FixedUpdate()
    {
        playerController.HandleMovement();  // 물리 기반 이동 처리
    }
}*/