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
    public bool _isDashing;
    public bool _isAirDashing;
    public float _dashTimer;
    public float _dashOnGroundTimer;
    private int _numberOfDashesUsed;
    private Vector2 _dashDirection;
    public bool _isDashFastFalling;
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
            else if (!Controller._isGrounded && !_isDashing && _numberOfDashesUsed < Controller.MoveStats.NumberOfDashes)
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
        float minDistance = Vector2.Distance(_dashDirection, Controller.MoveStats.DashDirections[0]);

        // ���� ����� ��ȿ �뽬 ���� ã��
        for (int i = 0; i < Controller.MoveStats.DashDirections.Length; i++)
        {
            // ��Ȯ�� ��ġ�ϴ� ������ ������ �ٷ� ����
            if (_dashDirection == Controller.MoveStats.DashDirections[i])
            {
                closestDirection = _dashDirection;
                break;
            }

            float distance = Vector2.Distance(_dashDirection, Controller.MoveStats.DashDirections[i]);

            // �밢�� ������ ��� ���Ⱚ�� ����
            bool isDiagonal = (Mathf.Abs(Controller.MoveStats.DashDirections[i].x) == 1 && Mathf.Abs(Controller.MoveStats.DashDirections[i].y) == 1);
            if (isDiagonal)
            {
                distance -= Controller.MoveStats.DashDiagonallyBias;
            }

            // ���� ����� ���� ����
            else if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = Controller.MoveStats.DashDirections[i];
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
        _dashTimer = 0f;
        _dashOnGroundTimer = Controller.MoveStats.TimeBtwDashesOnGround;

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
            if (_dashTimer >= Controller.MoveStats.DashTime)
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
            Controller.HorizontalVelocity = Controller.MoveStats.DashSpeed * _dashDirection.x;

            if (_dashDirection.y != 0f || _isAirDashing)
                Jump.VerticalVelocity = Controller.MoveStats.DashSpeed * _dashDirection.y;
        }
        // �뽬 �� ���� ���� ó��
        else if (_isDashFastFalling)
        {
            if (Jump.VerticalVelocity > 0f)
            {
                // ���� ��� ���� ��� �� ���� �ð����� õõ�� ����
                if (_dashFastFallTime < Controller.MoveStats.DashTimeForUpwardsCancel)
                {
                    Jump.VerticalVelocity = Mathf.Lerp(_dashFastFallReleaseSpeed, 0f, (_dashFastFallTime / Controller.MoveStats.DashTimeForUpwardsCancel));
                }
                // ���Ŀ��� �߷¿� ���� ����
                else
                {
                    Jump.VerticalVelocity += Controller.MoveStats.Gravity * Controller.MoveStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }

                _dashFastFallTime += Time.fixedDeltaTime;
            }
            else
            {
                // ���� ���� ��� �߷� ����
                Jump.VerticalVelocity += Controller.MoveStats.Gravity * Controller.MoveStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
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
