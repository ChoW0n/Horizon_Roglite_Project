using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    #region ���۷���
    private PlayerController Controller;

    [SerializeField] private Transform attackTranform;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float attackRange = 1.5f;

    // �и� �ý��� ���� �ʿ��� Bool��
    public bool ShouldBeDamageing { get; private set; } = false;

    private RaycastHit2D[] _hits;

    private float _attackTimeCounter;

    #endregion

    #region �ʱ�ȭ
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

            Controller.AnimManager.ChangeAnimationState(AnimationManager.PlayerAnimationState.Attack);
            EffectManager.instance.PlayEffect("Attack", attackTranform.position, Quaternion.identity);
        }

        _attackTimeCounter += Time.deltaTime;
    }

    #endregion

    #region ����
    public void Attack()
    {
        _hits = Physics2D.CircleCastAll(attackTranform.position, attackRange, Vector2.zero, 0f, enemyLayer);

        foreach (var hit in _hits)
        {
            IDamageable idamageable = hit.collider.GetComponent<IDamageable>();
            if (idamageable != null)
            {
                idamageable.Damage(Controller.PlayerSO.damageAmount);
            }
        }
    }

    #endregion

    #region �����
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackTranform.position, attackRange);
    }

    #endregion

    #region �ִϸ��̼� Ʈ����
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
