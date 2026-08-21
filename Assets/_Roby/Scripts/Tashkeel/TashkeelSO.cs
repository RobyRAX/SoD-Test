using System;
using System.Collections.Generic;
using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TashkeelSO", menuName = "Scriptable Objects/TashkeelSO")]
public class TashkeelSO : ScriptableObject, ICombatData
{
    public string tashkeelId;
    public TashkeelInstance tashkeelPrefab;

    [TitleGroup("Animation Override")]
    public AnimationClipSet idle;
    public AnimationClipSet walk;
    public AnimationClipSet run;

    [TitleGroup("Action")]
    public CombatActionType actionType;

    [ShowIf("@IsAttack")]
    public List<HitEntry> hitEntries;

    [ShowIf("@IsAttack")]
    public List<VfxEntry> vfxEntries;

    [ShowIf("@IsAttack")]
    public List<AttackAction> attackActions;

    bool IsAttack => actionType == CombatActionType.Attack;

    CombatActionType ICombatData.ActionType => actionType;
    IReadOnlyList<AttackAction> ICombatData.AttackActions => attackActions;
    IReadOnlyList<HitEntry> ICombatData.HitEntries => hitEntries;
    IReadOnlyList<VfxEntry> ICombatData.VfxEntries => vfxEntries;
    AnimationClipSet ICombatData.Idle => idle;
    AnimationClipSet ICombatData.Walk => walk;
    AnimationClipSet ICombatData.Run => run;
}

[Serializable]
public class AttackAction
{
    public string attackId;
    public AnimationClipSet animation;

    [TitleGroup("Timestamps")]
    [HideLabel]
    public AttackTimeEntrySet timeEntries;

    [TitleGroup("Timestamps")]
    [SuffixLabel("seconds")]
    public float allowTransitionTime;

    [TitleGroup("Timestamps")]
    [SuffixLabel("seconds")]
    public float endTime;

    [TitleGroup("Timestamps")]
    [SuffixLabel("seconds")]
    public float resetAttackSetTime;

    [TitleGroup("Dash")]
    public bool dashToEnemy = true;

    [TitleGroup("Dash")]
    [ShowIf("@dashToEnemy")]
    [SuffixLabel("meters")]
    public float distanceToDash = 4f;
}

[Serializable]
public class AttackTimeEntrySet
{
    [ListDrawerSettings(ShowIndexLabels = true, Expanded = true)]
    public List<HitTimeEntry> hitEntries = new();

    [ListDrawerSettings(ShowIndexLabels = true, Expanded = true)]
    public List<VfxTimeEntry> vfxEntries = new();
}

[Serializable]
public class HitEntry
{
    public float damage;
    public float knockBack;
    public float cameraShakePower;
    public float cameraShakeDuration;
    public float hitStopTimeScale = 0.1f;
    public float hitStopDuration = 0.2f;
    public GameObject hitFxPrefab;

    [SuffixLabel("meters")]
    public float hitFxSpread = 0.2f;
}

[Serializable]
public class VfxEntry
{
    public Vector3 pos;
    public Vector3 rot;
    public GameObject vfxPrefab;
}

[Serializable]
public class HitTimeEntry
{
    [SuffixLabel("seconds")]
    public float time;

    public int hitIndex;
}

[Serializable]
public class VfxTimeEntry
{
    [SuffixLabel("seconds")]
    public float time;

    public int vfxIndex;
}
