using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("References")]
    private PlayerController Controller;
    private PlayerWallSlide WallSlide;
    private PlayerWallJump WallJump;
    private PlayerJump Jump;

    [Header("Dash Vars")]
    [HideInInspector] public bool _isDashing;
    [HideInInspector] public bool _isAirDashing;
    [HideInInspector] public float _dashTimer;
    [HideInInspector] public float _dashOnGroundTimer;
    [HideInInspector] public bool _isDashFastFalling;
    private int _numberOfDashesUsed;
    private Vector2 _dashDirection;
    private float _dashFastFallTime;
    private float _dashFastFallReleaseSpeed;

    private void Start()
    {
        Controller = GetComponent<PlayerController>();
        WallJump = GetComponent<PlayerWallJump>();
        WallSlide = GetComponent<PlayerWallSlide>();
        Jump = GetComponent<PlayerJump>();
    }

    #region Dash

    // �뽬 �Է� Ȯ�� �� ���ǿ� ���� �뽬 ����
    public void DashCheck()
    {
        // �뽬 Ű�� ������ ��
        if (InputManager.DashWaspPressed)
        {
            // ���󿡼� �뽬 ���� ����
            if (Controller._isGrounded && _dashOnGroundTimer < 0 && !_isDashing)
            {
                InitiateDash(); // �뽬 ����  
            }
            // ���߿��� �뽬 ���� ����
            else if (!Controller._isGrounded && !_isDashing && _numberOfDashesUsed < Controller.PlayerSO.NumberOfDashes)
            {
                _isAirDashing = true;   // ���� �뽬 ���� ����
                InitiateDash();         // �뽬 ����

                // �� ���� ������ ª�� �ð� ���̶�� ���� Ƚ�� �ϳ� ����
                if (WallJump._wallJumpPostBufferTimer > 0f)
                {
                    Jump._numberOfJumpsUsed--;
                    if (Jump._numberOfJumpsUsed < 0f)
                        Jump._numberOfJumpsUsed = 0;
                }
            }
        }
    }

    // �뽬 ���� ��� �� �뽬 ���� �ʱ�ȭ
    public void InitiateDash()
    {
        // ���� �Է� ���� ����
        _dashDirection = InputManager.Movement;

        // ���� ����� �뽬 ����
        Vector2 closestDirection = Vector2.zero;
        float minDistance = Vector2.Distance(_dashDirection, Controller.PlayerSO.DashDirections[0]);

        // ���� ����� ��ȿ �뽬 ���� ã��
        for (int i = 0; i < Controller.PlayerSO.DashDirections.Length; i++)
        {
            // ��Ȯ�� ��ġ�ϴ� ������ ������ �ٷ� ����
            if (_dashDirection == Controller.PlayerSO.DashDirections[i])
            {
                closestDirection = _dashDirection;
                break;
            }

            float distance = Vector2.Distance(_dashDirection, Controller.PlayerSO.DashDirections[i]);

            // �밢�� ������ ��� ���Ⱚ�� ����
            bool isDiagonal = (Mathf.Abs(Controller.PlayerSO.DashDirections[i].x) == 1 && Mathf.Abs(Controller.PlayerSO.DashDirections[i].y) == 1);
            if (isDiagonal)
            {
                distance -= Controller.PlayerSO.DashDiagonallyBias;
            }

            // ���� ����� ���� ����
            else if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = Controller.PlayerSO.DashDirections[i];
            }
        }

        // �ƹ� ���� �Է��� ���� ��� �ٶ󺸴� �������� ����
        if (closestDirection == Vector2.zero)
        {
            if (Controller._isFacingRight)
                closestDirection = Vector2.right;
            else
                closestDirection = Vector2.left;
        }

        // ���� �뽬 ���� ���� �� �뽬 ���� �ʱ�ȭ
        _dashDirection = closestDirection;
        _numberOfDashesUsed++;
        _isDashing = true;
        Controller.AnimManager.ChangeAnimationState(AnimationManager.PlayerAnimationState.Dash);
        EffectManager.instance.PlayEffect("Dash", this.gameObject.transform.position, Quaternion.identity);
        _dashTimer = 0f;
        _dashOnGroundTimer = Controller.PlayerSO.TimeBtwDashesOnGround;

        // ���� �� �� ���� ���� ���� �ʱ�ȭ
        Jump.ResetJumpvalues();
        WallJump.ResetWallJumpValues();
        WallSlide.StopWallSlide();
    }

    // FixedUpdate ��� ȣ��Ǿ� �뽬 �� ���� ó�� ����
    public void Dash()
    {
        // �뽬 ���� ��
        if (_isDashing)
        {
            _dashTimer += Time.fixedDeltaTime;

            // �뽬 �ð� ���� ��
            if (_dashTimer >= Controller.PlayerSO.DashTime)
            {
                // ���󿡼� �뽬�� ���� ��� �뽬 Ƚ�� ����
                if (Controller._isGrounded)
                    ResetDashes();

                _isAirDashing = false;
                _isDashing = false;

                // ������ �� ���� ���°� �ƴϸ� ���� ���� �غ�
                if (!Jump._isJumping && !WallJump._isWallJumping)
                {
                    _dashFastFallTime = 0f;
                    _dashFastFallReleaseSpeed = Jump.VerticalVelocity;

                    if (!Controller._isGrounded)
                        _isDashFastFalling = true;
                }

                return;
            }

            // �뽬 ���⿡ ���� �ӵ� ����
            Controller.HorizontalVelocity = Controller.PlayerSO.DashSpeed * _dashDirection.x;

            if (_dashDirection.y != 0f || _isAirDashing)
                Jump.VerticalVelocity = Controller.PlayerSO.DashSpeed * _dashDirection.y;
        }
        // �뽬 �� ���� ���� ó��
        else if (_isDashFastFalling)
        {
            if (Jump.VerticalVelocity > 0f)
            {
                // ���� ��� ���� ��� �� ���� �ð����� õõ�� ����
                if (_dashFastFallTime < Controller.PlayerSO.DashTimeForUpwardsCancel)
                {
                    Jump.VerticalVelocity = Mathf.Lerp(_dashFastFallReleaseSpeed, 0f, (_dashFastFallTime / Controller.PlayerSO.DashTimeForUpwardsCancel));
                }
                // ���Ŀ��� �߷¿� ���� ����
                else
                {
                    Jump.VerticalVelocity += Controller.PlayerSO.Gravity * Controller.PlayerSO.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }

                _dashFastFallTime += Time.fixedDeltaTime;
            }
            else
            {
                // ���� ���� ��� �߷� ����
                Jump.VerticalVelocity += Controller.PlayerSO.Gravity * Controller.PlayerSO.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
        }
    }

    // �뽬 ���� ���� ���� �ʱ�ȭ
    public void ResetDashValues()
    {
        _isDashFastFalling = false;
        _dashOnGroundTimer = -0.01f;
    }

    // ����� �뽬 Ƚ�� �ʱ�ȭ
    public void ResetDashes()
    {
        _numberOfDashesUsed = 0;
    }

    #endregion
}
