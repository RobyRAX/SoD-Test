using RAXY.Movement;
using UnityEngine;

public abstract class UnitGroundedState : UnitStateBase
{
    protected UnitGroundedState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (_movementCont == null)
            return;

        _movementCont.WasJump = false;
        _movementCont.WasDoubleJump = false;
        _movementCont.GroundChecker.IsGroundedChange += OnGroundChangeHandler;
    }

    public override void Exit()
    {
        base.Exit();

        if (_movementCont?.GroundChecker != null)
            _movementCont.GroundChecker.IsGroundedChange -= OnGroundChangeHandler;
    }

    void OnGroundChangeHandler(bool isGrounded)
    {
        if (isGrounded == false)
            SM.ChangeFallState(true, 0);
    }
}
