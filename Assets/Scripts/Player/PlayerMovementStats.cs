using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerMovementStats : ScriptableObject
{
    // ---------------- 이동 속도 ----------------
    [Header("이동 속도")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;

    // ---------------- 가속도 ----------------
    [Header("지상 가속도")]
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;

    [Header("공중 가속도")]
    [Range(0.25f, 50f)] public float AirAcceleration = 5f;
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;

    // ---------------- 점프 ----------------
    [Header("점프")]
    public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1, 5)] public int NumberOfJumpsAllowed = 2;

    [Header("점프 중단 / 최고점 유지")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.027f;
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

    [Header("점프 유예 시간")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;

    // ---------------- 충돌 검사 ----------------
    [Header("충돌 검사")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLenght = 0.02f;
    public float HeadDetectionRayLenght = 0.02f;
    [Range(0f, 1f)] public float HeadWidth = 0.75f;

    // ---------------- 점프 시각화 ----------------
    [Header("점프 시각화 도구")]
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    // ---------------- 계산된 점프 수치 ----------------
    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

    // ---------------- 디버그 옵션 ----------------
    [Header("디버그")]
    public bool DebugShowIsGroundedBox;
    public bool DebugShowHeadBumpBox;

    // ---------------- 유니티 이벤트 ----------------
    private void OnValidate()
    {
        RecalculateJumpPhysics();
    }

    private void OnEnable()
    {
        RecalculateJumpPhysics();
    }

    // ---------------- 계산 함수 ----------------
    private void RecalculateJumpPhysics()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }
}
