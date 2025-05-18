using HRP.AnimatorCoder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveState : EnemyState
{
    private float leftLimit;
    private float rightLimit;

    public EnemyMoveState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        float range = enemy.PatrolRange;
        Vector2 start = enemy.startPosition;
        leftLimit = start.x - range;
        rightLimit = start.x + range;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.Play(new (Animations.ENEMY_WALK));
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        float direction = enemy.IsMovingRight ? 1f : -1f;
        Vector2 velocity = new Vector2(direction * enemy.MoveSpeed, enemy._rb.velocity.y);
        enemy._rb.velocity = velocity;

        // 방향에 따라 회전 반영
        enemy.Flip();

        if (enemy.IsMovingRight && enemy.transform.position.x >= rightLimit)
        {
            enemy.IsMovingRight = false;
            stateMachine.TransitionToState(new EnemyIdleState(stateMachine));
        }
        else if (!enemy.IsMovingRight && enemy.transform.position.x <= leftLimit)
        {
            enemy.IsMovingRight = true;
            stateMachine.TransitionToState(new EnemyIdleState(stateMachine));
        }
    }

    public override void Exit()
    {
        enemy._rb.velocity = Vector2.zero;
    }
}
