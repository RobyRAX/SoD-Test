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
    public AnimationClipSet animation;
} 