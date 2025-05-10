using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    #region ���۷���
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private ScreenShakeProfile profile;

    private float currentHealth;

    private CinemachineImpulseSource impulseSource;

    #endregion

    #region �ʱ�ȭ
    void Start()
    {
        currentHealth = maxHealth;
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    #endregion

    #region ���� ����
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
