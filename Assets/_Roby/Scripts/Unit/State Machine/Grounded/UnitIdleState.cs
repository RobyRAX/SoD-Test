using RAXY.Animation;
using UnityEngine;

public class UnitIdleState : UnitGroundedState
{
    public UnitIdleState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Idle";

    public override void Enter()
    {
        base.Enter();

        AnimationClipSet clipSet = SM.Cont?.ResolveIdleAnimation();
        if (SM.AnimancerCont != null && clipSet?.AnimationClip != null)
        {
            SM.AnimancerCont.PlayAnimation(
                clipSet,
                0.25f,
                AnimancerController.MAIN_LAYER);
        }

        _movementCont?.Set_HorizontalVelocity(Vector3.zero);
    }

    public override void PreUpdate()
    {
        base.PreUpdate();

        if (SM.Brain == null)
            return;

        if (SM.Brain.Move != Vector2.zero)
        {
            if (SM.Cont.IsWalkMode)
                SM.ChangeState(SM.Walk);
            else
                SM.ChangeState(SM.Run);
        }
    }
}
