using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlide : MonoBehaviour
{
    #region ���۷���
    [Header("References")]
    private PlayerController Controller;
    private PlayerJump Jump;
    private PlayerWallJump WallJump;
    private PlayerDash Dash;

    [Tooltip("Wall Slide Vars")]
    public bool _isWallSliding {  get; private set; }
    public bool _isWallSlideFalling { get; set; }

    #endregion

    #region �ʱ�ȭ
    private void Start()
    {
        Controller = GetComponent<PlayerController>();
        Jump = GetComponent<PlayerJump>();
        WallJump = GetComponent<PlayerWallJump>();
        Dash = GetComponent<PlayerDash>();
    }

    #endregion

    #region �� �����̵�

    // �� �����̵� ���� ���� Ȯ�� �� ���� ����/���� �Ǵ�
    public void WallSlideCheck()
    {
        // ���� ��� �ְ�, �����̸�, �뽬 ���� �ƴ� ��
        if (Controller._isTouchingWall && !Controller._isGrounded && !Dash._isDashing)
        {
            // �Ʒ��� ���� ���̰� ���� �����̵� ���°� �ƴ� ��
            if (Jump.VerticalVelocity < 0f && !_isWallSliding)
            {
                Jump.ResetJumpvalues();          // ���� ���� �ʱ�ȭ
                WallJump.ResetWallJumpValues();      // �� ���� ���� ���� �ʱ�ȭ
                Dash.ResetDashValues();          // �뽬 ���� �ʱ�ȭ

                // ������ ���� �� �����̵� �� �뽬 Ƚ�� ����
                if (Controller.PlayerSO.ResetDashOnWallSlide)
                {
                    Dash.ResetDashes();
                }

                _isWallSlideFalling = false;    // �� �����̵� ���� ���� ���� �ʱ�ȭ
                _isWallSliding = true;          // �� �����̵� ����

                // ������ ���� ���� Ƚ���� ����
                if (Controller.PlayerSO.ResetJumpsOnWallSlide)
                    Jump._numberOfJumpsUsed = 0;
            }
        }
        // �� �����̵� ���ε� ������ �������� ���� ���� �ƴϸ� ���ϵ� ���� �� ���� ��
        else if (_isWallSliding && !Controller._isTouchingWall && !Controller._isGrounded && !_isWallSlideFalling)
        {
            _isWallSlideFalling = true;     // ���� ���·� ��ȯ
            StopWallSlide();                // �����̵� �ߴ�
        }
        // �� �ܿ��� ��� �����̵� �ߴ�
        else
        {
            StopWallSlide();
        }
    }

    // �� �����̵� ���� ���� ó��
    public void StopWallSlide()
    {
        if (_isWallSliding)
        {
            Jump._numberOfJumpsUsed++;       // �����̵忡�� ��� �� ���� Ƚ�� 1 ����
            _isWallSliding = false;
        }
    }

    // �� �����̵� ���� ���� �� ���� ���� ó��
    public void WallSlide()
    {
        if (_isWallSliding)
        {
            // ���������� ��ǥ �����̵� �ӵ��� �����ϸ� ����
            Jump.VerticalVelocity = Mathf.Lerp(
                Jump.VerticalVelocity,
                -Controller.PlayerSO.WallSlideSpeed,
                Controller.PlayerSO.WallSlideDecelerationSpeed * Time.fixedDeltaTime
            );
        }
    }

    #endregion
}
