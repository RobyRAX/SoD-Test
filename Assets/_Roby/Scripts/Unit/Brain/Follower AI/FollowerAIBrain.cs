using Sirenix.OdinInspector;
using UnityEngine;

public class FollowerAIBrain : BrainBase
{
    public FollowerAIBrain() : base() { }

    public FollowerAIBrain(UnitControllerBase unitCont, FollowerAIBrainConfigSO config) : base(unitCont)
    {
        Config = config;
        FollowTarget = config?.followTarget;
        StopDistance = config != null ? config.stopDistance : 2f;
        ResumeDistance = config != null ? config.resumeDistance : 3f;
    }

    public FollowerAIBrainConfigSO Config { get; set; }

    [ShowInInspector]
    public Transform FollowTarget { get; private set; }

    [ShowInInspector]
    public float StopDistance { get; private set; }

    [ShowInInspector]
    public float ResumeDistance { get; private set; }

    [ShowInInspector]
    public bool IsFollowing { get; private set; }

    bool _stopped;

    public void AssignFollowTarget(Transform target)
    {
        FollowTarget = target;
    }

    public void SetFollowing(bool enabled)
    {
        IsFollowing = enabled;

        if (!enabled)
        {
            _stopped = false;
            Move = Vector2.zero;
        }
    }

    public override void Update()
    {
        if (!IsFollowing || FollowTarget == null)
        {
            Move = Vector2.zero;
            _stopped = false;
            return;
        }

        Vector3 offset = FollowTarget.position - Cont.transform.position;
        offset.y = 0f;
        float dist = offset.magnitude;

        if (_stopped)
        {
            if (dist > ResumeDistance)
                _stopped = false;
            else
            {
                Move = Vector2.zero;
                return;
            }
        }
        else if (dist <= StopDistance)
        {
            _stopped = true;
            Move = Vector2.zero;
            return;
        }

        Vector3 dir = offset / dist;
        Move = new Vector2(dir.x, dir.z);
    }
}
