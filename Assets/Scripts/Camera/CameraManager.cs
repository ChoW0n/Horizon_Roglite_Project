using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    #region References
    public static CameraManager instance;

    [Header("�ó׸ӽ� ���� ī�޶� ���")]
    [SerializeField] private CinemachineVirtualCamera[] allVirticalCameras;

    [Header("�÷��̾� ���� �� ī�޶� ��")]
    [SerializeField] private float fallPanAmount = 0.25f;
    [SerializeField] private float fallYPanTime = 0.35f;

    [Header("���� �ӵ� ����")]
    public float fallSpeedYDampingChangeThreshold = -15f;

    public bool isLerpingYDamping { get; private set; }
    public bool lerpedFromPlayerFalling { get; set; }

    private Coroutine lerpYPanCoroutine;
    private Coroutine panCameraCoroutine;

    private CinemachineVirtualCamera currentCamera;
    private CinemachineFramingTransposer framingTransposer;

    private float normalYPanAmount;
    private Vector2 startingTrackedObjOffset;

    #endregion

    #region �ʱ�ȭ
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        // ���� Ȱ��ȭ�� ī�޶�� �ش� ī�޶��� �����̹� Ʈ�������� ������Ʈ�� ĳ��
        for (int i = 0; i < allVirticalCameras.Length; i++)
        {
            if (allVirticalCameras[i].enabled)
            {
                currentCamera = allVirticalCameras[i];
                framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        normalYPanAmount = framingTransposer.m_YDamping;
        startingTrackedObjOffset = framingTransposer.m_TrackedObjectOffset;
    }

    #endregion

    #region YDamping ���� ó��
    // Y�� �� �� ���� ���� ����
    public void LerpYDamping(bool isPlayerFalling)
    {
        lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        isLerpingYDamping = true;

        float startDampAmount = framingTransposer.m_YDamping;
        float endDampAmount = isPlayerFalling ? fallPanAmount : normalYPanAmount;

        if (isPlayerFalling)
            lerpedFromPlayerFalling = true;

        float elapsedTime = 0f;

        while (elapsedTime < fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, elapsedTime / fallYPanTime);
            framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        isLerpingYDamping = false;
    }
    #endregion

    #region ī�޶� ��ġ �� ó��
    // Ư�� �������� ī�޶� ��
    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }

    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startingPos = Vector2.zero;

        if (!panToStartingPos)
        {
            switch (panDirection)
            {
                case PanDirection.Up: endPos = Vector2.up; break;
                case PanDirection.Down: endPos = Vector2.down; break;
                case PanDirection.Left: endPos = Vector2.left; break;
                case PanDirection.Right: endPos = Vector2.right; break;
            }

            endPos *= panDistance;
            startingPos = startingTrackedObjOffset;
            endPos += startingPos;
        }
        else
        {
            startingPos = framingTransposer.m_TrackedObjectOffset;
            endPos = startingTrackedObjOffset;
        }

        float elapsedTime = 0f;

        while (elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, elapsedTime / panTime);
            framingTransposer.m_TrackedObjectOffset = panLerp;

            yield return null;
        }
    }
    #endregion

    #region ī�޶� ����
    // Ʈ���� ���⿡ ���� ī�޶� ��ü
    public void SwapCamera(CinemachineVirtualCamera cameraFromLeft, CinemachineVirtualCamera cameraFromRight, Vector2 triggerExitDirection)
    {
        if (currentCamera == cameraFromLeft && triggerExitDirection.x > 0f)
        {
            cameraFromRight.enabled = true;
            cameraFromLeft.enabled = false;
            currentCamera = cameraFromRight;
            framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        else if (currentCamera == cameraFromRight && triggerExitDirection.x < 0f)
        {
            cameraFromLeft.enabled = true;
            cameraFromRight.enabled = false;
            currentCamera = cameraFromLeft;
            framingTransposer = currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }
    #endregion
}
