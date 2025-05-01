using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJump : MonoBehaviour
{
    [Header("References")]
    private PlayerController Controller;
    private PlayerJump Jump;
    private PlayerWallSlide WallSlide;

    [Header("Wall Jump Vars")]
    public bool _useWallJumpMoveState;
    public bool _isWallJumping;
    private float _wallJumpTime;
    private bool _isWallJumpFastFalling;
    public bool _isWallJumpFalling;
    private float _wallJumpFastFallTime;
    private float _wallJumpFastFallReleaseSpeed;
    public float _wallJumpPostBufferTimer;
    private float _walljumpApexPoint;
    private float _timePastWallJumpApexThreshold;
    private bool _isPastWallJumpApexThreshold;

    private void Start()
    {
        Controller = GetComponent<PlayerController>();
        Jump = GetComponent<PlayerJump>();
        WallSlide = GetComponent<PlayerWallSlide>();
    }

    #region Wall Jump

    // 벽 점프 조건 체크
    public void WallJumpCheck()
    {
        // 벽 점프 버퍼 조건 만족 시 타이머 초기화
        if (ShouldApplyPostWallJumpBuffer())
            _wallJumpPostBufferTimer = Controller.MoveStats.WallJumpPostBufferTime;

        // 점프 버튼을 뗐고, 벽을 타지 않고 있으며, 벽 점프 중이라면
        if (InputManager.JumpWasReleased && !WallSlide._isWallSliding && !Controller._isTouchingWall && _isWallJumping)
        {
            if (Jump.VerticalVelocity > 0f) // 상승 중일 때
            {
                if (_isPastWallJumpApexThreshold) // 정점 지점을 지난 경우
                {
                    _isPastWallJumpApexThreshold = false;
                    _isWallJumpFastFalling = true;
                    _wallJumpFastFallTime = Controller.MoveStats.TimeForUpwardsCancel;

                    Jump.VerticalVelocity = 0f; // 상승 멈추고 낙하 시작
                }
                else
                {
                    _isWallJumpFastFalling = true;
                    _wallJumpFastFallReleaseSpeed = Jump.VerticalVelocity; // 현재 속도 저장
                }
            }
        }

        // 벽 점프 버퍼 시간 내에 점프 버튼을 누른 경우 벽 점프 실행
        if (InputManager.JumpWasPressed && _wallJumpPostBufferTimer > 0f)
        {
            InitiateWallJump();
        }
    }

    // 벽 점프 실행
    public void InitiateWallJump()
    {
        if (!_isWallJumping)
        {
            _isWallJumping = true;
            _useWallJumpMoveState = true;
        }

        WallSlide.StopWallSlide();        // 벽 슬라이드 중단
        Jump.ResetJumpvalues();      // 점프 관련 값 초기화
        _wallJumpTime = 0f;     // 벽 점프 시간 초기화

        Jump.VerticalVelocity = Controller.MoveStats.InitialWallJumpVelocity; // 초기 수직 속도 설정

        int dirMultiplier = 0;
        Vector2 hitPoint = Controller._lastWallHit.collider.ClosestPoint(Controller._bodyColl.bounds.center);

        // 플레이어 기준 벽 위치에 따라 반대 방향으로 점프
        if (hitPoint.x > transform.position.x)
            dirMultiplier = -1;
        else
            dirMultiplier = 1;

        Controller.HorizontalVelocity = Mathf.Abs(Controller.MoveStats.WallJumpDirection.x) * dirMultiplier;
    }

    // 벽 점프 도중 물리 처리
    public void WallJump()
    {
        if (_isWallJumping)
        {
            _wallJumpTime += Time.fixedDeltaTime;

            // 점프 정점 도달 시간 이후엔 벽 점프 상태 해제
            if (_wallJumpTime >= Controller.MoveStats.TimeTillJumpApex)
                _useWallJumpMoveState = false;

            // 머리를 부딪힌 경우 빠르게 낙하 시작
            if (Controller._bumpedHead)
            {
                _isWallJumpFastFalling = true;
                _useWallJumpMoveState = false;
            }

            if (Jump.VerticalVelocity >= 0f) // 상승 중
            {
                // 현재 상승 속도로 점프 정점에 얼마나 가까운지 계산
                _walljumpApexPoint = Mathf.InverseLerp(Controller.MoveStats.WallJumpDirection.y, 0f, Jump.VerticalVelocity);

                // 정점에 도달한 경우
                if (_walljumpApexPoint > Controller.MoveStats.ApexThreshold)
                {
                    if (!_isPastWallJumpApexThreshold)
                    {
                        _isPastWallJumpApexThreshold = true;
                        _timePastWallJumpApexThreshold = 0f;
                    }

                    // 정점 유지 시간 동안 수직 속도 0으로 유지
                    if (_isPastWallJumpApexThreshold)
                    {
                        _timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                        if (_timePastWallJumpApexThreshold < Controller.MoveStats.ApexHangTime)
                            Jump.VerticalVelocity = 0f;
                        else
                            Jump.VerticalVelocity = -0.01f; // 정점 유지 시간이 끝났으면 느린 낙하 시작
                    }
                }
                else if (!_isWallJumpFastFalling)
                {
                    // 정점 도달 전에는 벽 점프용 중력 적용
                    Jump.VerticalVelocity += Controller.MoveStats.WallJumpGravity * Time.fixedDeltaTime;

                    // 정점 상태 초기화
                    if (_isPastWallJumpApexThreshold)
                        _isPastWallJumpApexThreshold = false;
                }
            }

            else if (!_isWallJumpFastFalling) // 낙하 중이고 빠른 낙하가 아닌 경우
            {
                Jump.VerticalVelocity += Controller.MoveStats.WallJumpGravity + Time.fixedDeltaTime;
            }
            else if (Jump.VerticalVelocity < 0f) // 빠른 낙하 중이면 낙하 상태로 전환
            {
                if (!_isWallJumpFalling)
                    _isWallJumpFalling = true;
            }
        }

        // 빠른 낙하 상태일 때
        if (_isWallJumpFastFalling)
        {
            if (_wallJumpFastFallTime >= Controller.MoveStats.TimeForUpwardsCancel)
            {
                // 낙하 중력이 증가된 비율로 적용됨
                Jump.VerticalVelocity += Controller.MoveStats.WallJumpGravity * Controller.MoveStats.WallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                // 점프 버튼 뗀 후 남은 시간에 따라 부드럽게 감속
                Jump.VerticalVelocity = Mathf.Lerp(_wallJumpFastFallReleaseSpeed, 0f, (_wallJumpFastFallTime / Controller.MoveStats.TimeForUpwardsCancel));
            }

            _wallJumpFastFallTime += Time.fixedDeltaTime;
        }
    }

    // 벽 점프 버퍼 조건 판단 (공중에 있고, 벽을 타고 있거나 벽에 붙어 있을 때)
    public bool ShouldApplyPostWallJumpBuffer()
    {
        if (!Controller._isGrounded & (Controller._isTouchingWall || WallSlide._isWallSliding)) return true;
        else return false;
    }

    // 벽 점프 관련 값 초기화
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
