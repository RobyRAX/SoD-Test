using UnityEngine;

public abstract class UnitControllerBase : MonoBehaviour
{
    public UnitMovement MovementCont { get; set; }
    public BrainBase Brain { get; set; }
    public UnitStateMachine UnitStateMachine { get; set; }
}