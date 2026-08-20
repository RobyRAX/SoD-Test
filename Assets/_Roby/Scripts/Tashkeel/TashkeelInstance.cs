
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
        multiParent = GetComponent<MultiParent>();

        if (multiParent != null)
        {
            multiParent.FindFollowTargets(transform.parent);
            multiParent.SetTarget(0);
        }
    }
}