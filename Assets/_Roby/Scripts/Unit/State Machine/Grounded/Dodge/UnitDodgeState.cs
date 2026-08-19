using UnityEngine;

public abstract class UnitDodgeState : UnitGroundedState
{
    public float targetSpeed;
    protected bool _stopMove;
    protected bool _useFallbackTimer;
    protected float _fallbackRemaining;
    protected virtual Vector3 Direction => SM.GetTransform.forward;

    protected UnitDodgeState(UnitStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        _stopMove = false;
        _useFallbackTimer = false;
        _fallbackRemaining = 0f;

        if (_movementCont == null)
            return;

        _movementCont.SetAccelerationModifier(0.75f);
        _movementCont.OnDashPhaseChange += DashPhaseChangeHandler;
        _movementCont.OnDashStopMove += DashStopMoveHandler;
    }

    public override void Exit()
    {
        base.Exit();

        if (_movementCont == null)
            return;

        _movementCont.DashPhase = DashPhase.None;
        _movementCont.OnDashPhaseChange -= DashPhaseChangeHandler;
        _movementCont.OnDashStopMove -= DashStopMoveHandler;
        _movementCont.SetAccelerationModifier();
    }

    public override void PreUpdate()
    {
        base.PreUpdate();

        if (_movementCont == null || _movementCont.DashPhase != DashPhase.IdleTransition)
            return;

        if (SM.Brain != null && SM.Brain.Move != Vector2.zero)
            SM.ChangeToMoveGait();
    }

    public override void Update()
    {
        base.Update();

        if (_movementCont == null)
            return;

        if (_useFallbackTimer)
        {
            _fallbackRemaining -= Time.deltaTime;
            if (_fallbackRemaining <= 0f)
            {
                _useFallbackTimer = false;
                FinishDodgeWithoutEvents();
                return;
            }
        }

        if (_stopMove || _movementCont.DashPhase == DashPhase.IdleTransition)
            _movementCont.Set_HorizontalVelocity(Vector3.zero);
        else
            _movementCont.Set_HorizontalVelocity(Direction * targetSpeed);
    }

    protected void StartFallbackTimer()
    {
        _useFallbackTimer = true;
        _fallbackRemaining = 0.33f;
        _movementCont.OnDash_Start();
    }

    protected void BindClipEndAsFinish()
    {
        var layer = SM.Animancer != null ? SM.Animancer.Layers[RAXY.Animation.AnimancerController.MAIN_LAYER] : null;
        if (layer?.CurrentState != null)
            layer.CurrentState.Events(SM.Cont).OnEnd = () => _movementCont.OnDash_Finish();
    }

    protected virtual void DashPhaseChangeHandler(DashPhase dashPhase) { }

    void DashStopMoveHandler()
    {
        _stopMove = true;
    }

    void FinishDodgeWithoutEvents()
    {
        if (SM.Brain != null && SM.Brain.Move != Vector2.zero)
            SM.ChangeToMoveGait();
        else
            SM.ChangeState(SM.Idle);
    }
}
