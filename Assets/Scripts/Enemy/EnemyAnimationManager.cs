using UnityEngine;

public class EnemyAnimationManager : MonoBehaviour
{
    public Animator animator;                        
    public EnemyStateMachine stateMachine;          

    // �ִϸ��̼� �Ķ���� �̸����� ����� ����
    private const string PARAM_IS_MOVING = "IsMoving";
    private const string PARAM_IS_STRIKING = "IsStriking";

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
                case EnemyIdleState:
                    break;
                case EnemyMoveState:
                    animator.SetBool(PARAM_IS_MOVING, true);
                    break;
                case EnemyChaseState:
                    animator.SetBool(PARAM_IS_STRIKING, true);
                    break;
            }
        }
    }

    // ��� bool �Ķ���͸� �ʱ�ȭ ���ִ� �Լ�
    private void ResetAllBoolParameters()
    {
        animator.SetBool(PARAM_IS_MOVING, false);
        animator.SetBool(PARAM_IS_STRIKING, false);
    }
}