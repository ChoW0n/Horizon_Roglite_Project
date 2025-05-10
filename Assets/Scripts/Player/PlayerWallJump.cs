using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJump : MonoBehaviour
{
    #region ���۷���
    [Header("References")]
    private PlayerController Controller;
    private PlayerJump Jump;
    private PlayerWallSlide WallSlide;

    [Header("Wall Jump Vars")]
    private float _wallJumpTime;
    private bool _isWallJumpFastFalling;
    private float _wallJumpFastFallTime;
    private float _wallJumpFastFallReleaseSpeed;
    private float _walljumpApexPoint;
    private float _timePastWallJumpApexThreshold;
    private bool _isPastWallJumpApexThreshold;
    public bool _useWallJumpMoveState {  get; private set; }
    public bool _isWallJumping { get; private set; }
    public bool _isWallJumpFalling { get; private set; }
    public float _wallJumpPostBufferTimer { get; set; }

    #endregion

    #region �ʱ�ȭ
    private void Start()
    {
        Controller = GetComponent<PlayerController>();
        Jump = GetComponent<PlayerJump>();
        WallSlide = GetComponent<PlayerWallSlide>();
    }

    #endregion

    #region �� ����

    // �� ���� ���� üũ
    public void WallJumpCheck()
    {
        // �� ���� ���� ���� ���� �� Ÿ�̸� �ʱ�ȭ
        if (ShouldApplyPostWallJumpBuffer())
            _wallJumpPostBufferTimer = Controller.PlayerSO.WallJumpPostBufferTime;

        // ���� ��ư�� �ð�, ���� Ÿ�� �ʰ� ������, �� ���� ���̶��
        if (InputManager.JumpWasReleased && !WallSlide._isWallSliding && !Controller._isTouchingWall && _isWallJumping)
        {
            if (Jump.VerticalVelocity > 0f) // ��� ���� ��
            {
                if (_isPastWallJumpApexThreshold) // ���� ������ ���� ���
                {
                    _isPastWallJumpApexThreshold = false;
                    _isWallJumpFastFalling = true;
                    _wallJumpFastFallTime = Controller.PlayerSO.TimeForUpwardsCancel;

                    Jump.VerticalVelocity = 0f; // ��� ���߰� ���� ����
                }
                else
                {
                    _isWallJumpFastFalling = true;
                    _wallJumpFastFallReleaseSpeed = Jump.VerticalVelocity; // ���� �ӵ� ����
                }
            }
        }

        // �� ���� ���� �ð� ���� ���� ��ư�� ���� ��� �� ���� ����
        if (InputManager.JumpWasPressed && _wallJumpPostBufferTimer > 0f)
        {
            InitiateWallJump();
        }
    }

    // �� ���� ����
    public void InitiateWallJump()
    {
        if (!_isWallJumping)
        {
            _isWallJumping = true;
            _useWallJumpMoveState = true;
        }

        WallSlide.StopWallSlide();        // �� �����̵� �ߴ�
        Jump.ResetJumpvalues();      // ���� ���� �� �ʱ�ȭ
        _wallJumpTime = 0f;     // �� ���� �ð� �ʱ�ȭ

        Jump.VerticalVelocity = Controller.PlayerSO.InitialWallJumpVelocity; // �ʱ� ���� �ӵ� ����

        int num = 0;
        num = ((!(Controller._lastWallHit.collider.ClosestPoint(Controller._bodyColl.bounds.center).x > base.transform.position.x)) ? 1 : (-1));
        Controller.HorizontalVelocity = new Vector2(Mathf.Abs(Controller.PlayerSO.WallJumpDirection.x) * (float)num, 0f);
    }

    // �� ���� ���� ���� ó��
    public void WallJump()
    {
        if (_isWallJumping)
        {
            _wallJumpTime += Time.fixedDeltaTime;

            // ���� ���� ���� �ð� ���Ŀ� �� ���� ���� ����
            if (_wallJumpTime >= Controller.PlayerSO.TimeTillJumpApex)
                _useWallJumpMoveState = false;

            // �Ӹ��� �ε��� ��� ������ ���� ����
            if (Controller._bumpedHead)
            {
                _isWallJumpFastFalling = true;
                _useWallJumpMoveState = false;
            }

            if (Jump.VerticalVelocity >= 0f) // ��� ��
            {
                // ���� ��� �ӵ��� ���� ������ �󸶳� ������� ���
                _walljumpApexPoint = Mathf.InverseLerp(Controller.PlayerSO.WallJumpDirection.y, 0f, Jump.VerticalVelocity);

                // ������ ������ ���
                if (_walljumpApexPoint > Controller.PlayerSO.ApexThreshold)
                {
                    if (!_isPastWallJumpApexThreshold)
                    {
                        _isPastWallJumpApexThreshold = true;
                        _timePastWallJumpApexThreshold = 0f;
                    }

                    // ���� ���� �ð� ���� ���� �ӵ� 0���� ����
                    if (_isPastWallJumpApexThreshold)
                    {
                        _timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                        if (_timePastWallJumpApexThreshold < Controller.PlayerSO.ApexHangTime)
                            Jump.VerticalVelocity = 0f;
                        else
                            Jump.VerticalVelocity = -0.01f; // ���� ���� �ð��� �������� ���� ���� ����
                    }
                }
                else if (!_isWallJumpFastFalling)
                {
                    // ���� ���� ������ �� ������ �߷� ����
                    Jump.VerticalVelocity += Controller.PlayerSO.WallJumpGravity * Time.fixedDeltaTime;

                    // ���� ���� �ʱ�ȭ
                    if (_isPastWallJumpApexThreshold)
                        _isPastWallJumpApexThreshold = false;
                }
            }

            else if (!_isWallJumpFastFalling) // ���� ���̰� ���� ���ϰ� �ƴ� ���
            {
                Jump.VerticalVelocity += Controller.PlayerSO.WallJumpGravity + Time.fixedDeltaTime;
            }
            else if (Jump.VerticalVelocity < 0f) // ���� ���� ���̸� ���� ���·� ��ȯ
            {
                if (!_isWallJumpFalling)
                    _isWallJumpFalling = true;
            }
        }

        // ���� ���� ������ ��
        if (_isWallJumpFastFalling)
        {
            if (_wallJumpFastFallTime >= Controller.PlayerSO.TimeForUpwardsCancel)
            {
                // ���� �߷��� ������ ������ �����
                Jump.VerticalVelocity += Controller.PlayerSO.WallJumpGravity * Controller.PlayerSO.WallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                // ���� ��ư �� �� ���� �ð��� ���� �ε巴�� ����
                Jump.VerticalVelocity = Mathf.Lerp(_wallJumpFastFallReleaseSpeed, 0f, (_wallJumpFastFallTime / Controller.PlayerSO.TimeForUpwardsCancel));
            }

            _wallJumpFastFallTime += Time.fixedDeltaTime;
        }
    }

    // �� ���� ���� ���� �Ǵ� (���߿� �ְ�, ���� Ÿ�� �ְų� ���� �پ� ���� ��)
    public bool ShouldApplyPostWallJumpBuffer()
    {
        if (!Controller._isGrounded & (Controller._isTouchingWall || WallSlide._isWallSliding)) return true;
        else return false;
    }

    // �� ���� ���� �� �ʱ�ȭ
    public void ResetWallJumpValues()
    {
        WallSlide._isWallSlideFalling = false;
        _useWallJumpMoveState = false;
        _isWallJumping = false;
        _isWallJumpFastFalling = false;
        _isWallJumpFalling = false;
        _isPastWallJumpApexThreshold = false;

        _wallJumpFastFallTime = 0;
        _wallJumpTime = 0;
    }

    #endregion
}
