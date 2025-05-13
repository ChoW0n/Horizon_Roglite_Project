using UnityEngine;

public class EnemyAnimationManager : MonoBehaviour
{
    public Animator animator;                        
    public EnemyStateMachine stateMachine;          

    // 애니메이션 파라미터 이름들을 상수로 정의
    private const string PARAM_IS_MOVING = "IsMoving";

    void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        // 현재 상태에 따라 애니메이션 파라미터 설정
        if (stateMachine.currentState != null)
        {
            // 모든 bool 파라미터를 초기화
            ResetAllBoolParameters();

            // 현재 상태에 따라 해당하는 애니메이션 파라미터를 설정
            switch (stateMachine.currentState)
            {
                case EnemyIdleState:
                    break;
                case EnemyMoveState:
                    animator.SetBool(PARAM_IS_MOVING, true);
                    break;
            }
        }
    }

    // 모든 bool 파라미터를 초기화 해주는 함수
    private void ResetAllBoolParameters()
    {
        animator.SetBool(PARAM_IS_MOVING, false);
    }
}