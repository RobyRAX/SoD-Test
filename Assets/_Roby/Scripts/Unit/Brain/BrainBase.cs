using RAXY.Movement;
using UnityEngine;

public abstract class BrainBase
{
    public UnitControllerBase Cont { get; }
    public UnitStateMachine UnitSM { get; set; }
    public UnitMovement MovementCont { get; set; }
    public GroundChecker GroundChecker { get; }

    public BrainBase() { }
    public BrainBase(UnitControllerBase unitController)
    {
        if (unitController == null)
        {
            Debug.LogError("[BrainBase] unitController is NULL");
            return;
        }

        Cont = unitController;
        UnitSM = unitController.UnitStateMachine;

        MovementCont = unitController.GetComponent<UnitMovement>();
        GroundChecker = unitController.GetComponent<GroundChecker>();
    }
}