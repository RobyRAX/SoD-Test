using Animancer;
using RAXY.Animation;
using RAXY.Movement;
using RAXY.StateMachine;
using UnityEngine;

public class UnitStateMachine : StateMachine
{
    public GameObject GetGameObject { get; set; }
    public Transform GetTransform { get; set; }
    public Animator Animator { get; set; }
    public AnimancerComponent Animancer { get; set; }
    public AnimancerController AnimancerCont { get; set; }
    public GroundChecker GroundChecker { get; set; }
    public UnitMovement MovementCont { get; set; }
    public UnitControllerBase Cont { get; set; }
    public BrainBase Brain { get; set; }
    public UnitAnimationClipsBaseSO AnimationClips { get; set; }

    public UnitIdleState Idle { get; set; }
    public UnitWalkState Walk { get; set; }
    public UnitRunState Run { get; set; }
    public UnitJumpState Jump { get; set; }
    public UnitDoubleJumpState DoubleJump { get; set; }
    public UnitFallState Fall { get; set; }
    public UnitLandState Land { get; set; }
    public UnitDodgeForwardState DodgeForward { get; set; }
    public UnitDodgeBackwardState DodgeBackward { get; set; }
    public UnitAttackState Attack { get; set; }

    public UnitStateMachine(UnitControllerBase ownerCont) : base()
    {
        if (ownerCont == null)
            return;

        GetGameObject = ownerCont.gameObject;
        GetTransform = ownerCont.transform;

        Cont = ownerCont;
        Animator = GetGameObject.GetComponent<Animator>();
        Animancer = GetGameObject.GetComponent<AnimancerComponent>();
        AnimancerCont = GetGameObject.GetComponent<AnimancerController>();
        GroundChecker = GetGameObject.GetComponent<GroundChecker>();
        MovementCont = GetGameObject.GetComponent<UnitMovement>();
        AnimationClips = Cont.AnimationClips;
        Brain = ownerCont.Brain;

        MovementCont?.SetStateMachine(this);

        Idle = new UnitIdleState(this);
        Walk = new UnitWalkState(this);
        Run = new UnitRunState(this);
        Jump = new UnitJumpState(this);
        DoubleJump = new UnitDoubleJumpState(this);
        Fall = new UnitFallState(this);
        Land = new UnitLandState(this);
        DodgeForward = new UnitDodgeForwardState(this);
        DodgeBackward = new UnitDodgeBackwardState(this);
        Attack = new UnitAttackState(this);

        ChangeState(Idle);
    }

    public void ChangeAttackState()
    {
        if (CurrentState == Attack)
            return;

        ChangeState(Attack);
    }

    public void ChangeFallState(bool playFallOnEnter, float animationEnterDelay)
    {
        Fall.Set_PlayAnimationOnEnter(playFallOnEnter);
        Fall.Set_AnimationEnterDelay(animationEnterDelay);
        ChangeState(Fall);
    }

    public void ChangeToMoveGait()
    {
        if (Cont != null && Cont.IsWalkMode)
            ChangeState(Walk);
        else
            ChangeState(Run);
    }

    public void ChangeDodgeState()
    {
        if (MovementCont != null && MovementCont.DashPhase == DashPhase.Active)
            return;

        if (GroundChecker != null && GroundChecker.IsGrounded)
        {
            if (Brain != null && Brain.Move != Vector2.zero)
                ChangeState(DodgeForward);
            else
                ChangeState(DodgeBackward);
        }
        else
        {
            ChangeState(DodgeForward);
        }
    }
}
