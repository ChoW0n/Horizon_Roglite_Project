using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region 레퍼런스
    public static PlayerInput PlayerInput;

    // 이동 벡터 및 입력 상태 변수들
    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool DashWaspPressed;
    public static bool LookUpIsHeld;

    // 입력 액션 참조용 변수
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _dashAction;
    private InputAction _lookUpAction;
    #endregion

    #region 초기화
    private void Awake()
    {
        // PlayerInput 컴포넌트 초기화
        PlayerInput = GetComponent<PlayerInput>();

        // Input Action 맵에서 액션 불러오기
        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _dashAction = PlayerInput.actions["Dash"];
        _lookUpAction = PlayerInput.actions["LookUp"];
    }
    #endregion

    #region 입력 업데이트
    private void Update()
    {
        // 이동 입력 (Vector2)
        Movement = _moveAction.ReadValue<Vector2>();

        // 점프 입력 상태 체크
        JumpWasPressed = _jumpAction.WasPressedThisFrame();     // 이번 프레임에 눌림
        JumpIsHeld = _jumpAction.IsPressed();                   // 누르고 있는 중
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();   // 이번 프레임에 뗌

        // 달리기 입력 체크
        RunIsHeld = _runAction.IsPressed();

        // 대쉬 입력 체크
        DashWaspPressed = _dashAction.WasPressedThisFrame();

        // LookUp 입력 처리
        LookUpIsHeld = _lookUpAction.IsPressed();
    }
    #endregion
}
