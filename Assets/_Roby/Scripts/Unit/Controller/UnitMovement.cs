using System;
using RAXY.Movement;
using Sirenix.OdinInspector;
using UnityEngine;

public enum DashPhase
{
    None,
    Active,
    IdleTransition
}

public class UnitMovement : MovementController
{
    public event Action OnLandFinish;
    public event Action<DashPhase> OnDashPhaseChange;
    public event Action OnDashStopMove;

    public float walkSpeedModifier = 2f;
    public float runSpeedModifier = 5f;
    public float jumpHeight = 7.5f;
    public float doubleJumpHeight = 5f;
    public float airborneSpeedModifier = 7.5f;
    public float forwardDodgeSpeedModifier = 2.75f;

    public DashPhase DashPhase;

    UnitStateMachine _unitSM;

    bool _wasJump;
    [ShowInInspector]
    public bool WasJump
    {
        get => _wasJump;
        set
        {
            if (value == _wasJump)
                return;

            _wasJump = value;
        }
    }

    bool _wasDoubleJump;
    [ShowInInspector]
    public bool WasDoubleJump
    {
        get => _wasDoubleJump;
        set
        {
            if (value == _wasDoubleJump)
                return;

            _wasDoubleJump = value;
        }
    }

    public void SetStateMachine(UnitStateMachine stateMachine)
    {
        _unitSM = stateMachine;
    }

    public void LookAtInput(Vector2 inputDirection, bool instant = false)
    {
        if (!enableRotation)
            return;

        Vector3 worldDirection = new Vector3(inputDirection.x, 0f, inputDirection.y);

        if (worldDirection.sqrMagnitude > 0.001f)
            LookAtDirection_AxisY(worldDirection, instant: instant);
    }

    public virtual void JumpStart(out float timeToReachHeight)
    {
        float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Gravity) * jumpHeight);
        AddImpulse(Vector3.up * jumpVelocity,
            horizontalDecay: 0f,
            verticalDecay: 0f,
            forceUnground: true,
            resetGravity: true,
            removeOnGrounded: true);

        timeToReachHeight = jumpVelocity / Mathf.Abs(Gravity);
    }

    public void DoubleJumpStart(out float timeToReachHeight)
    {
        float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Gravity) * doubleJumpHeight);
        AddImpulse(Vector3.up * jumpVelocity,
            horizontalDecay: 0f,
            verticalDecay: 0f,
            forceUnground: true,
            resetGravity: true,
            removeOnGrounded: true);

        timeToReachHeight = jumpVelocity / Mathf.Abs(Gravity);
    }

    public virtual void TryCommenceJump()
    {
        if (_unitSM == null)
            return;

        if (GroundChecker.IsGrounded && !WasJump)
        {
            _unitSM.ChangeState(_unitSM.Jump);
        }
        else
        {
            if (WasDoubleJump)
                return;

            _unitSM.ChangeState(_unitSM.DoubleJump);
        }
    }

    public virtual void TryCommenceDodge()
    {
        if (_unitSM == null)
            return;

        if (DashPhase == DashPhase.Active)
            return;

        _unitSM.ChangeDodgeState();
    }

    public void OnLanding_Finish()
    {
        OnLandFinish?.Invoke();
    }

    public void OnDash_Start()
    {
        DashPhase = DashPhase.Active;
        OnDashPhaseChange?.Invoke(DashPhase);
    }

    public void OnDash_AllowInput()
    {
    }

    public void OnDash_StopMove()
    {
        OnDashStopMove?.Invoke();
    }

    public void OnDash_End()
    {
        DashPhase = DashPhase.IdleTransition;
        OnDashPhaseChange?.Invoke(DashPhase);
    }

    public void OnDash_Finish()
    {
        DashPhase = DashPhase.None;
        OnDashPhaseChange?.Invoke(DashPhase);
    }
}
