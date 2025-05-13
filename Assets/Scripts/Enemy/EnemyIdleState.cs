using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private float idleTimer;

    public EnemyIdleState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        idleTimer = 0f;
    }

    public override void Update()
    {
        base.Update();

        idleTimer += Time.deltaTime;
        if (idleTimer >= enemy.IdleDuration)
        {
            stateMachine.TransitionToState(new EnemyMoveState(stateMachine));
        }
    }
}