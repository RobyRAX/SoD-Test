using RAXY.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class ActiveUnitBrain : BrainBase
{
    public ActiveUnitBrain() : base() { }

    public ActiveUnitBrain(UnitControllerBase unitCont, ActiveUnitBrainConfigSO config) : base(unitCont)
    {
        Config = config;
        SubscribeToEventSO();
    }

    public ActiveUnitBrainConfigSO Config { get; set; }

    [ShowInInspector]
    Transform _camTransform;

    Vector2 _rawMove;

    public void AssignCameraTransform(Transform camTransform)
    {
        _camTransform = camTransform;
    }

    public void SubscribeToEventSO()
    {
        if (Config == null)
            return;

        UnsubscribeToEventSO();

        if (Config.moveEventSO != null)
            Config.moveEventSO.Subscribe(MoveEventChangedHandler);

        if (Config.walkRunToggleEventSO != null)
            Config.walkRunToggleEventSO.Subscribe(WalkRunChangeHandler);

        if (Config.jumpEventSO != null)
            Config.jumpEventSO.Subscribe(JumpChangeHandler);

        if (Config.dashEventSO != null)
            Config.dashEventSO.Subscribe(DashChangeHandler);

        if (Config.attackEventSO != null)
            Config.attackEventSO.Subscribe(AttackChangeHandler);

        if (Config.switchEq1EventSO != null)
            Config.switchEq1EventSO.Subscribe(SwitchEq1ChangeHandler);

        if (Config.switchEq2EventSO != null)
            Config.switchEq2EventSO.Subscribe(SwitchEq2ChangeHandler);
    }

    public void UnsubscribeToEventSO()
    {
        if (Config == null)
            return;

        if (Config.moveEventSO != null)
            Config.moveEventSO.Unsubscribe(MoveEventChangedHandler);

        if (Config.walkRunToggleEventSO != null)
            Config.walkRunToggleEventSO.Unsubscribe(WalkRunChangeHandler);

        if (Config.jumpEventSO != null)
            Config.jumpEventSO.Unsubscribe(JumpChangeHandler);

        if (Config.dashEventSO != null)
            Config.dashEventSO.Unsubscribe(DashChangeHandler);

        if (Config.attackEventSO != null)
            Config.attackEventSO.Unsubscribe(AttackChangeHandler);

        if (Config.switchEq1EventSO != null)
            Config.switchEq1EventSO.Unsubscribe(SwitchEq1ChangeHandler);

        if (Config.switchEq2EventSO != null)
            Config.switchEq2EventSO.Unsubscribe(SwitchEq2ChangeHandler);
    }

    public override Vector2 Move
    {
        get
        {
            if (_camTransform == null)
                return _rawMove;

            Vector3 camForward = _camTransform.forward;
            Vector3 camRight = _camTransform.right;

            camForward.y = 0;
            camRight.y = 0;

            camForward.Normalize();
            camRight.Normalize();

            Vector3 worldDir = camForward * _rawMove.y + camRight * _rawMove.x;
            return new Vector2(worldDir.x, worldDir.z);
        }
        set => _rawMove = value;
    }

    public override void OnDestroy()
    {
        UnsubscribeToEventSO();
    }

    void MoveEventChangedHandler(InputContext ctx)
    {
        _rawMove = ctx.Vector2Value;
    }

    void WalkRunChangeHandler(InputContext ctx)
    {
        WalkRunToggle = ctx.BoolValue;
    }

    void JumpChangeHandler(InputContext ctx)
    {
        Jump = ctx.BoolValue;
    }

    void DashChangeHandler(InputContext ctx)
    {
        Dash = ctx.BoolValue;
    }

    void AttackChangeHandler(InputContext ctx)
    {
        Attack = ctx.BoolValue;
    }

    void SwitchEq1ChangeHandler(InputContext ctx)
    {
        SwitchEq1 = ctx.BoolValue;
    }

    void SwitchEq2ChangeHandler(InputContext ctx)
    {
        SwitchEq2 = ctx.BoolValue;
    }
}
