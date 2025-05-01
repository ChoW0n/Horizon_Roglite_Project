using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor;

#region 카메라 트리거 동작 처리
public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspecterObjects customInspecterObjects;

    private Collider2D coll;

    private void Start()
    {
        coll = GetComponent<Collider2D>();
    }

    // 플레이어가 트리거에 진입할 때 호출
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (customInspecterObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(
                    customInspecterObjects.panDistance,
                    customInspecterObjects.panTime,
                    customInspecterObjects.panDirection,
                    false
                );
            }
        }
    }

    // 플레이어가 트리거에서 나갈 때 호출
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector2 exitDirection = (collision.transform.position - coll.bounds.center).normalized;

            if (customInspecterObjects.swapCameras &&
                customInspecterObjects.cameraOnLeft != null &&
                customInspecterObjects.cameraOnRight != null)
            {
                CameraManager.instance.SwapCamera(
                    customInspecterObjects.cameraOnLeft,
                    customInspecterObjects.cameraOnRight,
                    exitDirection
                );
            }

            if (customInspecterObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(
                    customInspecterObjects.panDistance,
                    customInspecterObjects.panTime,
                    customInspecterObjects.panDirection,
                    true
                );
            }
        }
    }
}
#endregion

#region 커스텀 인스펙터용 설정 클래스
[System.Serializable]
public class CustomInspecterObjects
{
    public bool swapCameras = false;
    public bool panCameraOnContact = false;

    [HideInInspector] public CinemachineVirtualCamera cameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera cameraOnRight;

    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance = 3f;
    [HideInInspector] public float panTime = 0.35f;
}
#endregion

#region 카메라 팬 방향 Enum
public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}
#endregion

#region 커스텀 인스펙터 구현
[CustomEditor(typeof(CameraControlTrigger))]
public class MyScriptEditor : Editor
{
    CameraControlTrigger cameraControlTrigger;

    private void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 렌더링
        DrawDefaultInspector();

        // 카메라 스왑 UI 표시
        if (cameraControlTrigger.customInspecterObjects.swapCameras)
        {
            cameraControlTrigger.customInspecterObjects.cameraOnLeft = EditorGUILayout.ObjectField(
                "왼쪽 카메라",
                cameraControlTrigger.customInspecterObjects.cameraOnLeft,
                typeof(CinemachineVirtualCamera), true
            ) as CinemachineVirtualCamera;

            cameraControlTrigger.customInspecterObjects.cameraOnRight = EditorGUILayout.ObjectField(
                "오른쪽 카메라",
                cameraControlTrigger.customInspecterObjects.cameraOnRight,
                typeof(CinemachineVirtualCamera), true
            ) as CinemachineVirtualCamera;
        }

        // 카메라 팬 설정 UI 표시
        if (cameraControlTrigger.customInspecterObjects.panCameraOnContact)
        {
            cameraControlTrigger.customInspecterObjects.panDirection = (PanDirection)EditorGUILayout.EnumPopup(
                "카메라 팬 방향",
                cameraControlTrigger.customInspecterObjects.panDirection
            );

            cameraControlTrigger.customInspecterObjects.panDistance = EditorGUILayout.FloatField(
                "팬 거리",
                cameraControlTrigger.customInspecterObjects.panDistance
            );

            cameraControlTrigger.customInspecterObjects.panTime = EditorGUILayout.FloatField(
                "팬 시간",
                cameraControlTrigger.customInspecterObjects.panTime
            );
        }

        // 변경 사항 저장
        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
#endregion
