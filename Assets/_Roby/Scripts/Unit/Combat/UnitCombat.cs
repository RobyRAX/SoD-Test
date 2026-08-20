using System.Collections.Generic;
using Animancer;
using RAXY.Animation;
using RAXY.EventSequence;
using Sirenix.OdinInspector;
using UnityEngine;

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

    UnitControllerBase _cont;
    UnitAttackEventSequencer _eventSequencer;
    AttackAction _currentAction;

    public bool IsAttacking => _isAttacking;
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

        _eventSequencer?.StopAllSequence();

        float maxTime = ResolveAnimationLength(action);
        if (_cont?.AnimancerCont != null && action.animation != null)
        {
            _cont.AnimancerCont.PlayAnimation(
                action.animation,
                0.05f,
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

    public void OnAttackAnimationEnd(string attackId)
    {
        Debug.Log($"[Attack] AnimationEnd: {attackId}", this);

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
        _isAttacking = false;
        _currentAction = null;
        _currentIndex = 0;
    }

    public void OnAttackStateExit()
    {
        _eventSequencer?.StopAllSequence();
        ResetAttackFlags();
    }

    void TryContinueBufferedAttack()
    {
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

        if (action.allowTransitionTime > 0f)
        {
            float allowTime = Mathf.Clamp(action.allowTransitionTime, 0f, Mathf.Max(0f, maxTime - 0.01f));
            entries.Add(CreateEvent(AttackEventTags.ALLOW_TRANSITION, allowTime, attackId));
        }

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
