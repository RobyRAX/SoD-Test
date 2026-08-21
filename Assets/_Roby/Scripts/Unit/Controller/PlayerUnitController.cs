using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerUnitController : UnitControllerBase
{
    [SerializeField]
    UnitAnimationClipsBaseSO animationClips;

    [SerializeField]
    BrainType brainType = BrainType.ActiveUnit;

    [SerializeField]
    ActiveUnitBrainConfigSO activeUnitBrainConfig;

    [TitleGroup("Tashkeel")]
    [SerializeField]
    [LabelText("Eq1 (Staff)")]
    TashkeelSO eq1Tashkeel;

    [TitleGroup("Tashkeel")]
    [SerializeField]
    [LabelText("Eq1 Unlocked")]
    bool eq1Unlocked = true;

    [TitleGroup("Tashkeel")]
    [SerializeField]
    [LabelText("Eq2 (Compass)")]
    TashkeelSO eq2Tashkeel;

    [TitleGroup("Tashkeel")]
    [SerializeField]
    [LabelText("Eq2 Unlocked")]
    bool eq2Unlocked;

    [TitleGroup("Tashkeel")]
    [ShowInInspector]
    [ReadOnly]
    public TashkeelSO EquippedTashkeel { get; private set; }

    [TitleGroup("Tashkeel")]
    [ShowInInspector]
    [ReadOnly]
    public TashkeelInstance EquippedInstance { get; private set; }

    public bool Eq1Unlocked
    {
        get => eq1Unlocked;
        set => eq1Unlocked = value;
    }

    public bool Eq2Unlocked
    {
        get => eq2Unlocked;
        set => eq2Unlocked = value;
    }

    public override UnitAnimationClipsBaseSO AnimationClips => animationClips;

    public override AnimationClipSet ResolveIdleAnimation() =>
        ResolveLocomotionClip(EquippedTashkeel?.idle, AnimationClips?.Idle);

    public override AnimationClipSet ResolveWalkAnimation() =>
        ResolveLocomotionClip(EquippedTashkeel?.walk, AnimationClips?.Walk);

    public override AnimationClipSet ResolveRunAnimation() =>
        ResolveLocomotionClip(EquippedTashkeel?.run, AnimationClips?.Run);

    void Start()
    {
        InitUnit();
        InitBrain(brainType, activeUnitBrainConfig);

        if (eq1Tashkeel != null && eq1Unlocked)
            Equip(eq1Tashkeel);
    }

    protected override void SubscribeBrainEvents()
    {
        base.SubscribeBrainEvents();

        if (Brain == null)
            return;

        Brain.OnSwitchEq1Change -= SwitchEq1ChangedHandler;
        Brain.OnSwitchEq1Change += SwitchEq1ChangedHandler;

        Brain.OnSwitchEq2Change -= SwitchEq2ChangedHandler;
        Brain.OnSwitchEq2Change += SwitchEq2ChangedHandler;
    }

    protected override void UnsubscribeBrainEvents()
    {
        if (Brain != null)
        {
            Brain.OnSwitchEq1Change -= SwitchEq1ChangedHandler;
            Brain.OnSwitchEq2Change -= SwitchEq2ChangedHandler;
        }

        base.UnsubscribeBrainEvents();
    }

    void SwitchEq1ChangedHandler(bool isPressed)
    {
        if (isPressed)
            TryEquipSlot(1);
    }

    void SwitchEq2ChangedHandler(bool isPressed)
    {
        if (isPressed)
            TryEquipSlot(2);
    }

    [TitleGroup("Tashkeel")]
    [Button("Debug / Equip Eq1")]
    public void DebugEquipEq1()
    {
        TryEquipSlot(1);
    }

    [TitleGroup("Tashkeel")]
    [Button("Debug / Equip Eq2")]
    public void DebugEquipEq2()
    {
        TryEquipSlot(2);
    }

    [TitleGroup("Tashkeel")]
    [Button("Debug / Unequip")]
    public void Unequip()
    {
        DestroyEquippedInstance();
        EquippedTashkeel = null;

        if (CombatCont != null)
        {
            CombatCont.CombatData = null;
            CombatCont.RefreshLocomotionAnimation();
        }
    }

    public void TryEquipSlot(int slot)
    {
        TashkeelSO so;
        bool unlocked;

        if (slot == 1)
        {
            so = eq1Tashkeel;
            unlocked = eq1Unlocked;
        }
        else if (slot == 2)
        {
            so = eq2Tashkeel;
            unlocked = eq2Unlocked;
        }
        else
        {
            Debug.LogWarning($"[{name}] TryEquipSlot: invalid slot {slot}.", this);
            return;
        }

        if (!unlocked)
        {
            Debug.LogWarning($"[{name}] TryEquipSlot: Eq{slot} is locked.", this);
            return;
        }

        if (so != null && EquippedTashkeel == so && EquippedInstance != null)
        {
            Unequip();
            return;
        }

        Equip(so);
    }

    public void Equip(TashkeelSO so)
    {
        if (so == null)
        {
            Debug.LogWarning($"[{name}] Equip: TashkeelSO is null.", this);
            return;
        }

        if (so.tashkeelPrefab == null)
        {
            Debug.LogWarning($"[{name}] Equip: '{so.name}' has no tashkeelPrefab.", this);
            return;
        }

        if (EquippedTashkeel == so && EquippedInstance != null)
            return;

        DestroyEquippedInstance();

        EquippedTashkeel = so;

        if (CombatCont != null)
        {
            CombatCont.CombatData = so.actionType == CombatActionType.Attack
                ? so
                : null;
        }

        EquippedInstance = Instantiate(so.tashkeelPrefab, transform);
        EquippedInstance.TashkeelSO = so;
        EquippedInstance.BindToHolder(this);

        CombatCont?.RefreshLocomotionAnimation();
    }

    void DestroyEquippedInstance()
    {
        CombatCont?.StopCombat();

        if (EquippedInstance != null)
        {
            Destroy(EquippedInstance.gameObject);
            EquippedInstance = null;
        }
    }
}
