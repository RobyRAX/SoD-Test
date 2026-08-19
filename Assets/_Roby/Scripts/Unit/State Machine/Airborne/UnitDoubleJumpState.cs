using RAXY.Animation;
using UnityEngine;

public class UnitDoubleJumpState : UnitAirborneState
{
    float _timeToReachHeight;

    public UnitDoubleJumpState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "DoubleJump";

    public override void Enter()
    {
        base.Enter();

        if (_movementCont == null)
            return;

        _movementCont.DoubleJumpStart(out _timeToReachHeight);
        _movementCont.WasDoubleJump = true;

        if (SM.AnimancerCont != null && SM.AnimationClips?.DoubleJump != null)
        {
            SM.AnimancerCont.PlayAnimation(
                SM.AnimationClips.DoubleJump,
                0f,
                AnimancerController.MAIN_LAYER);
        }
    }

    public override void Update()
    {
        base.Update();

        if (_timeToReachHeight > 0)
            _timeToReachHeight -= Time.deltaTime;
        else
            SM.ChangeFallState(false, 0.1f);
    }
}
