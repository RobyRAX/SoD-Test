using UnityEngine;

public class UnitAttackState : UnitStateBase
{
    UnitCombat _combat;

    public UnitAttackState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Attack";

    public override void Enter()
    {
        base.Enter();

        _combat = SM.GetGameObject.GetComponent<UnitCombat>();
        _movementCont?.Set_HorizontalVelocity(Vector3.zero);
    }

    public override void Exit()
    {
        base.Exit();
        _combat?.OnAttackStateExit();
    }

    public override void Update()
    {
        base.Update();
        _movementCont?.Set_HorizontalVelocity(Vector3.zero);
    }
}
