using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Update()
    {
        base.Update(); // 전이 체크 포함됨
    }

    public override void FixedUpdate()
    {
        if (enemy.PlayerTransform == null)
            return;

        Vector2 enemyPos = enemy.transform.position;
        Vector2 playerPos = enemy.PlayerTransform.position;
        float distance = Vector2.Distance(enemyPos, playerPos);

        // 추적 거리 이내에 있는 경우만 추적
        if (distance <= enemy.StrikingDistance)
        {
            // 이동 방향 결정
            float direction = playerPos.x > enemyPos.x ? 1f : -1f;
            enemy.IsMovingRight = direction > 0f;

            // 속도 적용
            Vector2 velocity = new Vector2(direction * enemy.MoveSpeed, enemy._rb.velocity.y);
            enemy._rb.velocity = velocity;

            animationManager.animator.SetBool("IsFlying", true);

            // 방향 반영
            enemy.Flip();
        }
        else
        {
            // 추적 대상이 사라졌거나 범위를 벗어나면 Idle 또는 Patrol 상태로 복귀
            stateMachine.TransitionToState(new EnemyIdleState(stateMachine));
            animationManager.animator.SetBool("IsFlying", false);
            animationManager.animator.SetBool("IsStriking", false);

        }
    }
}
