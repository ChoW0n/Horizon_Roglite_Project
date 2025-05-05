using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region References
    [Header("References")]
    public PlayerSO PlayerSO;
    public Animator _animator;
    private PlayerWallSlide WallSlide;
    private PlayerWallJump WallJump;
    private PlayerDash Dash;
    private PlayerJump Jump;

    public Collider2D _feetColl;
    public Collider2D _bodyColl;

    private Rigidbody2D _rb;

    [Header("Movement Vars")]
    [HideInInspector] public float HorizontalVelocity;
    public bool _isFacingRight;

    [Header("Camera")]
    public GameObject _cameraFollowGO;
    private CameraFollowOBJ _cameraFollowOBJ;
    private float _fallSpeedYDampingChangeThreshold;

    [Header("Coyote time Vars")]
    [HideInInspector] public float _coyoteTimer;

    [Header("Collision Check Vars")]
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private RaycastHit2D _wallHit;
    [HideInInspector] public RaycastHit2D _lastWallHit;
    [HideInInspector] public bool _isGrounded;
    [HideInInspector] public bool _bumpedHead;
    [HideInInspector] public bool _isTouchingWall;

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

        // 카메라 Y Damping 조정 (낙하/점프 시 자연스러운 추적을 위한 보간)
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

        // 지면/공중 가속도에 따라 이동 처리
        if (_isGrounded)
        {
            Move(PlayerSO.GroundAcceleration, PlayerSO.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            if (WallJump._useWallJumpMoveState)
                Move(PlayerSO.WallJumpMoveAcceleration, PlayerSO.WallJumpMoveDeceleration, InputManager.Movement);
            else
                Move(PlayerSO.AirAcceleration, PlayerSO.AirDeceleration, InputManager.Movement);
        }

        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        if (!Dash._isDashing)
            Jump.VerticalVelocity = Mathf.Clamp(Jump.VerticalVelocity, -PlayerSO.MaxFallSpeed, 50f);
        else
            Jump.VerticalVelocity = Mathf.Clamp(Jump.VerticalVelocity, -50f, 50f);

        _rb.velocity = new Vector2(HorizontalVelocity, Jump.VerticalVelocity);
    }

    private void OnDrawGizmos()
    {
        PlayerSO.DrawRight = _isFacingRight;

        if (PlayerSO.ShowWalkJumpArc)
        {
            DrawJumpArc(PlayerSO.MaxWalkSpeed, Color.white);
        }
        else if (PlayerSO.ShowRunJumpArc)
        {
            DrawJumpArc(PlayerSO.MaxRunSpeed, Color.red);
        }
    }

    #endregion

    #region Movement
    /// <summary>
    /// 플레이어의 이동 처리 (걷기/달리기)
    /// </summary>
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (!Dash._isDashing)
        {
            if (Mathf.Abs(moveInput.x) >= PlayerSO.MoveThreshold)
            {
                TurnCheck(moveInput);

                float targetVelocity = 0f;
                if (InputManager.RunIsHeld)
                {
                    targetVelocity = moveInput.x * PlayerSO.MaxRunSpeed;
                    _animator.SetBool("IsRunning", true);
                    _animator.SetBool("IsWalking", false);
                }
                else
                {
                    targetVelocity = moveInput.x * PlayerSO.MaxWalkSpeed;
                    _animator.SetBool("IsRunning", false);
                    _animator.SetBool("IsWalking", true);
                }

                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, acceleration + Time.fixedDeltaTime);
            }
            else if (Mathf.Abs(moveInput.x) < PlayerSO.MoveThreshold)
            {
                _animator.SetBool("IsRunning", false);
                _animator.SetBool("IsWalking", false);
                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, 0f, deceleration + Time.fixedDeltaTime);
            }
        }
    }

    /// <summary>
    /// 좌우 반전 처리
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
    /// 방향 전환 + 카메라 대상 반전
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
    /// 점프 버퍼 및 코요테 타이머 관리
    /// </summary>
    private void CountTimers()
    {
        Jump._jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
            _coyoteTimer -= Time.deltaTime;
        else
            _coyoteTimer = PlayerSO.JumpCoyoteTime;

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
    /// 바닥 판정 (BoxCast)
    /// </summary>
    public void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, PlayerSO.GroundDetectionRayLenght);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, PlayerSO.GroundDetectionRayLenght, PlayerSO.GroundLayer);
        if (_groundHit.collider != null)
            _isGrounded = true;
        else
            _isGrounded = false;

        #region Debug Visualization

        if (PlayerSO.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if (_isGrounded)
                rayColor = Color.green;
            else
                rayColor = Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * PlayerSO.GroundDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * PlayerSO.GroundDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - PlayerSO.GroundDetectionRayLenght), Vector2.right * boxCastSize.x, rayColor);
        }

        #endregion
    }

    /// <summary>
    /// 머리 충돌 판정 (BoxCast)
    /// </summary>
    public void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * PlayerSO.HeadWidth, PlayerSO.HeadDetectionRayLenght);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, PlayerSO.HeadDetectionRayLenght, PlayerSO.GroundLayer);
        if (_headHit.collider != null)
            _bumpedHead = true;
        else
            _bumpedHead = false;

        #region Debug Visualization

        if (PlayerSO.DebugShowHeadBumpBox)
        {
            float headWidth = PlayerSO.HeadWidth;

            Color rayColor;
            if (_bumpedHead)
                rayColor = Color.green;
            else
                rayColor = Color.red;

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * PlayerSO.HeadDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * PlayerSO.HeadDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + PlayerSO.HeadDetectionRayLenght), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }

        #endregion
    }

    /// <summary>
    /// 점프 아크 시각화
    /// </summary>
    public void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 previousPosition = startPosition;

        float speed = 0f;
        if (PlayerSO.DrawRight)
            speed = moveSpeed;
        else
            speed = -moveSpeed;

        Vector2 velocity = new Vector2(speed, PlayerSO.InitialJumpVelocity);
        Gizmos.color = gizmoColor;
        float timeStep = 2 * PlayerSO.TimeTillJumpApex / PlayerSO.ArcResolution;

        for (int i = 0; i < PlayerSO.VisualizationSteps; i++)
        {
            float simulationtime = i * timeStep;
            Vector2 displacement, drawPoint;

            if (simulationtime < PlayerSO.TimeTillJumpApex)
            {
                displacement = velocity * simulationtime + 0.5f * new Vector2(0, PlayerSO.Gravity) * simulationtime * simulationtime;
            }
            else if (simulationtime < PlayerSO.TimeTillJumpApex + PlayerSO.ApexHangTime)
            {
                float apexTime = simulationtime - PlayerSO.TimeTillJumpApex;
                displacement = velocity * PlayerSO.TimeTillJumpApex + 0.5f * new Vector2(0, PlayerSO.Gravity) * PlayerSO.TimeTillJumpApex * PlayerSO.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime;
            }
            else
            {
                float descendTime = simulationtime - (PlayerSO.TimeTillJumpApex + PlayerSO.ApexHangTime);
                displacement = velocity * PlayerSO.TimeTillJumpApex + 0.5f * new Vector2(0, PlayerSO.Gravity) * PlayerSO.TimeTillJumpApex * PlayerSO.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * PlayerSO.ApexHangTime;
                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0, PlayerSO.Gravity) * descendTime * descendTime;
            }
            drawPoint = startPosition + displacement;

            if (PlayerSO.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint), PlayerSO.GroundLayer);
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
    /// 벽 점프 시각화
    /// </summary>
    public void IsTouchingWall()
    {
        float originEndPoint = 0f;
        if (_isFacingRight)
            originEndPoint = _bodyColl.bounds.max.x;
        else
            originEndPoint = _bodyColl.bounds.min.x;
        float abjustedHeight = _bodyColl.bounds.size.y * PlayerSO.WallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originEndPoint, _bodyColl.bounds.center.y);
        Vector2 boxCastSize = new Vector2(PlayerSO.WallDetectionRayLenght, abjustedHeight);

        _wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, PlayerSO.WallDetectionRayLenght, PlayerSO.GroundLayer);
        if (_wallHit.collider != null)
        {
            _lastWallHit = _wallHit;
            _isTouchingWall = true;
        }
        else
            _isTouchingWall = false;

        #region Debug Visualization

        if (PlayerSO.DebugShowWallHitbox)
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