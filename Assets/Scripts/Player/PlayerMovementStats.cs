using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerMovementStats : ScriptableObject
{
    // ---------------- �̵� �ӵ� ----------------
    [Header("�̵� �ӵ�")]
    [Range(0f, 1f)] public float MoveThreshold = 0.25f;
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;

    // ---------------- ���ӵ� ----------------
    [Header("���� ���ӵ�")]
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;

    [Header("���� ���ӵ�")]
    [Range(0.25f, 50f)] public float AirAcceleration = 5f;
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;

    [Header("�� ���� ���ӵ�")]
    [Range(0.25f, 50f)] public float WallJumpMoveAcceleration = 5f;
    [Range(0.25f, 50f)] public float WallJumpMoveDeceleration = 5f;

    // ---------------- ���� ----------------
    [Header("����")]
    public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1, 5)] public int NumberOfJumpsAllowed = 2;

    [Header("���� �ߴ� / �ְ��� ����")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.027f;
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

    [Header("���� ���� �ð�")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;

    // ---------------- �浹 �˻� ----------------
    [Header("�浹 �˻�")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLenght = 0.02f;
    public float HeadDetectionRayLenght = 0.02f;
    public float WallDetectionRayLenght = 0.125f;
    [Range(0f, 1f)] public float HeadWidth = 0.75f;
    [Range(0f, 1f)] public float WallDetectionRayHeightMultiplier = 0.9f;

    [Header("���� �ʱ�ȭ ����")]
    public bool ResetJumpsOnWallSlide = true;

    // ---------------- �����̴� �˻� ----------------
    [Header("�� �����̵� ����")]
    [Min(0.01f)] public float WallSlideSpeed = 5f;
    [Range(0.25f, 50f)] public float WallSlideDecelerationSpeed = 50f;

    [Header("�� ���� ����")]
    public Vector2 WallJumpDirection = new Vector2(-20f, 6.5f);
    [Range(0f, 1f)] public float WallJumpPostBufferTime = 0.125f;
    [Range(0.0f, 5f)] public float WallJumpGravityOnReleaseMultiplier = 1f;

    [Header("�뽬 ����")]
    [Range(0f, 1f)] public float DashTime = 0.11f;
    [Range(1f, 200f)] public float DashSpeed = 40f;
    [Range(0f, 1f)] public float TimeBtwDashesOnGround = 0.225f;
    public bool ResetDashOnWallSlide = true;
    [Range(0, 5)] public int NumberOfDashes = 2;
    [Range(0f, 0.5f)] public float DashDiagonallyBias = 0.4f;

    [Header("�뽬 ĵ�� ���� ����")]
    [Range(0.0f, 5f)] public float DashGravityOnReleaseMultiplier = 1f;
    [Range(0.02f, 0.3f)] public float DashTimeForUpwardsCancel = 0.027f;

    // ---------------- ���� �ð�ȭ ----------------
    [Header("���� �ð�ȭ ����")]
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    public readonly Vector2[] DashDirections = new Vector2[]
    {
        new Vector2(0, 0),                      // �ƹ��͵� �ƴ�
        new Vector2(1, 0),                      // ������
        new Vector2(1, 1).normalized,           // �� ���
        new Vector2(0, 1),                      // ��
        new Vector2(-1, 1).normalized,          // �� ���
        new Vector2(-1, 0),                     // ����
        new Vector2(-1, -1).normalized,         // �� �ϴ�
        new Vector2(0, -1),                     // �Ʒ�
        new Vector2(1, -1).normalized           // �� �ϴ�
    };

    // ---------------- ����� �ɼ� ----------------
    [Header("�����")]
    public bool DebugShowIsGroundedBox;
    public bool DebugShowHeadBumpBox;
    public bool DebugShowWallHitbox;

    #region Calculated Values

    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

    public float WallJumpGravity { get; private set; }
    public float InitialWallJumpVelocity { get; private set; }
    public float AdjustedWallJumpHeight { get; private set; }

    #endregion

    #region Unity Lifecycle

    private void OnValidate()
    {
        RecalculateJumpPhysics();
    }

    private void OnEnable()
    {
        RecalculateJumpPhysics();
    }

    #endregion

    // ---------------- ��� �Լ� ----------------
    private void RecalculateJumpPhysics()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;

        AdjustedWallJumpHeight = WallJumpDirection.y * JumpHeightCompensationFactor;
        WallJumpGravity = -(2f * AdjustedWallJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialWallJumpVelocity = Mathf.Abs(WallJumpGravity) * TimeTillJumpApex;
    }
}
