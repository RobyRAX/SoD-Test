using RAXY.InputSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Active Unit Brain Config SO", menuName = "RAXY/Unit/Brain/Active Unit Brain Config")]
public class ActiveUnitBrainConfigSO : BrainConfigBaseSO
{
    public InputActionEventSO moveEventSO;
    public InputActionEventSO walkRunToggleEventSO;
    public InputActionEventSO jumpEventSO;
    public InputActionEventSO dashEventSO;
}
