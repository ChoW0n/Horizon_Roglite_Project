using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor;

#region ī�޶� Ʈ���� ���� ó��
public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspecterObjects customInspecterObjects;

    private Collider2D coll;

    private void Start()
    {
        coll = GetComponent<Collider2D>();
    }

    // �÷��̾ Ʈ���ſ� ������ �� ȣ��
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

    // �÷��̾ Ʈ���ſ��� ���� �� ȣ��
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

#region Ŀ���� �ν����Ϳ� ���� Ŭ����
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

#region ī�޶� �� ���� Enum
public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}
#endregion

#region Ŀ���� �ν����� ����
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
        // �⺻ �ν����� ������
        DrawDefaultInspector();

        // ī�޶� ���� UI ǥ��
        if (cameraControlTrigger.customInspecterObjects.swapCameras)
        {
            cameraControlTrigger.customInspecterObjects.cameraOnLeft = EditorGUILayout.ObjectField(
                "���� ī�޶�",
                cameraControlTrigger.customInspecterObjects.cameraOnLeft,
                typeof(CinemachineVirtualCamera), true
            ) as CinemachineVirtualCamera;

            cameraControlTrigger.customInspecterObjects.cameraOnRight = EditorGUILayout.ObjectField(
                "������ ī�޶�",
                cameraControlTrigger.customInspecterObjects.cameraOnRight,
                typeof(CinemachineVirtualCamera), true
            ) as CinemachineVirtualCamera;
        }

        // ī�޶� �� ���� UI ǥ��
        if (cameraControlTrigger.customInspecterObjects.panCameraOnContact)
        {
            cameraControlTrigger.customInspecterObjects.panDirection = (PanDirection)EditorGUILayout.EnumPopup(
                "ī�޶� �� ����",
                cameraControlTrigger.customInspecterObjects.panDirection
            );

            cameraControlTrigger.customInspecterObjects.panDistance = EditorGUILayout.FloatField(
                "�� �Ÿ�",
                cameraControlTrigger.customInspecterObjects.panDistance
            );

            cameraControlTrigger.customInspecterObjects.panTime = EditorGUILayout.FloatField(
                "�� �ð�",
                cameraControlTrigger.customInspecterObjects.panTime
            );
        }

        // ���� ���� ����
        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
#endregion
