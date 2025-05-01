using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerController : MonoBehaviour
{
    #region References
    [Header("References")]
    public PlayerMovementStats MoveStats;
    private PlayerWallSlide WallSlide;
    private PlayerWallJump WallJump;
    private PlayerDash Dash;
    private PlayerJump Jump;

    public Collider2D _feetColl;
    public Collider2D _bodyColl;

    private Rigidbody2D _rb;

    // Movement Vars
    [Header("Movement Vars")]
    public float HorizontalVelocity;
    public bool _isFacingRight;

    [Header("Camera")]
    public GameObject _cameraFollowGO;
    private CameraFollowOBJ _cameraFollowOBJ;
    private float _fallSpeedYDampingChangeThreshold;

    [Header("Coyote time Vars")]
    public float _coyoteTimer;

    [Header("Collision Check Vars")]
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private RaycastHit2D _wallHit;
    public RaycastHit2D _lastWallHit;
    public bool _isGrounded;
    public bool _bumpedHead;
    public bool _isTouchingWall;

    #endregion

    #region Unity CallBack Functions

    private void Start()
    {
        _isFacingRight = true;

        _rb = GetComponent<Rigidbody2D>();
        _cameraFollowOBJ = _cameraFollowGO.GetComponent<CameraFollowOBJ>();
        WallSlide = GetComponent<PlayerWallSlide>();
        WallJump = GetComponent<PlayerWallJump>();
        Dash = GetComponent<PlayerDash>();
        Jump = GetComponent<PlayerJump>();

        _fallSpeedYDampingChangeThreshold = CameraManager.instance.fallSpeedYDampingChangeThreshold;
    }

    private void Update()
    {
        Jump.JumpChecks();
        CountTimers();
        Jump.LandCheck();
        WallSlide.WallSlideCheck();
        WallJump.WallJumpCheck();
        Dash.DashCheck();

        // ī�޶� Y Damping ���� (����/���� �� �ڿ������� ������ ���� ����)
        if (_rb.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.isLerpingYDamping && !CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        if (_rb.velocity.y >= 0f && !CameraManager.instance.isLerpingYDamping && CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.lerpedFromPlayerFalling = false;

            CameraManager.instance.LerpYDamping(false);
        }
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump.Jump();
        Jump.Fall();
        WallSlide.WallSlide();
        WallJump.WallJump();
        Dash.Dash();

        // ����/���� ���ӵ��� ���� �̵� ó��
        if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            if (WallJump._useWallJumpMoveState)
                Move(MoveStats.WallJumpMoveAcceleration, MoveStats.WallJumpMoveDeceleration, InputManager.Movement);
            else
                Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }

        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        if (!Dash._isDashing)
            Jump.VerticalVelocity = Mathf.Clamp(Jump.VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        else
            Jump.VerticalVelocity = Mathf.Clamp(Jump.VerticalVelocity, -50f, 50f);

        _rb.velocity = new Vector2(HorizontalVelocity, Jump.VerticalVelocity);
    }

    private void OnDrawGizmos()
    {
        MoveStats.DrawRight = _isFacingRight;

        if (MoveStats.ShowWalkJumpArc)
        {
            DrawJumpArc(MoveStats.MaxWalkSpeed, Color.white);
        }
        else if (MoveStats.ShowRunJumpArc)
        {
            DrawJumpArc(MoveStats.MaxRunSpeed, Color.red);
        }
    }

    #endregion

    #region Movement
    /// <summary>
    /// �÷��̾��� �̵� ó�� (�ȱ�/�޸���)
    /// </summary>
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (!Dash._isDashing)
        {
            if (Mathf.Abs(moveInput.x) >= MoveStats.MoveThreshold)
            {
                TurnCheck(moveInput);

                float targetVelocity = 0f;
                if (InputManager.RunIsHeld)
                    targetVelocity = moveInput.x * MoveStats.MaxRunSpeed;
                else
                    targetVelocity = moveInput.x * MoveStats.MaxWalkSpeed;

                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, acceleration + Time.fixedDeltaTime);
            }
            else if (Mathf.Abs(moveInput.x) < MoveStats.MoveThreshold)
            {
                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, 0f, deceleration + Time.fixedDeltaTime);
            }
        }
    }

    /// <summary>
    /// �¿� ���� ó��
    /// </summary>
    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    /// <summary>
    /// ���� ��ȯ + ī�޶� ��� ����
    /// </summary>
    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);

            _cameraFollowOBJ.CallTurn();
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);

            _cameraFollowOBJ.CallTurn();
        }
    }

    #endregion

    #region Timers
    /// <summary>
    /// ���� ���� �� �ڿ��� Ÿ�̸� ����
    /// </summary>
    private void CountTimers()
    {
        Jump._jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
            _coyoteTimer -= Time.deltaTime;
        else
            _coyoteTimer = MoveStats.JumpCoyoteTime;

        if (!WallJump.ShouldApplyPostWallJumpBuffer())
        {
            WallJump._wallJumpPostBufferTimer -= Time.deltaTime;
        }

        if (_isGrounded)
            Dash._dashOnGroundTimer -= Time.deltaTime;
    }

    #endregion

    #region Collision Checks
    /// <summary>
    /// �ٴ� ���� (BoxCast)
    /// </summary>
    public void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLenght);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLenght, MoveStats.GroundLayer);
        if (_groundHit.collider != null)
            _isGrounded = true;
        else
            _isGrounded = false;

        #region Debug Visualization

        if (MoveStats.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if (_isGrounded)
                rayColor = Color.green;
            else
                rayColor = Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MoveStats.GroundDetectionRayLenght), Vector2.right * boxCastSize.x, rayColor);
        }

        #endregion
    }

    /// <summary>
    /// �Ӹ� �浹 ���� (BoxCast)
    /// </summary>
    public void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLenght);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLenght, MoveStats.GroundLayer);
        if (_headHit.collider != null)
            _bumpedHead = true;
        else
            _bumpedHead = false;

        #region Debug Visualization

        if (MoveStats.DebugShowHeadBumpBox)
        {
            float headWidth = MoveStats.HeadWidth;

            Color rayColor;
            if (_bumpedHead)
                rayColor = Color.green;
            else
                rayColor = Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + MoveStats.HeadDetectionRayLenght), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }

        #endregion
    }

    /// <summary>
    /// ���� ��ũ �ð�ȭ
    /// </summary>
    public void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 previousPosition = startPosition;

        float speed = 0f;
        if (MoveStats.DrawRight)
            speed = moveSpeed;
        else
            speed = -moveSpeed;

        Vector2 velocity = new Vector2(speed, MoveStats.InitialJumpVelocity);
        Gizmos.color = gizmoColor;
        float timeStep = 2 * MoveStats.TimeTillJumpApex / MoveStats.ArcResolution;

        for (int i = 0; i < MoveStats.VisualizationSteps; i++)
        {
            float simulationtime = i * timeStep;
            Vector2 displacement, drawPoint;

            if (simulationtime < MoveStats.TimeTillJumpApex)
            {
                displacement = velocity * simulationtime + 0.5f * new Vector2(0, MoveStats.Gravity) * simulationtime * simulationtime;
            }
            else if (simulationtime < MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime)
            {
                float apexTime = simulationtime - MoveStats.TimeTillJumpApex;
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime;
            }
            else
            {
                float descendTime = simulationtime - (MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime);
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * MoveStats.ApexHangTime;
                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0, MoveStats.Gravity) * descendTime * descendTime;
            }
            drawPoint = startPosition + displacement;

            if (MoveStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint), MoveStats.GroundLayer);
                if (hit.collider != null)
                {
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }

    /// <summary>
    /// �� ���� �ð�ȭ
    /// </summary>
    public void IsTouchingWall()
    {
        float originEndPoint = 0f;
        if (_isFacingRight)
            originEndPoint = _bodyColl.bounds.max.x;
        else
            originEndPoint = _bodyColl.bounds.min.x;
        float abjustedHeight = _bodyColl.bounds.size.y * MoveStats.WallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originEndPoint, _bodyColl.bounds.center.y);
        Vector2 boxCastSize = new Vector2(MoveStats.WallDetectionRayLenght, abjustedHeight);

        _wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, MoveStats.WallDetectionRayLenght, MoveStats.GroundLayer);
        if (_wallHit.collider != null)
        {
            _lastWallHit = _wallHit;
            _isTouchingWall = true;
        }
        else
            _isTouchingWall = false;

        #region Debug Visualization

        if (MoveStats.DebugShowWallHitbox)
        {
            Color rayColor;
            if (_isTouchingWall)
                rayColor = Color.green;
            else
                rayColor = Color.red;

            Vector2 boxBottomLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxBottomRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxTopLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);
            Vector2 boxTopRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);

            Debug.DrawLine(boxBottomLeft, boxBottomRight, rayColor);
            Debug.DrawLine(boxBottomRight, boxTopRight, rayColor);
            Debug.DrawLine(boxTopRight, boxTopLeft, rayColor);
            Debug.DrawLine(boxTopLeft, boxBottomLeft, rayColor);
        }
        #endregion
    }

    public void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
        IsTouchingWall();
    }

    #endregion
}