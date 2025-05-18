using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Update()
    {
        base.Update(); // ���� üũ ���Ե�
    }

    public override void FixedUpdate()
    {
        if (enemy.PlayerTransform == null)
            return;

        Vector2 enemyPos = enemy.transform.position;
        Vector2 playerPos = enemy.PlayerTransform.position;
        float distance = Vector2.Distance(enemyPos, playerPos);

        // ���� �Ÿ� �̳��� �ִ� ��츸 ����
        if (distance <= enemy.StrikingDistance)
        {
            // �̵� ���� ����
            float direction = playerPos.x > enemyPos.x ? 1f : -1f;
            enemy.IsMovingRight = direction > 0f;

            // �ӵ� ����
            Vector2 velocity = new Vector2(direction * enemy.MoveSpeed, enemy._rb.velocity.y);
            enemy._rb.velocity = velocity;

            animationManager.animator.SetBool("IsFlying", true);

            // ���� �ݿ�
            enemy.Flip();
        }
        else
        {
            // ���� ����� ������ų� ������ ����� Idle �Ǵ� Patrol ���·� ����
            stateMachine.TransitionToState(new EnemyIdleState(stateMachine));
            animationManager.animator.SetBool("IsFlying", false);
            animationManager.animator.SetBool("IsStriking", false);

        }
    }
}
