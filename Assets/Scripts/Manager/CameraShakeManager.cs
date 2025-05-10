using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    #region ���۷���
    public static CameraShakeManager instance;

    [SerializeField] private float globalShakeForce = 1f;                       // �⺻ ī�޶� ��鸲 ����
    [SerializeField] private CinemachineImpulseListener impulseListener;

    private CinemachineImpulseDefinition impulseDefinition;

    #endregion

    #region �ʱ�ȭ
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    #endregion

    #region ī�޶� ��鸲

    /// <summary>
    /// ���� �⺻ ����� ī�޶� ��鸲 �߻�
    /// </summary>
    public void CameraShak(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }

    /// <summary>
    /// ������ ���� ������� ī�޶� ��鸲 �߻�
    /// </summary>
    public void ScreenShakeFromProfile(ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        SetupScreenShakeSettings(profile, impulseSource);               // ������ ���� ����
        impulseSource.GenerateImpulseWithForce(profile.impactForce);    // ������ ����� ��鸲 �߻�
    }

    /// <summary>
    /// ��ũ�� ��鸲�� ����� ���޽� ������ ������ ������� ����
    /// </summary>
    public void SetupScreenShakeSettings(ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        impulseDefinition = impulseSource.m_ImpulseDefinition;

        // ���޽� ���� �ð� ����
        impulseDefinition.m_ImpulseDuration = profile.impactTime;

        // ���޽� �⺻ ���� ����
        impulseSource.m_DefaultVelocity = profile.defaultVelocity;

        // Ŀ���� � (��� ����) ����
        impulseDefinition.m_CustomImpulseShape = profile.impactCurve;

        // �����ʰ� �����ϴ� ����, ���ļ�, ���ӽð� ����
        impulseListener.m_ReactionSettings.m_AmplitudeGain = profile.listenerAmplitude;
        impulseListener.m_ReactionSettings.m_FrequencyGain = profile.listenerFrequency;
        impulseListener.m_ReactionSettings.m_Duration = profile.listenerDureation;
    }

    #endregion
}
