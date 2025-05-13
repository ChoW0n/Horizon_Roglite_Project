using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerState 모든 플레이어 상태의 기본이 되는 추상 클래스
public abstract class EnemyState
{
    protected EnemyStateMachine stateMachine;     
    protected Enemy enemy;     
    protected EnemyAnimationManager animationManager;  

    // 생성자 : 상태 머신과 플레이어 컨트롤러 참조 초기화
    public EnemyState(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.enemy = stateMachine._enemy;
        this.animationManager = stateMachine.GetComponent<EnemyAnimationManager>();
    }

    // 가상 메서드들 : 하위 클래스에서 필요에 따라 오버라이드
    public virtual void Enter() { }             // 상태 진입 시 호출
    public virtual void Exit() { }              // 상태 종료 시 호출  
    public virtual void Update()                // 매 프레임 호출
    {
        CheckTransitions();
    }            
    public virtual void FixedUpdate() { }       // 고정 시간 간격으로 호출 (물리 연산용)

    // 상태 전호나 조건을 체크하는 메서드
    protected void CheckTransitions()
    {
        if (enemy.IsPlayerInStrikingDistance())
        {
            stateMachine.TransitionToState(new EnemyChaseState(stateMachine));
        }
    }
}