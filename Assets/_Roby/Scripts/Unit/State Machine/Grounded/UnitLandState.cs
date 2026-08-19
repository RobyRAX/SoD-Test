using Animancer;
using RAXY.Animation;
using UnityEngine;

public class UnitLandState : UnitGroundedState
{
    public UnitLandState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Land";

    public override void Enter()
    {
        base.Enter();

        SM.GroundChecker?.SetUseGroundTypeDelay(true);
        _movementCont?.ClearImpulse();

        if (_movementCont != null)
            _movementCont.OnLandFinish += LandFinishHandler;

        if (SM.Brain != null && SM.Brain.Move != Vector2.zero)
        {
            ChangeToMoveGait();
            return;
        }

        if (SM.AnimancerCont != null && SM.AnimationClips?.Land != null)
        {
            SM.AnimancerCont.PlayAnimation(
                SM.AnimationClips.Land,
                0.05f,
                fadeMode: FadeMode.FromStart);

            var layer = SM.Animancer != null ? SM.Animancer.Layers[AnimancerController.MAIN_LAYER] : null;
            if (layer?.CurrentState != null)
                layer.CurrentState.Events(SM.Cont).OnEnd = LandFinishHandler;
        }
        else
        {
            SM.ChangeState(SM.Idle);
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (_movementCont != null)
            _movementCont.OnLandFinish -= LandFinishHandler;
    }

    public override void PreUpdate()
    {
        base.PreUpdate();

        if (SM.Brain == null)
            return;

        if (SM.Brain.Move != Vector2.zero)
            ChangeToMoveGait();
    }

    public override void Update()
    {
        base.Update();
        _movementCont?.Set_HorizontalVelocity(Vector3.zero);
    }

    void ChangeToMoveGait()
    {
        if (SM.Cont.IsWalkMode)
            SM.ChangeState(SM.Walk);
        else
            SM.ChangeState(SM.Run);
    }

    void LandFinishHandler()
    {
        SM.ChangeState(SM.Idle);
    }
}
