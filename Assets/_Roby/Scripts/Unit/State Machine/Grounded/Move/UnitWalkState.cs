using RAXY.Animation;
using UnityEngine;

public class UnitWalkState : UnitMoveState
{
    public override string StateId => "Walk";

    public UnitWalkState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void RefreshTargetSpeed()
    {
        base.RefreshTargetSpeed();
        targetSpeed = SM.MovementCont.walkSpeedModifier;
    }

    public override void Enter()
    {
        base.Enter();

        AnimationClipSet clipSet = SM.Cont?.ResolveWalkAnimation();
        if (SM.AnimancerCont != null && clipSet?.AnimationClip != null)
        {
            SM.AnimancerCont.PlayAnimation(
                clipSet,
                0.2f,
                AnimancerController.MAIN_LAYER);
        }

        SM.Cont.OnWalkRunToggled += WalkRunToggledHandler;
    }

    public override void Exit()
    {
        base.Exit();
        SM.Cont.OnWalkRunToggled -= WalkRunToggledHandler;
    }

    public override void PreUpdate()
    {
        base.PreUpdate();

        if (SM.Brain == null)
            return;

        if (SM.Brain.Move == Vector2.zero)
            SM.ChangeState(SM.Idle);
    }

    void WalkRunToggledHandler(bool isWalkMode)
    {
        if (isWalkMode == false)
            SM.ChangeState(SM.Run);
    }
}
