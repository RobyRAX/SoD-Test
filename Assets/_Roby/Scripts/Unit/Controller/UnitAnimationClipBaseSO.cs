using RAXY.Animation;
using UnityEngine;

public abstract class UnitAnimationClipsBaseSO : ScriptableObject
{
    public AnimationClipSet Idle;
    public AnimationClipSet Walk;
    public AnimationClipSet Run;
    public AnimationClipSet DodgeForward;
    public AnimationClipSet Jump;
    public AnimationClipSet DoubleJump;
    public AnimationClipSet Fall;
    public AnimationClipSet Land;
    public AnimationClipSet Hit;
    public AnimationClipSet Die;
}