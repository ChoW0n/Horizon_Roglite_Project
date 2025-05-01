using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region References
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;

    private Rigidbody2D _rb;

    [Header("Movement Vars")]
    private Vector2 _moveVelocity;
    public bool _isFacingRight;

    [Header("Collision Check Vars")]
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    [Header("Camera")]
    public GameObject _cameraFollowGO;
    private CameraFollowOBJ _cameraFollowOBJ;
    private float _fallSpeedYDampingChangeThreshold;

    // Jump Vars
    public float VerticalVelocity {  get; private set; }
    [Header("Jump Vars")]
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    [Header("Apex Vars")]
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    [Header("Jump Buffer Vars")]
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    [Header("Coyote time Vars")]
    private float _coyoteTimer;

    #endregion

    #region Unity CallBack Functions

    private void Start()
    {
        _isFacingRight = true;

        _rb = GetComponent<Rigidbody2D>();
        _cameraFollowOBJ = _cameraFollowGO.GetComponent<CameraFollowOBJ>();

        _fallSpeedYDampingChangeThreshold = CameraManager.instance.fallSpeedYDampingChangeThreshold;
    }

    private void Update()
    {
        JumpChecks();
        CountTimers();

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
        Jump();

        // ����/���� ���ӵ��� ���� �̵� ó��
        if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }
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
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            }

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration + Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
        }
        else if (moveInput == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration + Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
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

    #region Jump
    /// <summary>
    /// ���� �Է� ó�� �� ���� ���� ���� ����
    /// </summary>
    private void JumpChecks()
    {
        if (InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        if (InputManager.JumpWasReleased)
        {
            if (_jumpBufferTimer > 0f)
                _jumpReleasedDuringBuffer = true;

            // ���� ���� ���� Ű���� ���� ���� ������ ����
            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        // ���� ���� + �ڿ��� Ÿ�� ����
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        // ���� ���� (�ٴ�����)
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }
        // ���� �� �߰� ���� (�ٴ����� ����)
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }

        // ���� �� ���� �ʱ�ȭ
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            VerticalVelocity = Physics2D.gravity.y;
            _numberOfJumpsUsed = 0;
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
            _isJumping = true;

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
    }

    /// <summary>
    /// ���� ���¿����� ���� �ӵ� �� ���� ���� ó��
    /// </summary>
    private void Jump()
    {
        if (_isJumping)
        {
            if (_bumpedHead)
                _isFastFalling = true;

            if (VerticalVelocity >= 0f)
            {
                // ���� �߰� ���� ���� �Ǵ�
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < MoveStats.ApexHangTime)
                            VerticalVelocity = 0f;
                        else
                            VerticalVelocity = -0.01f;
                    }
                }
                else
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                        _isPastApexThreshold = false;
                }
            }
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                    _isFalling = true;
            }
        }

        // ���� ���� ó��
        if (_isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            else if (_fastFallTime < MoveStats.TimeForUpwardsCancel)
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MoveStats.TimeForUpwardsCancel));

            _fastFallTime += Time.fixedDeltaTime;
        }

        // ������ �ƴ� �ܼ� ����
        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
                _isFalling = true;

            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);

        _rb.velocity = new Vector2(_rb.velocity.x, VerticalVelocity);
    }

    #endregion

    #region Collision Checks
    /// <summary>
    /// �ٴ� ���� (BoxCast)
    /// </summary>
    private void IsGrounded()
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
    private void BumpedHead()
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
    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
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

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }

    #endregion

    #region Timers
    /// <summary>
    /// ���� ���� �� �ڿ��� Ÿ�̸� ����
    /// </summary>
    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
            _coyoteTimer -= Time.deltaTime;
        else
            _coyoteTimer = MoveStats.JumpCoyoteTime;
    }

    #endregion
}
