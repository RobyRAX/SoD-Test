using System.Collections.Generic;
using Animancer;
using Cysharp.Threading.Tasks;
using RAXY.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

public class NibblesController : MonoBehaviour
{
    NpcUnitController unitController;
    List<MonoBehaviour> components;

    async UniTask Start()
    {
        await UniTask.Yield();

        components = new();
        foreach (var comp in gameObject.GetComponents<MonoBehaviour>())
        {
            if (comp == this)
                continue;
            if (comp is AnimancerComponent)
                continue;
            if (comp is AnimancerController)
                continue;

            components.Add(comp);
        }

        unitController = GetComponent<NpcUnitController>();
        SetUnfollow();
    }

    [Button]
    public void SetFollow()
    {
        foreach (var comp in components)
        {
            comp.enabled = true;
        }

        unitController.IsWalkMode = false;
        unitController.SetFollowing(true);
    }

    [Button]
    public void SetUnfollow()
    {
        foreach (var comp in components)
        {
            comp.enabled = false;
        }

        unitController.SetFollowing(false);
    }
}
