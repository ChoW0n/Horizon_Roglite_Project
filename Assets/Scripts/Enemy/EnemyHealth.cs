using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    #region ���۷���
    [SerializeField] private float maxHealth = 3f;

    private float currentHealth;

    #endregion

    #region �ʱ�ȭ
    void Start()
    {
        currentHealth = maxHealth;
    }

    #endregion

    #region ���� ����
    public void Damage(float damageAmout)
    {
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
