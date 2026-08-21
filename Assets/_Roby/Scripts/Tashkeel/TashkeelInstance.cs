
using RAXY.Utility.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

public class TashkeelInstance : MonoBehaviour
{
    [TitleGroup("Runtime")]
    [ShowInInspector]
    public TashkeelSO TashkeelSO { get; set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public UnitControllerBase HolderUnit { get; set; }

    MultiParent multiParent;

    void Start()
    {
        if (HolderUnit != null)
            BindToHolder(HolderUnit);
    }

    public void BindToHolder(UnitControllerBase holder)
    {
        HolderUnit = holder;
        multiParent = GetComponent<MultiParent>();

        if (multiParent == null || holder == null)
            return;

        multiParent.FindFollowTargets(holder.transform);
        multiParent.SetTarget(0);
    }
}
