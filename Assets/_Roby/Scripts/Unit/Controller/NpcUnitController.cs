using Sirenix.OdinInspector;
using UnityEngine;

public class NpcUnitController : UnitControllerBase
{
    [SerializeField]
    UnitAnimationClipsBaseSO animationClips;

    [SerializeField]
    BrainType brainType = BrainType.FollowerAI;

    [SerializeField]
    FollowerAIBrainConfigSO followerBrainConfig;

    public override UnitAnimationClipsBaseSO AnimationClips => animationClips;

    void Start()
    {
        InitUnit();
        InitBrain(brainType, followerBrainConfig);

        if (Brain is FollowerAIBrain followerBrain && followerBrain.FollowTarget == null)
        {
            if (GameplayManager.Instance?.playerUnit != null)
                followerBrain.AssignFollowTarget(GameplayManager.Instance.playerUnit.transform);
        }

        AnimancerCont.PlayAnimation(animationClips.Idle, 0);
    }

    public void SetFollowing(bool enabled)
    {
        if (Brain is FollowerAIBrain followerBrain)
            followerBrain.SetFollowing(enabled);
    }

    public void AssignFollowTarget(Transform target)
    {
        if (Brain is FollowerAIBrain followerBrain)
            followerBrain.AssignFollowTarget(target);
    }

    [Button("Debug / Start Following")]
    void DebugStartFollowing()
    {
        SetFollowing(true);
    }

    [Button("Debug / Stop Following")]
    void DebugStopFollowing()
    {
        SetFollowing(false);
    }
}
