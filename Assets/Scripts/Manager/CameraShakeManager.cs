using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    #region 레퍼런스
    public static CameraShakeManager instance;

    [SerializeField] private float globalShakeForce = 1f;                       // 기본 카메라 흔들림 세기
    [SerializeField] private CinemachineImpulseListener impulseListener;

    private CinemachineImpulseDefinition impulseDefinition;

    #endregion

    #region 초기화
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    #endregion

    #region 카메라 흔들림

    /// <summary>
    /// 전역 기본 세기로 카메라 흔들림 발생
    /// </summary>
    public void CameraShak(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }

    /// <summary>
    /// 프로필 설정 기반으로 카메라 흔들림 발생
    /// </summary>
    public void ScreenShakeFromProfile(ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        SetupScreenShakeSettings(profile, impulseSource);               // 프로필 설정 적용
        impulseSource.GenerateImpulseWithForce(profile.impactForce);    // 지정된 세기로 흔들림 발생
    }

    /// <summary>
    /// 스크린 흔들림에 사용할 임펄스 설정을 프로필 기반으로 구성
    /// </summary>
    public void SetupScreenShakeSettings(ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        impulseDefinition = impulseSource.m_ImpulseDefinition;

        // 임펄스 지속 시간 설정
        impulseDefinition.m_ImpulseDuration = profile.impactTime;

        // 임펄스 기본 방향 설정
        impulseSource.m_DefaultVelocity = profile.defaultVelocity;

        // 커스텀 곡선 (충격 파형) 설정
        impulseDefinition.m_CustomImpulseShape = profile.impactCurve;

        // 리스너가 반응하는 진폭, 주파수, 지속시간 설정
        impulseListener.m_ReactionSettings.m_AmplitudeGain = profile.listenerAmplitude;
        impulseListener.m_ReactionSettings.m_FrequencyGain = profile.listenerFrequency;
        impulseListener.m_ReactionSettings.m_Duration = profile.listenerDureation;
    }

    #endregion
}
