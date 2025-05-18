using HRP.AnimatorCoder;
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    private float waitTime;
    private float elapsedTime;

    public EnemyIdleState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        enemy.Play(new(Animations.ENEMY_IDLE));
        waitTime = Random.Range(1f, 2.5f); // ��� �ð� ����
        elapsedTime = 0f;
    }

    public override void Update()
    {
        base.Update();
        elapsedTime += Time.deltaTime;

        // �÷��̾� ���� üũ �߰�
        if (enemy.PlayerTransform != null)
        {
            float distance = Vector2.Distance(enemy.transform.position, enemy.PlayerTransform.position);
            if (distance <= enemy.strikingDistance)
            {
                stateMachine.TransitionToState(new EnemyChaseState(stateMachine));
                return;
            }
        }

        if (elapsedTime >= waitTime)
        {
            stateMachine.TransitionToState(new EnemyMoveState(stateMachine));
        }
    }

    protected override void CheckTransitions()
    {
        base.CheckTransitions();
    }
}