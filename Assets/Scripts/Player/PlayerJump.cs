using HRP.AnimatorCoder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    #region 레퍼런스
    [Header("References")]
    private PlayerController Controller;
    private PlayerWallJump WallJump;
    private PlayerWallSlide WallSlide;
    private PlayerDash Dash;

    [Header("Jump Vars")]
    private float _fastFallReleaseSpeed;
    public int _numberOfJumpsUsed { get; set; }
    public bool _isJumping { get; private set; }
    public bool _isFastFalling { get; private set; }
    public bool _isFalling { get; private set; }
    public float _fastFallTime { get; private set; }
    public float VerticalVelocity { get; set; }

    [Header("Apex Vars")]
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    [Tooltip("Jump Buffer Vars")]
    public float _jumpBufferTimer { get; set; }
    private bool _jumpReleasedDuringBuffer;

    #endregion

    #region 초기화
    private void Start()
    {
        Controller = GetComponent<PlayerController>();
        WallJump = GetComponent<PlayerWallJump>();
        WallSlide = GetComponent<PlayerWallSlide>();
        Dash = GetComponent<PlayerDash>();
    }

    #endregion

    #region 점프
    /// <summary>
    /// 점프 입력 처리 및 점프 상태 전이 관리
    /// </summary>
    public void ResetJumpvalues()
    {
        _isJumping = false;
        _isFalling = false;
        _isFastFalling = false;
        _fastFallTime = 0f;
        _isPastApexThreshold = false;
    }

    public void JumpChecks()
    {
        if (InputManager.JumpWasPressed)
        {
            if (WallSlide._isWallSlideFalling && WallJump._wallJumpPostBufferTimer >= 0f) return;
            else if (WallSlide._isWallSliding || (Controller._isTouchingWall && !Controller._isGrounded)) return;

            _jumpBufferTimer = Controller.PlayerSO.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        if (InputManager.JumpWasReleased)
        {
            if (_jumpBufferTimer > 0f)
                _jumpReleasedDuringBuffer = true;

            // 점프 도중 점프 키에서 손을 떼면 빠르게 낙하
            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = Controller.PlayerSO.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        // 점프 버퍼 + 코요테 타임 점프
        if (_jumpBufferTimer > 0f && !_isJumping && (Controller._isGrounded || Controller._coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        // 공중 점프 (다단점프)
        else if (_jumpBufferTimer > 0f && (_isJumping || WallJump._isWallJumping || WallSlide._isWallSlideFalling || Dash._isAirDashing || Dash._isDashFastFalling)
            && !Controller._isTouchingWall && _numberOfJumpsUsed < Controller.PlayerSO.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);

            if (Dash._isDashFastFalling)
                Dash._isDashFastFalling = false;
        }
        // 낙하 중 추가 점프 (다단점프 예외)
        else if (_jumpBufferTimer > 0f && _isFalling && !WallSlide._isWallSlideFalling && _numberOfJumpsUsed < Controller.PlayerSO.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }
    }

    public void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
            _isJumping = true;

        Controller.Play(new(Animations.JUMP, true));

        WallJump.ResetWallJumpValues();

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = Controller.PlayerSO.InitialJumpVelocity;
    }

    /// <summary>
    /// 점프 상태에서의 수직 속도 및 낙하 가속 처리
    /// </summary>
    public void Jump()
    {
        if (_isJumping)
        {
            if (Controller._bumpedHead)
                _isFastFalling = true;

            if (VerticalVelocity >= 0f)
            {
                // 점프 중간 지점 도달 판단
                _apexPoint = Mathf.InverseLerp(Controller.PlayerSO.InitialJumpVelocity, 0f, VerticalVelocity);
                
                if (_apexPoint > Controller.PlayerSO.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < Controller.PlayerSO.ApexHangTime)
                            VerticalVelocity = 0f;
                        else
                            VerticalVelocity = -0.01f;
                    }
                }
                else if (!_isFastFalling)
                {
                    VerticalVelocity += Controller.PlayerSO.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                        _isPastApexThreshold = false;
                }
            }
            else if (!_isFastFalling)
            {
                VerticalVelocity += Controller.PlayerSO.Gravity * Controller.PlayerSO.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (VerticalVelocity < 0f && !_isFalling)
            {
                _isFalling = true;
            }
        }

        // 빠른 낙하 처리
        if (_isFastFalling)
        {
            if (_fastFallTime >= Controller.PlayerSO.TimeForUpwardsCancel)
                VerticalVelocity += Controller.PlayerSO.Gravity * Controller.PlayerSO.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            else if (_fastFallTime < Controller.PlayerSO.TimeForUpwardsCancel)
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / Controller.PlayerSO.TimeForUpwardsCancel));

            _fastFallTime += Time.fixedDeltaTime;
        }
    }

    #endregion

    #region 착지/낙하

    public void LandCheck()
    {
        // 착지 시 상태 초기화
        if ((_isJumping || _isFalling || WallJump._isWallJumpFalling || WallJump._isWallJumping || WallSlide._isWallSlideFalling || WallSlide._isWallSliding || Dash._isDashFastFalling)
            && Controller._isGrounded && VerticalVelocity <= 0f)
        {
            ResetJumpvalues();
            WallSlide.StopWallSlide();
            WallJump.ResetWallJumpValues();
            Dash.ResetDashes();

            _numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;

            Controller.Play(new(Animations.LAND, true));
            EffectManager.instance.PlayEffect("Land", this.gameObject.transform.position, Quaternion.identity);

            if (Dash._isDashFastFalling && Controller._isGrounded)
            {
                Dash.ResetDashValues();
                return;
            }

            Dash.ResetDashValues();
        }
    }

    public void Fall()
    {
        if (!Controller._isGrounded && !_isJumping && !WallSlide._isWallSliding && !WallJump._isWallJumping && !Dash._isDashing && !Dash._isDashFastFalling)
        {
            if (!_isFalling)
                _isFalling = true;

            VerticalVelocity += Controller.PlayerSO.Gravity * Time.fixedDeltaTime;
        }
    }

    #endregion
}
