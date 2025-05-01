using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowOBJ : MonoBehaviour
{
    #region References
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Y축 회전 반전 설정")]
    [SerializeField] private float flipYRotationTime = 0.5f;

    [Header("Look Up 설정")]
    [SerializeField] private float lookUpOffset = 2f;         // 위로 이동할 정도
    [SerializeField] private float followSpeed = 5f;          // 따라가는 속도

    private PlayerController player;
    private bool isFacingRight;

    private Vector3 targetPosition;
    private Vector3 currentOffset = Vector3.zero;

    #endregion

    #region 초기화
    private void Awake()
    {
        // 플레이어 컨트롤러 참조
        player = playerTransform.gameObject.GetComponent<PlayerController>();
        isFacingRight = player._isFacingRight;
    }
    #endregion

    #region 카메라 따라가기
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

    #region Y축 회전 반전
    // 방향 전환 시 호출 (카메라 오브젝트 Y축 회전 반전)
    public void CallTurn()
    {
        LeanTween.rotateY(gameObject, DetermineEndRotation(), flipYRotationTime).setEaseInOutSine();
    }

    // 현재 방향에 따라 회전 각도 결정
    private float DetermineEndRotation()
    {
        isFacingRight = !isFacingRight;

        return isFacingRight ? 150f : 0f;
    }
    #endregion
}
