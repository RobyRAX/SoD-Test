using RAXY.Animation;
using RAXY.Movement;
using UnityEngine;

public class UnitFallState : UnitAirborneState
{
    public bool playAnimationOnEnter;
    public float animationEnterDelay = 0.034f;

    float _animDelayRemaining;
    bool _fallClipPlayed;
    int groundedFrameCounter;

    public UnitFallState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override string StateId => "Fall";

    public void Set_PlayAnimationOnEnter(bool play)
    {
        playAnimationOnEnter = play;
    }

    public void Set_AnimationEnterDelay(float delay)
    {
        animationEnterDelay = delay;
    }

    public override void Enter()
    {
        base.Enter();

        _groundChecker?.SetUseGroundTypeDelay(false);
        groundedFrameCounter = 0;
        _fallClipPlayed = false;
        _animDelayRemaining = playAnimationOnEnter ? 0f : animationEnterDelay;

        if (_animDelayRemaining <= 0f)
            PlayFallClip();
    }

    public override void Update()
    {
        base.Update();

        if (!_fallClipPlayed)
        {
            _animDelayRemaining -= Time.deltaTime;
            if (_animDelayRemaining <= 0f)
                PlayFallClip();
        }

        if (_groundChecker == null || !_groundChecker.IsGrounded)
        {
            groundedFrameCounter = 0;
            return;
        }

        if (_groundChecker.GroundType == GroundType.Steep)
        {
            groundedFrameCounter = 0;
            return;
        }

        bool isStableGround = !_groundChecker.isUseRaycast || _groundChecker.RaycastHit;
        if (isStableGround)
        {
            SM.ChangeState(SM.Land);
        }
        else
        {
            groundedFrameCounter++;
            if (groundedFrameCounter >= 1)
                SM.ChangeState(SM.Land);
        }
    }

    void PlayFallClip()
    {
        _fallClipPlayed = true;

        if (SM.AnimancerCont != null && SM.AnimationClips?.Fall != null)
        {
            SM.AnimancerCont.PlayAnimation(
                SM.AnimationClips.Fall,
                0.2f,
                AnimancerController.MAIN_LAYER,
                Animancer.FadeMode.FromStart);
        }
    }
}
