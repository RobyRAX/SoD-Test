using RAXY.Animation;
using UnityEngine;

public class UnitJumpState : UnitAirborneState
{
    float _timeToReachHeight;

    public UnitJumpState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Jump";

    public override void Enter()
    {
        base.Enter();

        if (_movementCont == null)
            return;

        _movementCont.WasJump = true;
        _movementCont.JumpStart(out _timeToReachHeight);

        if (SM.Brain != null)
            _moveInput = SM.Brain.Move;

        if (SM.AnimancerCont != null && SM.AnimationClips?.Jump != null)
        {
            SM.AnimancerCont.PlayAnimation(
                SM.AnimationClips.Jump,
                0.05f,
                fadeMode: Animancer.FadeMode.FromStart);
        }
    }

    public override void Update()
    {
        base.Update();

        if (_timeToReachHeight > 0)
            _timeToReachHeight -= Time.deltaTime;
        else
            SM.ChangeFallState(false, 0.033f);
    }
}
