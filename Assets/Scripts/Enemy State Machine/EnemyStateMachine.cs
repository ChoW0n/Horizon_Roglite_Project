using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public EnemyState currentState;
    public Enemy _enemy;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>(); 
    }

    void Start()
    {
        // 초기 상태를 IdleState 로 설정
        TransitionToState(new EnemyIdleState(this));
    }

    void Update()
    {
        // 현재 상테가 존재한다면 현재 상태의 Update 메서드 호출
        if (currentState != null)
        {
            currentState.Update();
        }
    }

    private void FixedUpdate()
    {
        // 현재 상테가 존재한다면 현재 상태의 FixedUpdate 메서드 호출
        if (currentState != null)
        {
            currentState.FixedUpdate();
        }
    }

    // 새로운 상태로 전환 하는 메서드
    public void TransitionToState(EnemyState newState)
    {
        // 현재 상태와 새로운 상태가 같은 타입 일 경우
        if (currentState?.GetType() == newState.GetType()) return;  // 같은 타입이면 상태를 전환 하지 않고 리턴

        // 현제 상태가 존재 한다면 Exit 메서드를 호출
        currentState?.Exit();       // 검사해서 호출 종료 (?)는 IF 조건

        // 새로운 상태로 전환
        currentState = newState;

        // 새로운 상태의 Enter 메서드를 호출(상태 시작)
        currentState.Enter();

        // 로그에 상태 전환 정보를 출력
        Debug.Log($"상태 전환 되는 스테이트 {newState.GetType().Name}");
    }
}