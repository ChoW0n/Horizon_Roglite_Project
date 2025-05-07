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

    // 대쉬 입력 확인 및 조건에 따라 대쉬 시작
    public void DashCheck()
    {
        // 대쉬 키가 눌렸을 때
        if (InputManager.DashWaspPressed)
        {
            // 지상에서 대쉬 가능 조건
            if (Controller._isGrounded && _dashOnGroundTimer < 0 && !_isDashing)
            {
                InitiateDash(); // 대쉬 실행  
            }
            // 공중에서 대쉬 가능 조건
            else if (!Controller._isGrounded && !_isDashing && _numberOfDashesUsed < Controller.PlayerSO.NumberOfDashes)
            {
                _isAirDashing = true;   // 공중 대쉬 상태 설정
                InitiateDash();         // 대쉬 실행

                // 벽 점프 직후의 짧은 시간 안이라면 점프 횟수 하나 복구
                if (WallJump._wallJumpPostBufferTimer > 0f)
                {
                    Jump._numberOfJumpsUsed--;
                    if (Jump._numberOfJumpsUsed < 0f)
                        Jump._numberOfJumpsUsed = 0;
                }
            }
        }
    }

    // 대쉬 방향 계산 및 대쉬 상태 초기화
    public void InitiateDash()
    {
        // 현재 입력 방향 저장
        _dashDirection = InputManager.Movement;

        // 가장 가까운 대쉬 방향
        Vector2 closestDirection = Vector2.zero;
        float minDistance = Vector2.Distance(_dashDirection, Controller.PlayerSO.DashDirections[0]);

        // 가장 가까운 유효 대쉬 방향 찾기
        for (int i = 0; i < Controller.PlayerSO.DashDirections.Length; i++)
        {
            // 정확히 일치하는 방향이 있으면 바로 설정
            if (_dashDirection == Controller.PlayerSO.DashDirections[i])
            {
                closestDirection = _dashDirection;
                break;
            }

            float distance = Vector2.Distance(_dashDirection, Controller.PlayerSO.DashDirections[i]);

            // 대각선 방향일 경우 편향값을 적용
            bool isDiagonal = (Mathf.Abs(Controller.PlayerSO.DashDirections[i].x) == 1 && Mathf.Abs(Controller.PlayerSO.DashDirections[i].y) == 1);
            if (isDiagonal)
            {
                distance -= Controller.PlayerSO.DashDiagonallyBias;
            }

            // 가장 가까운 방향 갱신
            else if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = Controller.PlayerSO.DashDirections[i];
            }
        }

        // 아무 방향 입력이 없을 경우 바라보는 방향으로 설정
        if (closestDirection == Vector2.zero)
        {
            if (Controller._isFacingRight)
                closestDirection = Vector2.right;
            else
                closestDirection = Vector2.left;
        }

        // 최종 대쉬 방향 설정 및 대쉬 상태 초기화
        _dashDirection = closestDirection;
        _numberOfDashesUsed++;
        _isDashing = true;
        Controller.AnimManager.ChangeAnimationState(AnimationManager.PlayerAnimationState.Dash);
        EffectManager.instance.PlayEffect("Dash", this.gameObject.transform.position, Quaternion.identity);
        _dashTimer = 0f;
        _dashOnGroundTimer = Controller.PlayerSO.TimeBtwDashesOnGround;

        // 점프 및 벽 점프 관련 상태 초기화
        Jump.ResetJumpvalues();
        WallJump.ResetWallJumpValues();
        WallSlide.StopWallSlide();
    }

    // FixedUpdate 등에서 호출되어 대쉬 중 물리 처리 수행
    public void Dash()
    {
        // 대쉬 중일 때
        if (_isDashing)
        {
            _dashTimer += Time.fixedDeltaTime;

            // 대쉬 시간 종료 시
            if (_dashTimer >= Controller.PlayerSO.DashTime)
            {
                // 지상에서 대쉬를 끝낸 경우 대쉬 횟수 리셋
                if (Controller._isGrounded)
                    ResetDashes();

                _isAirDashing = false;
                _isDashing = false;

                // 점프나 벽 점프 상태가 아니면 빠른 낙하 준비
                if (!Jump._isJumping && !WallJump._isWallJumping)
                {
                    _dashFastFallTime = 0f;
                    _dashFastFallReleaseSpeed = Jump.VerticalVelocity;

                    if (!Controller._isGrounded)
                        _isDashFastFalling = true;
                }

                return;
            }

            // 대쉬 방향에 따라 속도 적용
            Controller.HorizontalVelocity = Controller.PlayerSO.DashSpeed * _dashDirection.x;

            if (_dashDirection.y != 0f || _isAirDashing)
                Jump.VerticalVelocity = Controller.PlayerSO.DashSpeed * _dashDirection.y;
        }
        // 대쉬 후 빠른 낙하 처리
        else if (_isDashFastFalling)
        {
            if (Jump.VerticalVelocity > 0f)
            {
                // 위로 상승 중인 경우 → 일정 시간까지 천천히 멈춤
                if (_dashFastFallTime < Controller.PlayerSO.DashTimeForUpwardsCancel)
                {
                    Jump.VerticalVelocity = Mathf.Lerp(_dashFastFallReleaseSpeed, 0f, (_dashFastFallTime / Controller.PlayerSO.DashTimeForUpwardsCancel));
                }
                // 이후에는 중력에 따라 낙하
                else
                {
                    Jump.VerticalVelocity += Controller.PlayerSO.Gravity * Controller.PlayerSO.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }

                _dashFastFallTime += Time.fixedDeltaTime;
            }
            else
            {
                // 낙하 중일 경우 중력 가속
                Jump.VerticalVelocity += Controller.PlayerSO.Gravity * Controller.PlayerSO.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
        }
    }

    // 대쉬 관련 낙하 상태 초기화
    public void ResetDashValues()
    {
        _isDashFastFalling = false;
        _dashOnGroundTimer = -0.01f;
    }

    // 사용한 대쉬 횟수 초기화
    public void ResetDashes()
    {
        _numberOfDashesUsed = 0;
    }

    #endregion
}
