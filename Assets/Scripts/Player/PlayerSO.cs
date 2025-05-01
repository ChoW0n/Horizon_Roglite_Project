using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerSO : ScriptableObject
{
    // ---------------- 이동 속도 ----------------
    [Header("이동 속도")]
    [Range(0f, 1f)] public float MoveThreshold = 0.25f;                     // 입력 감지 최소값 (이보다 작으면 이동 안 함)
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;                    // 걷기 최대 속도
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;                       // 달리기 최대 속도

    // ---------------- 가속도 ----------------
    [Header("지상 가속도")]
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;               // 지상에서 가속도
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;              // 지상에서 감속도

    [Header("공중 가속도")]
    [Range(0.25f, 50f)] public float AirAcceleration = 5f;                  // 공중에서 가속도
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;                  // 공중에서 감속도

    [Header("벽 점프 가속도")]
    [Range(0.25f, 50f)] public float WallJumpMoveAcceleration = 5f;         // 벽 점프 후 가속도
    [Range(0.25f, 50f)] public float WallJumpMoveDeceleration = 5f;         // 벽 점프 후 감속도

    // ---------------- 점프 ----------------
    [Header("점프")]
    public float JumpHeight = 6.5f;                                         // 점프 높이
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;   // 점프 보정 계수
    public float TimeTillJumpApex = 0.35f;                                  // 최고점 도달까지 시간
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;        // 점프 버튼에서 손 뗐을 때 중력 가중치
    public float MaxFallSpeed = 26f;                                        // 최대 낙하 속도 제한
    [Range(1, 5)] public int NumberOfJumpsAllowed = 2;                      // 허용되는 점프 횟수 (이중 점프 포함)

    [Header("점프 중단 / 최고점 유지")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.027f;        // 점프 후 위쪽 이동 중 중단 가능한 시간
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;                   // 최고점 판정 기준
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;                  // 최고점에서 잠깐 멈추는 시간

    [Header("점프 유예 시간")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;                   // 점프 입력 유예 시간
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;                     // 착지 후 잠깐 점프 가능한 시간 (코요테 타임)

    // ---------------- 충돌 검사 ----------------
    [Header("충돌 검사")]
    public LayerMask GroundLayer;                                           // 지면 레이어
    public float GroundDetectionRayLenght = 0.02f;                          // 지면 감지 레이 길이
    public float HeadDetectionRayLenght = 0.02f;                            // 머리 감지 레이 길이
    public float WallDetectionRayLenght = 0.125f;                           // 벽 감지 레이 길이
    [Range(0f, 1f)] public float HeadWidth = 0.75f;                         // 머리 감지 영역 너비
    [Range(0f, 1f)] public float WallDetectionRayHeightMultiplier = 0.9f;   // 벽 감지 레이 높이 보정값

    [Header("점프 초기화 설정")]
    public bool ResetJumpsOnWallSlide = true;                               // 벽 슬라이드 시 점프 횟수 리셋 여부

    // ---------------- 슬라이드 검사 ----------------
    [Header("벽 슬라이드 설정")]
    [Min(0.01f)] public float WallSlideSpeed = 5f;                          // 벽 슬라이드 최대 속도
    [Range(0.25f, 50f)] public float WallSlideDecelerationSpeed = 50f;      // 슬라이드 감속도

    [Header("벽 점프 설정")]
    public Vector2 WallJumpDirection = new Vector2(-20f, 6.5f);             // 벽 점프 방향 및 힘
    [Range(0f, 1f)] public float WallJumpPostBufferTime = 0.125f;           // 벽 점프 후 조작 유예 시간
    [Range(0.0f, 5f)] public float WallJumpGravityOnReleaseMultiplier = 1f; // 벽 점프 중 점프 키 뗐을 때 중력 배수

    [Header("대쉬 설정")]
    [Range(0f, 1f)] public float DashTime = 0.11f;                          // 대쉬 지속 시간
    [Range(1f, 200f)] public float DashSpeed = 40f;                         // 대쉬 속도
    [Range(0f, 1f)] public float TimeBtwDashesOnGround = 0.225f;            // 지상에서 대쉬 후 다시 대쉬 가능까지 시간
    public bool ResetDashOnWallSlide = true;                                // 벽 슬라이드 시 대쉬 리셋 여부
    [Range(0, 5)] public int NumberOfDashes = 2;                            // 대쉬 가능한 횟수
    [Range(0f, 0.5f)] public float DashDiagonallyBias = 0.4f;               // 대쉬 입력 방향 비율 (대각선 가중치)

    [Header("대쉬 캔슬 관련 설정")]
    [Range(0.0f, 5f)] public float DashGravityOnReleaseMultiplier = 1f;     // 대쉬 중 키를 떼었을 때 중력 배수
    [Range(0.02f, 0.3f)] public float DashTimeForUpwardsCancel = 0.027f;    // 대쉬 중 상향 이동 취소 타이밍

    // ---------------- 점프 시각화 ----------------
    [Header("점프 시각화 도구")]
    public bool ShowWalkJumpArc = false;                                    // 걷기 점프 궤적 표시
    public bool ShowRunJumpArc = false;                                     // 달리기 점프 궤적 표시
    public bool StopOnCollision = true;                                     // 충돌 시 시각화 멈춤
    public bool DrawRight = true;                                           // 오른쪽 방향으로만 시각화
    [Range(5, 100)] public int ArcResolution = 20;                          // 궤적 곡선 정밀도
    [Range(0, 500)] public int VisualizationSteps = 90;                     // 궤적 시뮬레이션 단계 수

    // ---------------- 대쉬 방향 ----------------
    public readonly Vector2[] DashDirections = new Vector2[]
    {
        new Vector2(0, 0),                      // 입력 없음
        new Vector2(1, 0),                      // 오른쪽
        new Vector2(1, 1).normalized,           // 우 상단
        new Vector2(0, 1),                      // 위
        new Vector2(-1, 1).normalized,          // 좌 상단
        new Vector2(-1, 0),                     // 왼쪽
        new Vector2(-1, -1).normalized,         // 좌 하단
        new Vector2(0, -1),                     // 아래
        new Vector2(1, -1).normalized           // 우 하단
    };

    // ---------------- 디버그 옵션 ----------------
    [Header("콜라이더 시각화")]
    public bool DebugShowIsGroundedBox;         // 땅에 붙었는지 확인용 디버그 박스 표시
    public bool DebugShowHeadBumpBox;           // 머리 충돌 확인용 박스 표시
    public bool DebugShowWallHitbox;            // 벽 감지 디버그 박스 표시

    #region Calculated Values
    public float Gravity { get; private set; }                      // 계산된 중력 값
    public float InitialJumpVelocity { get; private set; }          // 초기 점프 속도
    public float AdjustedJumpHeight { get; private set; }           // 보정된 점프 높이

    public float WallJumpGravity { get; private set; }              // 벽 점프 중력
    public float InitialWallJumpVelocity { get; private set; }      // 벽 점프 초기 속도
    public float AdjustedWallJumpHeight { get; private set; }       // 보정된 벽 점프 높이
    #endregion

    #region Unity Lifecycle
    private void OnValidate()
    {
        // 에디터에서 값 변경 시 점프 물리 다시 계산
        RecalculateJumpPhysics(); 
    }

    private void OnEnable()
    {
        // 활성화 시 점프 물리 다시 계산
        RecalculateJumpPhysics(); 
    }
    #endregion

    // ---------------- 점프 물리 계산 ----------------
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
