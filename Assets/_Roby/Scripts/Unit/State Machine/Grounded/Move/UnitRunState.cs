using RAXY.Animation;

public class UnitRunState : UnitMoveState
{
    public override string StateId => "Run";

    public UnitRunState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override void RefreshTargetSpeed()
    {
        base.RefreshTargetSpeed();
        targetSpeed = SM.MovementCont.runSpeedModifier;
    }

    public override void Enter()
    {
        base.Enter();

        if (SM.AnimancerCont != null && SM.AnimationClips?.Run != null)
        {
            SM.AnimancerCont.PlayAnimation(
                SM.AnimationClips.Run,
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

    void WalkRunToggledHandler(bool isWalkMode)
    {
        if (isWalkMode)
            SM.ChangeState(SM.Walk);
    }
}
