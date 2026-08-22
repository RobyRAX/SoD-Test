using UnityEngine;

[CreateAssetMenu(fileName = "Follower AI Brain Config SO", menuName = "RAXY/Unit/Brain/Follower AI Brain Config")]
public class FollowerAIBrainConfigSO : BrainConfigBaseSO
{
    public Transform followTarget;
    public float stopDistance = 2f;
    public float resumeDistance = 3f;
}
