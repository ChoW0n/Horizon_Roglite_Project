using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region ���۷���
    public static PlayerInput PlayerInput;

    // �̵� ���� �� �Է� ���� ������
    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool DashWaspPressed;
    public static bool LookUpIsHeld;

    // �Է� �׼� ������ ����
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _dashAction;
    private InputAction _lookUpAction;
    #endregion

    #region �ʱ�ȭ
    private void Awake()
    {
        // PlayerInput ������Ʈ �ʱ�ȭ
        PlayerInput = GetComponent<PlayerInput>();

        // Input Action �ʿ��� �׼� �ҷ�����
        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _dashAction = PlayerInput.actions["Dash"];
        _lookUpAction = PlayerInput.actions["LookUp"];
    }
    #endregion

    #region �Է� ������Ʈ
    private void Update()
    {
        // �̵� �Է� (Vector2)
        Movement = _moveAction.ReadValue<Vector2>();

        // ���� �Է� ���� üũ
        JumpWasPressed = _jumpAction.WasPressedThisFrame();     // �̹� �����ӿ� ����
        JumpIsHeld = _jumpAction.IsPressed();                   // ������ �ִ� ��
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();   // �̹� �����ӿ� ��

        // �޸��� �Է� üũ
        RunIsHeld = _runAction.IsPressed();

        // �뽬 �Է� üũ
        DashWaspPressed = _dashAction.WasPressedThisFrame();

        // LookUp �Է� ó��
        LookUpIsHeld = _lookUpAction.IsPressed();
    }
    #endregion
}
