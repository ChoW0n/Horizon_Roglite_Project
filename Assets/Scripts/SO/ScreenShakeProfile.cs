using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScreenShake/New Profile")]
public class ScreenShakeProfile : ScriptableObject
{
    [Header("세팅")]
    public float impactTime = 0.2f;
    public float impactForce = 1f;
    public Vector3 defaultVelocity = new Vector3(0f, -1f, 0f);
    public AnimationCurve impactCurve;

    [Header("Listener 세팅")]
    public float listenerAmplitude = 1f;
    public float listenerFrequency = 1f;
    public float listenerDureation = 1f;
}
