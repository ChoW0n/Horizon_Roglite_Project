using Cinemachine;
using HRP.AnimatorCoder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AnimatorCoder, IDamageable
{
    #region 레퍼런스
    [SerializeField] private ScreenShakeSO profile;
    private CinemachineImpulseSource impulseSource;
    public Rigidbody2D _rb { get; private set; }

    [SerializeField] private float maxHealth = 3f;
    public float currentHealth { get; set; }

    #region 움직임 State
    [SerializeField] private float patrolRange = 3f;      // 이동 반경
    [SerializeField] private float moveSpeed = 2f;        // 이동 속도
    [SerializeField] private float idleDuration = 1.5f;   // 방향 바꿀 때 잠깐 쉬는 시간

    public Vector2 startPosition { get; private set; }
    public bool IsMovingRight { get; set; } = true;
    public float PatrolRange => patrolRange;
    public float MoveSpeed => moveSpeed;
    public float IdleDuration => idleDuration;

    #endregion

    #region 어그로 State
    public float strikingDistance = 5f;
    [SerializeField] private Transform playerTransform; 
    [SerializeField] private LayerMask playerLayer;

    public Transform PlayerTransform => playerTransform;
    public float StrikingDistance => strikingDistance;

    #endregion

    #endregion

    #region 초기화
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

    #region 움직임
    public void Flip()
    {
        // 이동 방향에 따라 Y축 회전을 0도 또는 180도로 설정
        float yRotation = IsMovingRight ? 0f : 180f;
        transform.localEulerAngles = new Vector3(0f, yRotation, 0f);
    }

    #endregion

    #region 플레이어 감지
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

    #region 상태 관련
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

    #region 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, strikingDistance);
    }

    #endregion
}
