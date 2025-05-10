using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    #region 레퍼런스
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private ScreenShakeProfile profile;

    private float currentHealth;

    private CinemachineImpulseSource impulseSource;

    #endregion

    #region 초기화
    void Start()
    {
        currentHealth = maxHealth;
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    #endregion

    #region 공격 관련
    public void Damage(float damageAmout)
    {
        CameraShakeManager.instance.ScreenShakeFromProfile(profile, impulseSource);

        currentHealth -= damageAmout;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    #endregion
}
