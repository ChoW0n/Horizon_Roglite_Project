using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerState ��� �÷��̾� ������ �⺻�� �Ǵ� �߻� Ŭ����
public abstract class EnemyState
{
    protected EnemyStateMachine stateMachine;     
    protected Enemy enemy;     

    // ������ : ���� �ӽŰ� �÷��̾� ��Ʈ�ѷ� ���� �ʱ�ȭ
    public EnemyState(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.enemy = stateMachine._enemy;
    }

    // ���� �޼���� : ���� Ŭ�������� �ʿ信 ���� �������̵�
    public virtual void Enter() { }             // ���� ���� �� ȣ��
    public virtual void Exit() { }              // ���� ���� �� ȣ��  
    public virtual void Update()                // �� ������ ȣ��
    {
        CheckTransitions();
    }            
    public virtual void FixedUpdate() { }       // ���� �ð� �������� ȣ�� (���� �����)

    // ���� ��ȣ�� ������ üũ�ϴ� �޼���
    protected virtual void CheckTransitions() 
    {

    }
}