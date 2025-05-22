using HRP.AnimatorCoder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    /// <summary>
    /// 플레이어의 공격 방향을 나타내는 enum (기본/위/아래)
    /// </summary>
    public enum AttackDirection
    {
        Normal,
        Up,
        Down
    }

    #region 레퍼런스
    private PlayerController Controller;

    [SerializeField] private Transform normalAttackTransform;
    [SerializeField] private Transform upAttackTransform;
    [SerializeField] private Transform downAttackTransform;

    [SerializeField] private Transform normalAttackHitboxTransform;
    [SerializeField] private Transform upAttackHitboxTransform;
    [SerializeField] private Transform downAttackHitboxTransform;

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float attackRange = 1.5f;

    private Dictionary<AttackDirection, Transform> attackTransformMap;
    private Dictionary<AttackDirection, Transform> hitboxTransformMap;

    // 패링 시스템 들어가면 필요한 Bool값
    public bool ShouldBeDamageing { get; private set; } = false;

    private RaycastHit2D[] _hits;

    private float _attackTimeCounter;

    #endregion

    #region 초기화
    void Awake()
    {
        attackTransformMap = new()
        {
            { AttackDirection.Normal, normalAttackTransform },
            { AttackDirection.Up, upAttackTransform },
            { AttackDirection.Down, downAttackTransform }
        };

        hitboxTransformMap = new()
    {
        { AttackDirection.Normal, normalAttackHitboxTransform },
        { AttackDirection.Up, upAttackHitboxTransform },
        { AttackDirection.Down, downAttackHitboxTransform }
    };
    }

    void Start()
    {
        Controller = GetComponent<PlayerController>();

        _attackTimeCounter = Controller.PlayerSO.timeBtwAttacks;
    }

    void Update()
    {
        if (InputManager.AttackWasPressed && _attackTimeCounter >= Controller.PlayerSO.timeBtwAttacks)
        {
            _attackTimeCounter = 0f;

            AttackDirection direction = AttackDirection.Normal;

            if (InputManager.LookUpIsHeld)
            {
                direction = AttackDirection.Up;
                Controller.Play(new(Animations.UP_ATTACK, true, new(Animations.IDLE)));
            }
            else if (InputManager.LookDownIsHeld)
            {
                if (Controller._isGrounded)
                    return;

                direction = AttackDirection.Down;
                Controller.Play(new(Animations.DOWN_ATTACK, true, new(Animations.IDLE)));
            }

            Controller.Play(new(Animations.ATTACK, true, new(Animations.IDLE)));

            string effectName = direction switch
            {
                AttackDirection.Up => "UpAttack",
                AttackDirection.Down => "DownAttack",
                AttackDirection.Normal => Controller._isFacingRight ? "RightAttack" : "LeftAttack"
            };

            EffectManager.instance.PlayEffect(effectName, attackTransformMap[direction].position, Quaternion.identity);

            // 실제 공격
            Attack(direction);
        }

        _attackTimeCounter += Time.deltaTime;
    }

    #endregion

    #region 공격
    public void Attack(AttackDirection direction)
    {
        Transform atkTf = hitboxTransformMap[direction]; // 또는 배열 접근
        _hits = Physics2D.CircleCastAll(atkTf.position, attackRange, Vector2.zero, 0f, enemyLayer);

        foreach (var hit in _hits)
        {
            IDamageable dmg = hit.collider.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.Damage(Controller.PlayerSO.damageAmount);
            }
        }
    }

    #endregion

    #region 기즈모
    private void OnDrawGizmosSelected()
    {
        if (normalAttackHitboxTransform != null)
            Gizmos.DrawWireSphere(normalAttackHitboxTransform.position, attackRange);

        if (upAttackHitboxTransform != null)
            Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(upAttackHitboxTransform.position, attackRange);

        if (downAttackHitboxTransform != null)
            Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(downAttackHitboxTransform.position, attackRange);
    }

    #endregion

    #region 애니메이션 트리거
    public void ShouldBeDamagingToTrue()
    {
        ShouldBeDamageing = true;
    }

    public void ShouldBeDamagingToFalse()
    {
        ShouldBeDamageing = false;
    }

    #endregion
}
