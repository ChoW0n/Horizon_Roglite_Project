using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor;

public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspecterObjects customInspecterObjects;

    private Collider2D coll;

    private void Start()
    {
        coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (customInspecterObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspecterObjects.panDistance, 
                    customInspecterObjects.panTime, 
                    customInspecterObjects.panDirection, 
                    false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector2 exitDirection = (collision.transform.position - coll.bounds.center).normalized;

            if (customInspecterObjects.swapCameras && customInspecterObjects.cameraOnLeft != null && customInspecterObjects.cameraOnRight != null)
            {
                CameraManager.instance.SwapCamera(customInspecterObjects.cameraOnLeft, 
                    customInspecterObjects.cameraOnRight, 
                    exitDirection);
            }

            if (customInspecterObjects.panCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(customInspecterObjects.panDistance, 
                    customInspecterObjects.panTime, 
                    customInspecterObjects.panDirection, 
                    true);
            }
        }
    }
}

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

public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}

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
        DrawDefaultInspector();

        if (cameraControlTrigger.customInspecterObjects.swapCameras)
        {
            cameraControlTrigger.customInspecterObjects.cameraOnLeft = EditorGUILayout.ObjectField("Camera on Left", cameraControlTrigger.customInspecterObjects.cameraOnLeft,
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;

            cameraControlTrigger.customInspecterObjects.cameraOnRight = EditorGUILayout.ObjectField("Camera on Right", cameraControlTrigger.customInspecterObjects.cameraOnRight,
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
        }

        if (cameraControlTrigger.customInspecterObjects.panCameraOnContact)
        {
            cameraControlTrigger.customInspecterObjects.panDirection = (PanDirection)EditorGUILayout.EnumPopup("Camera Pan Direction",
                cameraControlTrigger.customInspecterObjects.panDirection);

            cameraControlTrigger.customInspecterObjects.panDistance = EditorGUILayout.FloatField("Pan Distance", cameraControlTrigger.customInspecterObjects.panDistance);
            cameraControlTrigger.customInspecterObjects.panTime = EditorGUILayout.FloatField("Pan Time", cameraControlTrigger.customInspecterObjects.panTime);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
