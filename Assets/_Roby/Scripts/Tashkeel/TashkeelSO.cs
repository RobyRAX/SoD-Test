using System;
using System.Collections.Generic;
using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TashkeelSO", menuName = "Scriptable Objects/TashkeelSO")]
public class TashkeelSO : ScriptableObject
{
    public string tashkeelId;
    public TashkeelInstance tashkeelPrefab;

    [TitleGroup("Animation Override")]
    public AnimationClipSet idle;
    public AnimationClipSet walk;
    public AnimationClipSet run;

    [TitleGroup("Action")]
    public TashkeelActionType actionType;

    [ShowIf("@IsAttack")]
    public List<AttackAction> attackActions;

    bool IsAttack => actionType == TashkeelActionType.Attack;
}

public enum TashkeelActionType
{
    Nothing,
    Attack
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

    public string vfxId;
}
