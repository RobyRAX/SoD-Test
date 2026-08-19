using RAXY.Animation;
using UnityEngine;

public class UnitDodgeBackwardState : UnitDodgeState
{
    public UnitDodgeBackwardState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Dodge_Backward";
    protected override Vector3 Direction => SM.Cont.transform.forward;

    public override void Enter()
    {
        base.Enter();

        targetSpeed = _movementCont != null ? -_movementCont.backDodgeSpeedModifier : 0f;

        var clipSet = SM.AnimationClips?.DodgeBackward;
        if (SM.AnimancerCont != null && clipSet?.AnimationClip != null)
        {
            SM.AnimancerCont.PlayAnimation(
                clipSet,
                0f,
                AnimancerController.MAIN_LAYER,
                Animancer.FadeMode.FromStart);
            BindClipEndAsFinish();
        }
        else
        {
            StartFallbackTimer();
        }
    }

    protected override void DashPhaseChangeHandler(DashPhase dashPhase)
    {
        base.DashPhaseChangeHandler(dashPhase);

        if (dashPhase == DashPhase.IdleTransition)
        {
            if (SM.Brain != null && SM.Brain.Move != Vector2.zero)
                SM.ChangeToMoveGait();
        }
        else if (dashPhase == DashPhase.None)
        {
            SM.ChangeState(SM.Idle);
        }
    }
}
