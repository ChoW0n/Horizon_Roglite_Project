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

    private PlayerController player;
    private bool isFacingRight;

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
        // �÷��̾� ��ġ�� ���������� ����
        transform.position = playerTransform.position;
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
