using RAXY.EventSequence;
using UnityEngine;

public class UnitAttackEventSequencer : EventSequencer, ISequencedEventListener
{
    UnitCombat _combat;

    protected override void Awake()
    {
        base.Awake();
        _combat = GetComponent<UnitCombat>();
    }

    public void ReactToTriggeredEvent(EventEntry entry)
    {
        if (_combat == null)
            _combat = GetComponent<UnitCombat>();

        if (_combat == null || entry == null)
            return;

        var param = entry.parameters != null && entry.parameters.Length > 0
            ? entry.parameters[0]
            : null;

        string stringParam = param?.stringParam;
        int intParam = param?.intParam ?? 0;

        if (entry.eventTag == AttackEventTags.START)
            _combat.OnAttackStart(stringParam);
        else if (entry.eventTag == AttackEventTags.HIT)
            _combat.OnAttackHit(stringParam, intParam);
        else if (entry.eventTag == AttackEventTags.LAST_HIT)
            _combat.OnAttackLastHit(stringParam, intParam);
        else if (entry.eventTag == AttackEventTags.VFX)
            _combat.OnAttackVfx(stringParam);
        else if (entry.eventTag == AttackEventTags.ALLOW_TRANSITION)
            _combat.OnAllowTransition(stringParam);
        else if (entry.eventTag == AttackEventTags.ANIMATION_END)
            _combat.OnAttackAnimationEnd(stringParam);
    }
}
