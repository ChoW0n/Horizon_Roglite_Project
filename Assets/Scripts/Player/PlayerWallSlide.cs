using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlide : MonoBehaviour
{
    #region 레퍼런스
    [Header("References")]
    private PlayerController Controller;
    private PlayerJump Jump;
    private PlayerWallJump WallJump;
    private PlayerDash Dash;

    [Tooltip("Wall Slide Vars")]
    public bool _isWallSliding {  get; private set; }
    public bool _isWallSlideFalling { get; set; }

    #endregion

    #region 초기화
    private void Start()
    {
        Controller = GetComponent<PlayerController>();
        Jump = GetComponent<PlayerJump>();
        WallJump = GetComponent<PlayerWallJump>();
        Dash = GetComponent<PlayerDash>();
    }

    #endregion

    #region 벽 슬라이드

    // 벽 슬라이드 가능 여부 확인 및 상태 진입/종료 판단
    public void WallSlideCheck()
    {
        // 벽에 닿아 있고, 공중이며, 대쉬 중이 아닐 때
        if (Controller._isTouchingWall && !Controller._isGrounded && !Dash._isDashing)
        {
            // 아래로 낙하 중이고 아직 슬라이드 상태가 아닐 때
            if (Jump.VerticalVelocity < 0f && !_isWallSliding)
            {
                Jump.ResetJumpvalues();          // 점프 상태 초기화
                WallJump.ResetWallJumpValues();      // 벽 점프 관련 상태 초기화
                Dash.ResetDashValues();          // 대쉬 상태 초기화

                // 설정에 따라 벽 슬라이드 시 대쉬 횟수 리셋
                if (Controller.PlayerSO.ResetDashOnWallSlide)
                {
                    Dash.ResetDashes();
                }

                _isWallSlideFalling = false;    // 벽 슬라이드 도중 낙하 상태 초기화
                _isWallSliding = true;          // 벽 슬라이드 시작

                // 설정에 따라 점프 횟수도 리셋
                if (Controller.PlayerSO.ResetJumpsOnWallSlide)
                    Jump._numberOfJumpsUsed = 0;
            }
        }
        // 벽 슬라이드 중인데 벽에서 떨어졌고 아직 지상도 아니며 낙하도 시작 안 했을 때
        else if (_isWallSliding && !Controller._isTouchingWall && !Controller._isGrounded && !_isWallSlideFalling)
        {
            _isWallSlideFalling = true;     // 낙하 상태로 전환
            StopWallSlide();                // 슬라이드 중단
        }
        // 그 외에는 모두 슬라이드 중단
        else
        {
            StopWallSlide();
        }
    }

    // 벽 슬라이드 상태 종료 처리
    public void StopWallSlide()
    {
        if (_isWallSliding)
        {
            Jump._numberOfJumpsUsed++;       // 슬라이드에서 벗어날 때 점프 횟수 1 증가
            _isWallSliding = false;
        }
    }

    // 벽 슬라이드 실행 중일 때 느린 낙하 처리
    public void WallSlide()
    {
        if (_isWallSliding)
        {
            // 점진적으로 목표 슬라이드 속도로 보간하며 낙하
            Jump.VerticalVelocity = Mathf.Lerp(
                Jump.VerticalVelocity,
                -Controller.PlayerSO.WallSlideSpeed,
                Controller.PlayerSO.WallSlideDecelerationSpeed * Time.fixedDeltaTime
            );
        }
    }

    #endregion
}
