using RAXY.Movement;
using UnityEngine;

public abstract class UnitAirborneState : UnitStateBase
{
    public float horizontalSpeed;
    protected Vector2 _moveInput;
    protected GroundChecker _groundChecker;

    protected UnitAirborneState(UnitStateMachine stateMachine) : base(stateMachine)
    {
        _groundChecker = SM.MovementCont?.GroundChecker;
    }

    public override void Enter()
    {
        base.Enter();

        if (_movementCont == null)
            return;

        float airborneSpeed = _movementCont.airborneSpeedModifier;
        float currentSpeed = _movementCont.currentHorizontalVelocity.magnitude;
        horizontalSpeed = Mathf.Max(currentSpeed, airborneSpeed);
    }

    public override void PreUpdate()
    {
        base.PreUpdate();

        if (SM.Brain == null)
            return;

        _moveInput = SM.Brain.Move;
    }

    public override void Update()
    {
        base.Update();

        if (_movementCont == null)
            return;

        _movementCont.LookAtInput(_moveInput);

        if (_moveInput != Vector2.zero)
            _movementCont.Set_HorizontalVelocity(horizontalSpeed * SM.GetTransform.forward);
        else
            _movementCont.Set_HorizontalVelocity(Vector3.zero);

        _groundChecker?.ConfirmGroundType();
    }
}
