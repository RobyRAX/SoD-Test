using UnityEngine;

public abstract class UnitMoveState : UnitGroundedState
{
    public float targetSpeed;

    protected UnitMoveState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected virtual void RefreshTargetSpeed() { }

    public override void Enter()
    {
        base.Enter();
        RefreshTargetSpeed();
    }

    public override void PreUpdate()
    {
        base.PreUpdate();

        if (SM.Brain == null)
            return;

        if (SM.Brain.Move == Vector2.zero)
            SM.ChangeState(SM.Idle);
    }

    public override void Update()
    {
        base.Update();
        MoveBehaviour();
    }

    protected virtual void MoveBehaviour()
    {
        if (SM.Brain == null)
            return;

        _movementCont.Set_HorizontalVelocity(targetSpeed * SM.GetTransform.forward);
        _movementCont.LookAtInput(SM.Brain.Move);
    }
}
