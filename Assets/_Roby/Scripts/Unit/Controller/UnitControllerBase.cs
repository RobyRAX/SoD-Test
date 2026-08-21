using System;
using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class UnitControllerBase : MonoBehaviour, IDamageable
{
    #region IDamageable
    public Transform GetTransform => this.transform;
    public GameObject GetGameObject => this.gameObject;
    
    [ShowInInspector]
    public float CurrentHp { get; set; }

    [SerializeField]
    float maxHp;
    public float MaxHp { get => maxHp; set { maxHp = value; } }
    public void TakeDamage(float damage)
    {
        CurrentHp -= damage;

        if (CurrentHp <= 0)
            Die();
    }

    public void Die()
    {
        
    }

    public void SetAlive()
    {
        CurrentHp = MaxHp;
    }

    public void TakeKnockBack(float power, Vector3 direction)
    {
        if (power <= 0f || MovementCont == null)
            return;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        Vector3 impulse = direction.normalized * power;
        MovementCont.AddImpulse(
            impulse,
            horizontalDecay: 0f,
            verticalDecay: 0f,
            forceUnground: false,
            resetGravity: false,
            removeOnGrounded: false);
    }
    #endregion

    [SerializeField]
    protected Transform brainCamera;

    public UnitMovement MovementCont { get; set; }
    public AnimancerController AnimancerCont { get; set; }
    public UnitCombat CombatCont { get; set; }

    [ShowInInspector]
    [HideReferenceObjectPicker]
    public BrainBase Brain { get; set; }

    [ShowInInspector]
    [HideReferenceObjectPicker]
    public UnitStateMachine UnitStateMachine { get; set; }

    public virtual UnitAnimationClipsBaseSO AnimationClips { get; }

    public virtual AnimationClipSet ResolveIdleAnimation() =>
        ResolveLocomotionClip(CombatCont?.CombatData?.Idle, AnimationClips?.Idle);

    public virtual AnimationClipSet ResolveWalkAnimation() =>
        ResolveLocomotionClip(CombatCont?.CombatData?.Walk, AnimationClips?.Walk);

    public virtual AnimationClipSet ResolveRunAnimation() =>
        ResolveLocomotionClip(CombatCont?.CombatData?.Run, AnimationClips?.Run);

    protected static AnimationClipSet ResolveLocomotionClip(AnimationClipSet overrideClip, AnimationClipSet fallback)
    {
        if (overrideClip?.AnimationClip != null)
            return overrideClip;
        return fallback;
    }

    public event Action<bool> OnWalkRunToggled;

    bool _isWalkMode;

    [ShowInInspector]
    public bool IsWalkMode
    {
        get => _isWalkMode;
        set
        {
            if (value == _isWalkMode)
                return;

            _isWalkMode = value;
            OnWalkRunToggled?.Invoke(_isWalkMode);
        }
    }

    [Button]
    public void ToggleWalkMode()
    {
        IsWalkMode = !IsWalkMode;
    }

    public virtual void InitUnit()
    {
        MovementCont = GetComponent<UnitMovement>();
        AnimancerCont = GetComponent<AnimancerController>();
        CombatCont = GetComponent<UnitCombat>();
        UnitStateMachine = new UnitStateMachine(this);

        SetAlive();
    }

    public virtual void InitBrain(BrainType brainType, BrainConfigBaseSO configSO)
    {
        UnsubscribeBrainEvents();

        if (UnitStateMachine == null)
        {
            Debug.LogError($"[{name}] InitBrain called before InitUnit (UnitStateMachine is null).", this);
            return;
        }

        if (brainType == BrainType.ActiveUnit)
        {
            Brain = new ActiveUnitBrain(this, configSO as ActiveUnitBrainConfigSO);
            if (Brain is ActiveUnitBrain activeUnitBrain)
                activeUnitBrain.AssignCameraTransform(ResolveBrainCamera());
        }
        else if (brainType == BrainType.Undefined)
        {
            Brain = new EmptyBrain(this);
        }
        else
        {
            Debug.LogError($"[{name}] Unsupported BrainType: {brainType}. Use ActiveUnit or Undefined.", this);
            Brain = new EmptyBrain(this);
        }

        UnitStateMachine.Brain = Brain;
        SubscribeBrainEvents();
    }

    protected virtual void SubscribeBrainEvents()
    {
        if (Brain == null)
            return;

        Brain.OnWalkRunToggleChange -= WalkRunChangedHandler;
        Brain.OnWalkRunToggleChange += WalkRunChangedHandler;

        Brain.OnJumpChange -= JumpChangedHandler;
        Brain.OnJumpChange += JumpChangedHandler;

        Brain.OnDashChange -= DashChangedHandler;
        Brain.OnDashChange += DashChangedHandler;

        Brain.OnAttackChange -= AttackChangedHandler;
        Brain.OnAttackChange += AttackChangedHandler;
    }

    protected virtual void UnsubscribeBrainEvents()
    {
        if (Brain == null)
            return;

        Brain.OnWalkRunToggleChange -= WalkRunChangedHandler;
        Brain.OnJumpChange -= JumpChangedHandler;
        Brain.OnDashChange -= DashChangedHandler;
        Brain.OnAttackChange -= AttackChangedHandler;
    }

    protected virtual void Update()
    {
        if (Brain != null)
            Brain.Update();

        if (UnitStateMachine?.CurrentState != null)
        {
            UnitStateMachine.CurrentState.PreUpdate();
            UnitStateMachine.CurrentState.Update();
        }
    }

    protected virtual void LateUpdate()
    {
        if (UnitStateMachine?.CurrentState != null)
            UnitStateMachine.CurrentState.LateUpdate();
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeBrainEvents();
        Brain?.OnDestroy();
    }

    Transform ResolveBrainCamera()
    {
        if (brainCamera != null)
            return brainCamera;

        Camera main = Camera.main;
        return main != null ? main.transform : null;
    }

    void WalkRunChangedHandler(bool isPressed)
    {
        if (isPressed)
            ToggleWalkMode();
    }

    void JumpChangedHandler(bool isPressed)
    {
        if (isPressed)
            MovementCont?.TryCommenceJump();
    }

    void DashChangedHandler(bool isPressed)
    {
        if (isPressed)
            MovementCont?.TryCommenceDodge();
    }

    void AttackChangedHandler(bool isPressed)
    {
        if (isPressed)
            CombatCont?.TryCommenceAttack();
    }
}
