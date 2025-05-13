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
        // �ʱ� ���¸� IdleState �� ����
        TransitionToState(new EnemyIdleState(this));
    }

    void Update()
    {
        // ���� ���װ� �����Ѵٸ� ���� ������ Update �޼��� ȣ��
        if (currentState != null)
        {
            currentState.Update();
        }
    }

    private void FixedUpdate()
    {
        // ���� ���װ� �����Ѵٸ� ���� ������ FixedUpdate �޼��� ȣ��
        if (currentState != null)
        {
            currentState.FixedUpdate();
        }
    }

    // ���ο� ���·� ��ȯ �ϴ� �޼���
    public void TransitionToState(EnemyState newState)
    {
        // ���� ���¿� ���ο� ���°� ���� Ÿ�� �� ���
        if (currentState?.GetType() == newState.GetType()) return;  // ���� Ÿ���̸� ���¸� ��ȯ ���� �ʰ� ����

        // ���� ���°� ���� �Ѵٸ� Exit �޼��带 ȣ��
        currentState?.Exit();       // �˻��ؼ� ȣ�� ���� (?)�� IF ����

        // ���ο� ���·� ��ȯ
        currentState = newState;

        // ���ο� ������ Enter �޼��带 ȣ��(���� ����)
        currentState.Enter();

        // �α׿� ���� ��ȯ ������ ���
        Debug.Log($"���� ��ȯ �Ǵ� ������Ʈ {newState.GetType().Name}");
    }
}