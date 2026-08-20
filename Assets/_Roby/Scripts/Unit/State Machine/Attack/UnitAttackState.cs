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
        SM.Animator.applyRootMotion = true;
    }

    public override void Exit()
    {
        base.Exit();

        SM.Animator.applyRootMotion = false;
        _combat?.OnAttackStateExit();
    }

    public override void PreUpdate()
    {
        base.PreUpdate();

        if (_combat == null || _combat.AttackPhase != AttackPhase.IdleTransition)
            return;

        if (SM.Brain != null && SM.Brain.Move != Vector2.zero)
            SM.ChangeToMoveGait();
    }

    public override void Update()
    {
        base.Update();

        if (_combat != null && _combat.AttackPhase == AttackPhase.IdleTransition)
            return;

        _movementCont?.Set_HorizontalVelocity(Vector3.zero);
    }
}
