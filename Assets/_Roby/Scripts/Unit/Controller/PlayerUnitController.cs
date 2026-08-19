using UnityEngine;

public class PlayerUnitController : UnitControllerBase
{
    [SerializeField]
    UnitAnimationClipsBaseSO animationClips;

    [SerializeField]
    BrainType brainType = BrainType.ActiveUnit;

    [SerializeField]
    ActiveUnitBrainConfigSO activeUnitBrainConfig;

    public override UnitAnimationClipsBaseSO AnimationClips => animationClips;

    void Start()
    {
        InitUnit();
        InitBrain(brainType, activeUnitBrainConfig);
    }
}
