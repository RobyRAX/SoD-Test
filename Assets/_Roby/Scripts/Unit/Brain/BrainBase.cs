using System;
using RAXY.Movement;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class BrainBase
{
    public event Action<bool> OnWalkRunToggleChange;
    public event Action<bool> OnJumpChange;
    public event Action<bool> OnDashChange;
    public event Action<bool> OnAttackChange;
    public event Action<bool> OnSwitchEq1Change;
    public event Action<bool> OnSwitchEq2Change;

    public UnitControllerBase Cont { get; }
    public UnitStateMachine UnitSM { get; set; }
    public UnitMovement MovementCont { get; set; }
    public GroundChecker GroundChecker { get; }

    public BrainBase() { }

    public BrainBase(UnitControllerBase unitController)
    {
        if (unitController == null)
        {
            Debug.LogError("[BrainBase] unitController is NULL");
            return;
        }

        Cont = unitController;
        UnitSM = unitController.UnitStateMachine;

        MovementCont = unitController.GetComponent<UnitMovement>();
        GroundChecker = unitController.GetComponent<GroundChecker>();
    }

    [TitleGroup("Input To Send")]
    [ShowInInspector]
    public virtual Vector2 Move { get; set; }

    bool _walkRunToggle;
    [TitleGroup("Input To Send")]
    [ShowInInspector]
    public virtual bool WalkRunToggle
    {
        get => _walkRunToggle;
        set
        {
            if (_walkRunToggle == value)
                return;

            _walkRunToggle = value;
            OnWalkRunToggleChange?.Invoke(_walkRunToggle);
        }
    }

    bool _jump;
    [TitleGroup("Input To Send")]
    [ShowInInspector]
    public virtual bool Jump
    {
        get => _jump;
        set
        {
            if (_jump == value)
                return;

            _jump = value;
            OnJumpChange?.Invoke(_jump);
        }
    }

    bool _dash;
    [TitleGroup("Input To Send")]
    [ShowInInspector]
    public virtual bool Dash
    {
        get => _dash;
        set
        {
            if (_dash == value)
                return;

            _dash = value;
            OnDashChange?.Invoke(_dash);
        }
    }

    bool _attack;
    [TitleGroup("Input To Send")]
    [ShowInInspector]
    public virtual bool Attack
    {
        get => _attack;
        set
        {
            if (_attack == value)
                return;

            _attack = value;
            OnAttackChange?.Invoke(_attack);
        }
    }

    bool _switchEq1;
    [TitleGroup("Input To Send")]
    [ShowInInspector]
    public virtual bool SwitchEq1
    {
        get => _switchEq1;
        set
        {
            if (_switchEq1 == value)
                return;

            _switchEq1 = value;
            OnSwitchEq1Change?.Invoke(_switchEq1);
        }
    }

    bool _switchEq2;
    [TitleGroup("Input To Send")]
    [ShowInInspector]
    public virtual bool SwitchEq2
    {
        get => _switchEq2;
        set
        {
            if (_switchEq2 == value)
                return;

            _switchEq2 = value;
            OnSwitchEq2Change?.Invoke(_switchEq2);
        }
    }

    public virtual void Update() { }
    public virtual void OnDestroy() { }

    public virtual void ResetAllInput()
    {
        Move = default;
        WalkRunToggle = false;
        Jump = false;
        Dash = false;
        Attack = false;
        SwitchEq1 = false;
        SwitchEq2 = false;
    }
}
