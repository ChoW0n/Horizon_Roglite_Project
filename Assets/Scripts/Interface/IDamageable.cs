public interface IDamageable
{
    float currentHealth { get; set; }

    void Damage(float damageAmount);

    void Die();
}
