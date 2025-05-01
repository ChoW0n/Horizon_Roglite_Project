using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowOBJ : MonoBehaviour
{
    #region References
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Y�� ȸ�� ���� ����")]
    [SerializeField] private float flipYRotationTime = 0.5f;

    [Header("Look Up ����")]
    [SerializeField] private float lookUpOffset = 2f;         // ���� �̵��� ����
    [SerializeField] private float followSpeed = 5f;          // ���󰡴� �ӵ�

    private PlayerController player;
    private bool isFacingRight;

    private Vector3 targetPosition;
    private Vector3 currentOffset = Vector3.zero;

    #endregion

    #region �ʱ�ȭ
    private void Awake()
    {
        // �÷��̾� ��Ʈ�ѷ� ����
        player = playerTransform.gameObject.GetComponent<PlayerController>();
        isFacingRight = player._isFacingRight;
    }
    #endregion

    #region ī�޶� ���󰡱�
    private void Update()
    {
        if (InputManager.Movement.x == 0 && InputManager.LookUpIsHeld)
        {
            currentOffset = Vector3.Lerp(currentOffset, new Vector3(0, lookUpOffset, 0), Time.deltaTime * followSpeed);
        }
        else
        {
            currentOffset = Vector3.Lerp(currentOffset, Vector3.zero, Time.deltaTime * followSpeed);
        }

        targetPosition = playerTransform.position + currentOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
    }
    #endregion

    #region Y�� ȸ�� ����
    // ���� ��ȯ �� ȣ�� (ī�޶� ������Ʈ Y�� ȸ�� ����)
    public void CallTurn()
    {
        LeanTween.rotateY(gameObject, DetermineEndRotation(), flipYRotationTime).setEaseInOutSine();
    }

    // ���� ���⿡ ���� ȸ�� ���� ����
    private float DetermineEndRotation()
    {
        isFacingRight = !isFacingRight;

        return isFacingRight ? 150f : 0f;
    }
    #endregion
}
