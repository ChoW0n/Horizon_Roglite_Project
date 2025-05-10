using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    #region 레퍼런스
    [SerializeField] private float maxHealth = 3f;

    private float currentHealth;

    #endregion

    #region 초기화
    void Start()
    {
        currentHealth = maxHealth;
    }

    #endregion

    #region 공격 관련
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
