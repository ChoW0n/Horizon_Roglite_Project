using Cinemachine;
using HRP.AnimatorCoder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AnimatorCoder, IDamageable
{
    #region ���۷���
    [SerializeField] private ScreenShakeSO profile;
    private CinemachineImpulseSource impulseSource;
    public Rigidbody2D _rb { get; private set; }

    [SerializeField] private float maxHealth = 3f;
    public float currentHealth { get; set; }

    #region ������ State
    [SerializeField] private float patrolRange = 3f;      // �̵� �ݰ�
    [SerializeField] private float moveSpeed = 2f;        // �̵� �ӵ�
    [SerializeField] private float idleDuration = 1.5f;   // ���� �ٲ� �� ��� ���� �ð�

    public Vector2 startPosition { get; private set; }
    public bool IsMovingRight { get; set; } = true;
    public float PatrolRange => patrolRange;
    public float MoveSpeed => moveSpeed;
    public float IdleDuration => idleDuration;

    #endregion

    #region ��׷� State
    public float strikingDistance = 5f;
    [SerializeField] private Transform playerTransform; 
    [SerializeField] private LayerMask playerLayer;

    public Transform PlayerTransform => playerTransform;
    public float StrikingDistance => strikingDistance;

    #endregion

    #endregion

    #region �ʱ�ȭ
    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;

        impulseSource = GetComponent<CinemachineImpulseSource>();
        _rb = GetComponent<Rigidbody2D>();

        Initialize();
    }

    public override void DefaultAnimation(int layer) { }

    #endregion

    #region ������
    public void Flip()
    {
        // �̵� ���⿡ ���� Y�� ȸ���� 0�� �Ǵ� 180���� ����
        float yRotation = IsMovingRight ? 0f : 180f;
        transform.localEulerAngles = new Vector3(0f, yRotation, 0f);
    }

    #endregion

    #region �÷��̾� ����
    public bool IsPlayerInStrikingDistance()
    {
        var hit = Physics2D.OverlapCircle(transform.position, strikingDistance, playerLayer);
        playerTransform = hit?.transform;
        return hit != null;
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    #endregion

    #region ���� ����
    public void Damage(float damageAmout)
    {
        CameraShakeManager.instance.ScreenShakeFromProfile(profile, impulseSource);

        currentHealth -= damageAmout;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    #endregion

    #region �����
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, strikingDistance);
    }

    #endregion
}
