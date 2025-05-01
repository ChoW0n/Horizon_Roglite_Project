using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerSO : ScriptableObject
{
    // ---------------- �̵� �ӵ� ----------------
    [Header("�̵� �ӵ�")]
    [Range(0f, 1f)] public float MoveThreshold = 0.25f;                     // �Է� ���� �ּҰ� (�̺��� ������ �̵� �� ��)
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;                    // �ȱ� �ִ� �ӵ�
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;                       // �޸��� �ִ� �ӵ�

    // ---------------- ���ӵ� ----------------
    [Header("���� ���ӵ�")]
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;               // ���󿡼� ���ӵ�
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;              // ���󿡼� ���ӵ�

    [Header("���� ���ӵ�")]
    [Range(0.25f, 50f)] public float AirAcceleration = 5f;                  // ���߿��� ���ӵ�
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;                  // ���߿��� ���ӵ�

    [Header("�� ���� ���ӵ�")]
    [Range(0.25f, 50f)] public float WallJumpMoveAcceleration = 5f;         // �� ���� �� ���ӵ�
    [Range(0.25f, 50f)] public float WallJumpMoveDeceleration = 5f;         // �� ���� �� ���ӵ�

    // ---------------- ���� ----------------
    [Header("����")]
    public float JumpHeight = 6.5f;                                         // ���� ����
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;   // ���� ���� ���
    public float TimeTillJumpApex = 0.35f;                                  // �ְ��� ���ޱ��� �ð�
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;        // ���� ��ư���� �� ���� �� �߷� ����ġ
    public float MaxFallSpeed = 26f;                                        // �ִ� ���� �ӵ� ����
    [Range(1, 5)] public int NumberOfJumpsAllowed = 2;                      // ���Ǵ� ���� Ƚ�� (���� ���� ����)

    [Header("���� �ߴ� / �ְ��� ����")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.027f;        // ���� �� ���� �̵� �� �ߴ� ������ �ð�
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;                   // �ְ��� ���� ����
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;                  // �ְ������� ��� ���ߴ� �ð�

    [Header("���� ���� �ð�")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;                   // ���� �Է� ���� �ð�
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;                     // ���� �� ��� ���� ������ �ð� (�ڿ��� Ÿ��)

    // ---------------- �浹 �˻� ----------------
    [Header("�浹 �˻�")]
    public LayerMask GroundLayer;                                           // ���� ���̾�
    public float GroundDetectionRayLenght = 0.02f;                          // ���� ���� ���� ����
    public float HeadDetectionRayLenght = 0.02f;                            // �Ӹ� ���� ���� ����
    public float WallDetectionRayLenght = 0.125f;                           // �� ���� ���� ����
    [Range(0f, 1f)] public float HeadWidth = 0.75f;                         // �Ӹ� ���� ���� �ʺ�
    [Range(0f, 1f)] public float WallDetectionRayHeightMultiplier = 0.9f;   // �� ���� ���� ���� ������

    [Header("���� �ʱ�ȭ ����")]
    public bool ResetJumpsOnWallSlide = true;                               // �� �����̵� �� ���� Ƚ�� ���� ����

    // ---------------- �����̵� �˻� ----------------
    [Header("�� �����̵� ����")]
    [Min(0.01f)] public float WallSlideSpeed = 5f;                          // �� �����̵� �ִ� �ӵ�
    [Range(0.25f, 50f)] public float WallSlideDecelerationSpeed = 50f;      // �����̵� ���ӵ�

    [Header("�� ���� ����")]
    public Vector2 WallJumpDirection = new Vector2(-20f, 6.5f);             // �� ���� ���� �� ��
    [Range(0f, 1f)] public float WallJumpPostBufferTime = 0.125f;           // �� ���� �� ���� ���� �ð�
    [Range(0.0f, 5f)] public float WallJumpGravityOnReleaseMultiplier = 1f; // �� ���� �� ���� Ű ���� �� �߷� ���

    [Header("�뽬 ����")]
    [Range(0f, 1f)] public float DashTime = 0.11f;                          // �뽬 ���� �ð�
    [Range(1f, 200f)] public float DashSpeed = 40f;                         // �뽬 �ӵ�
    [Range(0f, 1f)] public float TimeBtwDashesOnGround = 0.225f;            // ���󿡼� �뽬 �� �ٽ� �뽬 ���ɱ��� �ð�
    public bool ResetDashOnWallSlide = true;                                // �� �����̵� �� �뽬 ���� ����
    [Range(0, 5)] public int NumberOfDashes = 2;                            // �뽬 ������ Ƚ��
    [Range(0f, 0.5f)] public float DashDiagonallyBias = 0.4f;               // �뽬 �Է� ���� ���� (�밢�� ����ġ)

    [Header("�뽬 ĵ�� ���� ����")]
    [Range(0.0f, 5f)] public float DashGravityOnReleaseMultiplier = 1f;     // �뽬 �� Ű�� ������ �� �߷� ���
    [Range(0.02f, 0.3f)] public float DashTimeForUpwardsCancel = 0.027f;    // �뽬 �� ���� �̵� ��� Ÿ�̹�

    // ---------------- ���� �ð�ȭ ----------------
    [Header("���� �ð�ȭ ����")]
    public bool ShowWalkJumpArc = false;                                    // �ȱ� ���� ���� ǥ��
    public bool ShowRunJumpArc = false;                                     // �޸��� ���� ���� ǥ��
    public bool StopOnCollision = true;                                     // �浹 �� �ð�ȭ ����
    public bool DrawRight = true;                                           // ������ �������θ� �ð�ȭ
    [Range(5, 100)] public int ArcResolution = 20;                          // ���� � ���е�
    [Range(0, 500)] public int VisualizationSteps = 90;                     // ���� �ùķ��̼� �ܰ� ��

    // ---------------- �뽬 ���� ----------------
    public readonly Vector2[] DashDirections = new Vector2[]
    {
        new Vector2(0, 0),                      // �Է� ����
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
    [Header("�ݶ��̴� �ð�ȭ")]
    public bool DebugShowIsGroundedBox;         // ���� �پ����� Ȯ�ο� ����� �ڽ� ǥ��
    public bool DebugShowHeadBumpBox;           // �Ӹ� �浹 Ȯ�ο� �ڽ� ǥ��
    public bool DebugShowWallHitbox;            // �� ���� ����� �ڽ� ǥ��

    #region Calculated Values
    public float Gravity { get; private set; }                      // ���� �߷� ��
    public float InitialJumpVelocity { get; private set; }          // �ʱ� ���� �ӵ�
    public float AdjustedJumpHeight { get; private set; }           // ������ ���� ����

    public float WallJumpGravity { get; private set; }              // �� ���� �߷�
    public float InitialWallJumpVelocity { get; private set; }      // �� ���� �ʱ� �ӵ�
    public float AdjustedWallJumpHeight { get; private set; }       // ������ �� ���� ����
    #endregion

    #region Unity Lifecycle
    private void OnValidate()
    {
        // �����Ϳ��� �� ���� �� ���� ���� �ٽ� ���
        RecalculateJumpPhysics(); 
    }

    private void OnEnable()
    {
        // Ȱ��ȭ �� ���� ���� �ٽ� ���
        RecalculateJumpPhysics(); 
    }
    #endregion

    // ---------------- ���� ���� ��� ----------------
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
