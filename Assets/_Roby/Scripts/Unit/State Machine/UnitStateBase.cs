using RAXY.StateMachine;

public abstract class UnitStateBase : State
{
    public new UnitStateMachine SM { get; set; }
    protected UnitMovement _movementCont => SM.MovementCont;

    protected UnitStateBase(UnitStateMachine stateMachine) : base(stateMachine)
    {
        SM = stateMachine;
    }
}
