using System.Collections.Generic;
using Animancer;
using RAXY.Animation;
using RAXY.EventSequence;
using Sirenix.OdinInspector;
using UnityEngine;

public enum AttackPhase
{
    None,
    Active,
    IdleTransition
}

public class UnitCombat : MonoBehaviour
{
    [SerializeField]
    TashkeelSO tashkeel;

    [ShowInInspector]
    [ReadOnly]
    int _currentIndex;

    [ShowInInspector]
    [ReadOnly]
    bool _isAttacking;

    [ShowInInspector]
    [ReadOnly]
    bool _allowExecute;

    [ShowInInspector]
    [ReadOnly]
    bool _inputTriggered;

    [ShowInInspector]
    [ReadOnly]
    bool _forceStartFromZero;

    [ShowInInspector]
    [ReadOnly]
    AttackPhase _attackPhase;

    UnitControllerBase _cont;
    UnitAttackEventSequencer _eventSequencer;
    AttackAction _currentAction;

    public bool IsAttacking => _isAttacking;
    public AttackPhase AttackPhase => _attackPhase;
    public TashkeelSO Tashkeel => tashkeel;

    void Awake()
    {
        CacheRefs();
    }

    public void CacheRefs()
    {
        _cont = GetComponent<UnitControllerBase>();
        _eventSequencer = GetComponent<UnitAttackEventSequencer>();
    }

    [Button("Debug / Commence Attack")]
    public void TryCommenceAttack()
    {
        if (tashkeel == null ||
            tashkeel.actionType != TashkeelActionType.Attack ||
            tashkeel.attackActions == null ||
            tashkeel.attackActions.Count == 0)
        {
            Debug.LogWarning($"[{name}] TryCommenceAttack: no attack actions on Tashkeel.", this);
            return;
        }

        if (_isAttacking)
        {
            if (!_allowExecute)
            {
                _inputTriggered = true;
                return;
            }

            if (_forceStartFromZero)
            {
                _forceStartFromZero = false;
                _currentIndex = 0;
                ExecuteAttack(tashkeel.attackActions[0]);
                return;
            }

            if (_currentIndex + 1 >= tashkeel.attackActions.Count)
            {
                _inputTriggered = true;
                return;
            }

            _currentIndex++;
            ExecuteAttack(tashkeel.attackActions[_currentIndex]);
            return;
        }

        _currentIndex = 0;
        _forceStartFromZero = false;
        ExecuteAttack(tashkeel.attackActions[_currentIndex]);
    }

    public void ExecuteAttack(AttackAction action)
    {
        if (action == null)
            return;

        CacheRefs();

        _currentAction = action;
        _isAttacking = true;
        _allowExecute = false;
        _inputTriggered = false;
        _forceStartFromZero = false;
        _attackPhase = AttackPhase.Active;

        _eventSequencer?.StopAllSequence();

        float maxTime = ResolveAnimationLength(action);
        if (_cont?.AnimancerCont != null && action.animation != null)
        {
            _cont.AnimancerCont.PlayAnimation(
                action.animation,
                0.1f,
                AnimancerController.MAIN_LAYER,
                FadeMode.FromStart);
        }

        var sequence = AttackEventSequenceBuilder.Build(action, maxTime);
        sequence.eventEntries.SortByEventTime();
        _eventSequencer?.StartSequence(sequence, maxTime);

        _cont?.UnitStateMachine?.ChangeAttackState();
    }

    public void OnAttackStart(string attackId)
    {
        Debug.Log($"[Attack] Start: {attackId}", this);
    }

    public void OnAttackHit(string attackId, int hitIndex)
    {
        Debug.Log($"[Attack] Hit: {attackId} index={hitIndex}", this);
    }

    public void OnAttackLastHit(string attackId, int hitIndex)
    {
        Debug.Log($"[Attack] LastHit: {attackId} index={hitIndex}", this);
    }

    public void OnAttackVfx(string vfxId)
    {
        Debug.Log($"[Attack] Vfx: {vfxId}", this);
    }

    public void OnAllowTransition(string attackId)
    {
        Debug.Log($"[Attack] AllowTransition: {attackId}", this);
        _allowExecute = true;

        if (_inputTriggered)
            TryContinueBufferedAttack();
    }

    public void OnAttackEnd(string attackId)
    {
        Debug.Log($"[Attack] End: {attackId}", this);
        _attackPhase = AttackPhase.IdleTransition;
        _allowExecute = true;
    }

    public void OnResetAttackSet(string attackId)
    {
        Debug.Log($"[Attack] ResetAttackSet: {attackId}", this);
        _currentIndex = 0;
        _inputTriggered = false;
        _forceStartFromZero = true;
    }

    public void OnAttackAnimationEnd(string attackId)
    {
        Debug.Log($"[Attack] AnimationEnd: {attackId}", this);

        if (_forceStartFromZero)
        {
            FinishAttack();
            return;
        }

        if (_inputTriggered && CanContinueToNext())
        {
            TryContinueBufferedAttack();
            return;
        }

        FinishAttack();
    }

    public void ResetAttackFlags()
    {
        _allowExecute = false;
        _inputTriggered = false;
        _forceStartFromZero = false;
        _isAttacking = false;
        _currentAction = null;
        _currentIndex = 0;
        _attackPhase = AttackPhase.None;
    }

    public void OnAttackStateExit()
    {
        _eventSequencer?.StopAllSequence();
        ResetAttackFlags();
    }

    void TryContinueBufferedAttack()
    {
        if (_forceStartFromZero)
        {
            _inputTriggered = false;
            return;
        }

        if (!CanContinueToNext())
        {
            _inputTriggered = false;
            return;
        }

        _currentIndex++;
        ExecuteAttack(tashkeel.attackActions[_currentIndex]);
    }

    bool CanContinueToNext()
    {
        return tashkeel != null &&
               tashkeel.attackActions != null &&
               _currentIndex + 1 < tashkeel.attackActions.Count;
    }

    void FinishAttack()
    {
        _eventSequencer?.StopAllSequence();
        ResetAttackFlags();

        var sm = _cont?.UnitStateMachine;
        if (sm == null)
            return;

        if (sm.CurrentState == sm.Attack)
        {
            if (sm.Brain != null && sm.Brain.Move != Vector2.zero)
                sm.ChangeToMoveGait();
            else
                sm.ChangeState(sm.Idle);
        }
    }

    static float ResolveAnimationLength(AttackAction action)
    {
        var clip = action?.animation?.AnimationClip;
        if (clip == null)
            return 1f;

        return Mathf.Max(0.01f, clip.length);
    }
}

public static class AttackEventTags
{
    public const string START = "Start";
    public const string HIT = "Hit";
    public const string LAST_HIT = "LastHit";
    public const string VFX = "Vfx";
    public const string ALLOW_TRANSITION = "AllowTransition";
    public const string END = "End";
    public const string RESET_ATTACK_SET = "ResetAttackSet";
    public const string ANIMATION_END = "AnimationEnd";
}

public static class AttackEventSequenceBuilder
{
    public static EventSequenceEntry Build(AttackAction action, float maxTime)
    {
        var entries = new List<EventEntry>();
        string attackId = string.IsNullOrEmpty(action.attackId) ? "attack" : action.attackId;

        entries.Add(CreateEvent(AttackEventTags.START, 0f, attackId));
        entries.Add(CreateEvent(AttackEventTags.ANIMATION_END, maxTime, attackId));

        AddTimedEvent(entries, AttackEventTags.ALLOW_TRANSITION, action.allowTransitionTime, attackId, maxTime);
        AddTimedEvent(entries, AttackEventTags.END, action.endTime, attackId, maxTime);
        AddTimedEvent(entries, AttackEventTags.RESET_ATTACK_SET, action.resetAttackSetTime, attackId, maxTime);

        var hits = action.timeEntries?.hitEntries;
        if (hits != null)
        {
            for (int i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];
                bool isLast = i == hits.Count - 1;
                string tag = isLast ? AttackEventTags.LAST_HIT : AttackEventTags.HIT;
                entries.Add(new EventEntry
                {
                    eventTag = tag,
                    timeEntry = new TimeEntry { time = Mathf.Clamp(hit.time, 0f, maxTime) },
                    parameters = new[]
                    {
                        new EventParameter
                        {
                            stringParam = attackId,
                            intParam = hit.hitIndex
                        }
                    }
                });
            }
        }

        var vfxList = action.timeEntries?.vfxEntries;
        if (vfxList != null)
        {
            foreach (var vfx in vfxList)
            {
                entries.Add(new EventEntry
                {
                    eventTag = AttackEventTags.VFX,
                    timeEntry = new TimeEntry { time = Mathf.Clamp(vfx.time, 0f, maxTime) },
                    parameters = new[]
                    {
                        new EventParameter { stringParam = vfx.vfxId }
                    }
                });
            }
        }

        return new EventSequenceEntry
        {
            sequenceId = attackId,
            eventEntries = entries
        };
    }

    static void AddTimedEvent(List<EventEntry> entries, string tag, float time, string attackId, float maxTime)
    {
        if (time <= 0f)
            return;

        float clamped = Mathf.Clamp(time, 0f, maxTime);
        entries.Add(CreateEvent(tag, clamped, attackId));
    }

    static EventEntry CreateEvent(string tag, float time, string attackId)
    {
        return new EventEntry
        {
            eventTag = tag,
            timeEntry = new TimeEntry { time = time },
            parameters = new[]
            {
                new EventParameter { stringParam = attackId }
            }
        };
    }
}
