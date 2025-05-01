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

    private PlayerController player;
    private bool isFacingRight;

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
        // 플레이어 위치를 지속적으로 따라감
        transform.position = playerTransform.position;
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
