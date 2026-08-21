using UnityEngine;

public interface IDamageable
{
    public Transform GetTransform { get; }
    public GameObject GetGameObject { get; }
    public float CurrentHp { get; set; }
    public float MaxHp { get; set; }
    public void TakeDamage(float damage);
    public void TakeKnockBack(float power, Vector3 direction);
    public void Die();
    public void SetAlive();
}
