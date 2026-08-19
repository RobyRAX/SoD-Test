using RAXY.Animation;
using UnityEngine;

public class UnitDodgeForwardState : UnitDodgeState
{
    public UnitDodgeForwardState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Dodge_Forward";
    protected override Vector3 Direction => SM.Cont.transform.forward;

    public override void Enter()
    {
        base.Enter();

        targetSpeed = _movementCont != null ? _movementCont.forwardDodgeSpeedModifier : 0f;

        if (SM.Brain != null)
            _movementCont?.LookAtInput(SM.Brain.Move);

        var clipSet = SM.AnimationClips?.DodgeForward;
        if (SM.AnimancerCont != null && clipSet?.AnimationClip != null)
        {
            SM.AnimancerCont.PlayAnimation(
                clipSet,
                0.1f,
                AnimancerController.MAIN_LAYER,
                Animancer.FadeMode.FromStart);
            BindClipEndAsFinish();
        }
        else
        {
            StartFallbackTimer();
        }
    }

    public override void Update()
    {
        base.Update();

        if (SM.Brain != null)
            _movementCont?.LookAtInput(SM.Brain.Move);
    }

    protected override void DashPhaseChangeHandler(DashPhase dashPhase)
    {
        base.DashPhaseChangeHandler(dashPhase);

        if (SM.GroundChecker != null && SM.GroundChecker.IsGrounded)
        {
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
        else if (dashPhase == DashPhase.IdleTransition)
        {
            SM.ChangeFallState(true, 0);
        }
    }
}
