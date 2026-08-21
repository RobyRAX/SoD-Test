using System.Collections.Generic;
using RAXY.Animation;

public enum CombatActionType
{
    Nothing,
    Attack
}

public interface ICombatData
{
    CombatActionType ActionType { get; }
    IReadOnlyList<AttackAction> AttackActions { get; }
    IReadOnlyList<HitEntry> HitEntries { get; }
    IReadOnlyList<VfxEntry> VfxEntries { get; }
    AnimationClipSet Idle { get; }
    AnimationClipSet Walk { get; }
    AnimationClipSet Run { get; }
}
