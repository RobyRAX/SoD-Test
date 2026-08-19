using System;
using RAXY.Movement;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class BrainBase
{
    public event Action<bool> OnWalkRunToggleChange;
    public event Action<bool> OnJumpChange;
    public event Action<bool> OnDashChange;

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

    public virtual void Update() { }
    public virtual void OnDestroy() { }

    public virtual void ResetAllInput()
    {
        Move = default;
        WalkRunToggle = false;
        Jump = false;
        Dash = false;
    }
}
