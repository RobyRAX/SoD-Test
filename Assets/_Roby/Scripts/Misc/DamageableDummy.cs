using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Lightweight IDamageable for testing ApplyHit (damage + knockback).
/// Put on the same root (or parent) as a Collider on the enemy layer used by UnitCombat.
/// </summary>
[InfoBox("Setup: Collider on this object or a child, layer = UnitCombat enemyLayer. Dummy on root/parent so GetComponentInParent finds it.")]
public class DamageableDummy : MonoBehaviour, IDamageable
{
    [SerializeField]
    float maxHp = 100f;

    [ShowInInspector]
    [ReadOnly]
    bool _alive = true;

    public Transform GetTransform => transform;
    public GameObject GetGameObject => gameObject;

    [ShowInInspector]
    public float CurrentHp { get; set; }

    public float MaxHp
    {
        get => maxHp;
        set => maxHp = value;
    }

    void Awake()
    {
        SetAlive();
    }

    void OnEnable()
    {
        if (CurrentHp <= 0f)
            SetAlive();
    }

    public void TakeDamage(float damage)
    {
        if (!_alive || damage <= 0f)
            return;

        CurrentHp -= damage;
        Debug.Log($"[{name}] TakeDamage {damage} → HP {CurrentHp}/{MaxHp}", this);

        if (CurrentHp <= 0f)
            Die();
    }

    public void TakeKnockBack(float power, Vector3 direction)
    {
        if (!_alive || power <= 0f)
            return;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        Vector3 offset = direction.normalized * power;
        transform.position += offset;
        Debug.Log($"[{name}] TakeKnockBack power={power} offset={offset}", this);
    }

    public void Die()
    {
        if (!_alive)
            return;

        _alive = false;
        CurrentHp = 0f;
        Debug.Log($"[{name}] Die", this);
    }

    public void SetAlive()
    {
        _alive = true;
        CurrentHp = MaxHp;
        Debug.Log($"[{name}] SetAlive HP={CurrentHp}", this);
    }

    [Button("Debug / Take Damage 10")]
    void DebugTakeDamage10()
    {
        TakeDamage(10f);
    }

    [Button("Debug / Set Alive")]
    void DebugSetAlive()
    {
        SetAlive();
    }
}
