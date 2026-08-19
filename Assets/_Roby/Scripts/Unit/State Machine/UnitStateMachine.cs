using Animancer;
using RAXY.Movement;
using RAXY.StateMachine;
using UnityEngine;

public abstract class UnitStateMachine : StateMachine
{
    public GameObject GetGameObject { get; set; }
    public Transform GetTransform { get; set; }
    public Animator Animator { get; set; }
    public AnimancerComponent Animancer { get; set; }
    public GroundChecker GroundChecker { get; set; }
    public UnitMovement MovementCont { get; set; }
    public UnitControllerBase Cont { get; set; }
    public BrainBase Brain { get; set; }
}