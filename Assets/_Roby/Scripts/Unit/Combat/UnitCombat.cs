using System.Collections;
using System.Collections.Generic;
using Animancer;
using RAXY.Animation;
using RAXY.EventSequence;
using RAXY.Pooling;
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
    public const float MinDashStopDistance = 2f;
    public const float DashSpeed = 15f;
    public const float DashDuration = 0.1f;

    [SerializeField]
    TashkeelSO tashkeel;

    [TitleGroup("Target Detection")]
    [SerializeField]
    LayerMask enemyLayer;

    [TitleGroup("Target Detection")]
    [SerializeField]
    float targetDetectRadius = 8f;

    [TitleGroup("Target Detection")]
    [SerializeField]
    int nearbyBufferSize = 8;

    [TitleGroup("Hit Detection")]
    [SerializeField]
    [SuffixLabel("meters")]
    float hitDetectRadius = 1.5f;

    [TitleGroup("Hit Detection")]
    [SerializeField]
    [Tooltip("Local offset from this unit. Sphere center = TransformPoint(hitDetectPosition).")]
    Vector3 hitDetectPosition = new Vector3(0f, 1f, 1f);

    [TitleGroup("Hit Detection")]
    [SerializeField]
    int hitBufferSize = 16;

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

    [ShowInInspector]
    [ReadOnly]
    Collider _currentTarget;

    [ShowInInspector]
    [ReadOnly]
    bool _isAttackDashing;

    UnitControllerBase _cont;
    UnitAttackEventSequencer _eventSequencer;
    AttackAction _currentAction;
    Collider[] _nearbyBuffer;
    int _nearbyCount;
    Collider[] _hitBuffer;
    readonly HashSet<IDamageable> _hitDamageables = new();
    Coroutine _dashCoroutine;

    public bool IsAttacking => _isAttacking;
    public bool IsAttackDashing => _isAttackDashing;
    public AttackPhase AttackPhase => _attackPhase;
    public TashkeelSO Tashkeel => tashkeel;
    public Collider CurrentTarget => _currentTarget;

    void Awake()
    {
        CacheRefs();
        EnsureNearbyBuffer();
        EnsureHitBuffer();
    }

    public void CacheRefs()
    {
        _cont = GetComponent<UnitControllerBase>();
        _eventSequencer = GetComponent<UnitAttackEventSequencer>();
    }

    void EnsureNearbyBuffer()
    {
        int size = Mathf.Max(1, nearbyBufferSize);
        if (_nearbyBuffer == null || _nearbyBuffer.Length != size)
            _nearbyBuffer = new Collider[size];
    }

    void EnsureHitBuffer()
    {
        int size = Mathf.Max(1, hitBufferSize);
        if (_hitBuffer == null || _hitBuffer.Length != size)
            _hitBuffer = new Collider[size];
    }

    Vector3 GetHitDetectWorldCenter()
    {
        return transform.TransformPoint(hitDetectPosition);
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

        StopAttackDash();
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

        var target = DecideAttackTarget();
        var move = _cont?.Brain?.Move ?? Vector2.zero;

        if (target != null)
            _cont?.MovementCont?.LookAt(target.bounds.center, instant: true);
        else if (move.sqrMagnitude > 0.001f)
            _cont?.MovementCont?.LookAtInput(move, instant: true);

        TryStartDashToTarget();
    }

    void TryStartDashToTarget()
    {
        StopAttackDash();

        if (_currentAction == null || !_currentAction.dashToEnemy)
            return;

        if (_currentTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, _currentTarget.bounds.center);
        if (distance > _currentAction.distanceToDash)
            return;

        _dashCoroutine = StartCoroutine(DashToTargetCo(_currentTarget));
    }

    IEnumerator DashToTargetCo(Collider target)
    {
        _isAttackDashing = true;
        float elapsed = 0f;

        while (elapsed < DashDuration)
        {
            if (target == null)
            {
                _cont?.MovementCont?.Set_HorizontalVelocity(Vector3.zero);
                break;
            }

            _cont?.MovementCont?.LookAt(target.bounds.center);

            float distance = Vector3.Distance(transform.position, target.bounds.center);
            if (distance > MinDashStopDistance)
                _cont?.MovementCont?.Set_HorizontalVelocity(DashSpeed * transform.forward);
            else
                _cont?.MovementCont?.Set_HorizontalVelocity(Vector3.zero);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _cont?.MovementCont?.Set_HorizontalVelocity(Vector3.zero);
        _isAttackDashing = false;
        _dashCoroutine = null;
    }

    void StopAttackDash()
    {
        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
        }

        if (_isAttackDashing)
        {
            _cont?.MovementCont?.Set_HorizontalVelocity(Vector3.zero);
            _isAttackDashing = false;
        }
    }

    public Collider DecideAttackTarget()
    {
        EnsureNearbyBuffer();
        CacheRefs();

        Vector3 origin = transform.position;
        _nearbyCount = Physics.OverlapSphereNonAlloc(
            origin,
            targetDetectRadius,
            _nearbyBuffer,
            enemyLayer);

        if (_nearbyCount <= 0)
        {
            _currentTarget = null;
            return null;
        }

        Vector2 move = _cont?.Brain?.Move ?? Vector2.zero;
        bool hasMove = move.sqrMagnitude > 0.001f;
        Vector3 aimDir = hasMove
            ? new Vector3(move.x, 0f, move.y).normalized
            : transform.forward;

        bool stickyValid = IsColliderInResults(_currentTarget);
        bool isFirstAttack = _currentIndex <= 0;

        Collider chosen;
        if (!stickyValid)
        {
            // No usable sticky — always pick nearest.
            chosen = GetNearestTarget(origin);
        }
        else if (!isFirstAttack && !hasMove)
        {
            // Mid-combo without stick: never retarget.
            chosen = _currentTarget;
        }
        else if (hasMove)
        {
            // Stick held: allow angle-based switch (first or later).
            chosen = GetTargetByAngle(origin, aimDir, _currentTarget);
        }
        else
        {
            // First attack, no stick: nearest.
            chosen = GetNearestTarget(origin);
        }

        _currentTarget = chosen;
        return _currentTarget;
    }

    Collider GetNearestTarget(Vector3 origin)
    {
        Collider nearest = null;
        float nearestSqr = float.MaxValue;

        for (int i = 0; i < _nearbyCount; i++)
        {
            Collider col = _nearbyBuffer[i];
            if (col == null)
                continue;

            float sqr = (origin - col.bounds.center).sqrMagnitude;
            if (sqr < nearestSqr)
            {
                nearestSqr = sqr;
                nearest = col;
            }
        }

        return nearest;
    }

    Collider GetTargetByAngle(Vector3 origin, Vector3 aimDir, Collider sticky)
    {
        Collider bestByAngle = null;
        float bestAngle = float.MaxValue;

        for (int i = 0; i < _nearbyCount; i++)
        {
            Collider col = _nearbyBuffer[i];
            if (col == null)
                continue;

            Vector3 toEnemy = col.bounds.center - origin;
            toEnemy.y = 0f;
            if (toEnemy.sqrMagnitude < 0.0001f)
                continue;

            float angle = Vector3.Angle(aimDir, toEnemy);
            if (angle < bestAngle)
            {
                bestAngle = angle;
                bestByAngle = col;
            }
        }

        if (bestByAngle == null)
            return sticky;

        if (bestByAngle == sticky)
            return sticky;

        // Alice: if current sticky is already far off aim, force switch.
        Vector3 toSticky = sticky.bounds.center - origin;
        toSticky.y = 0f;
        float stickyAngle = toSticky.sqrMagnitude > 0.0001f
            ? Vector3.Angle(aimDir, toSticky)
            : 180f;

        if (stickyAngle > 75f)
            return bestByAngle;

        // Challenger is behind aim cone — keep sticky.
        if (bestAngle >= 90f)
            return sticky;

        // Prefer closer sticky when both are roughly in front.
        float stickyDist = Vector3.Distance(origin, sticky.bounds.center);
        float challengerDist = Vector3.Distance(origin, bestByAngle.bounds.center);
        if (stickyDist < challengerDist)
            return sticky;

        return bestByAngle;
    }

    bool IsColliderInResults(Collider target)
    {
        if (target == null)
            return false;

        for (int i = 0; i < _nearbyCount; i++)
        {
            if (_nearbyBuffer[i] == target)
                return true;
        }

        return false;
    }

    public void OnAttackHit(string attackId, int hitIndex)
    {
        Debug.Log($"[Attack] Hit: {attackId} index={hitIndex}", this);
        ApplyHit(hitIndex);
    }

    public void OnAttackLastHit(string attackId, int hitIndex)
    {
        Debug.Log($"[Attack] LastHit: {attackId} index={hitIndex}", this);
        ApplyHit(hitIndex);
    }

    void ApplyHit(int hitIndex)
    {
        if (tashkeel == null || tashkeel.hitEntries == null)
        {
            Debug.LogWarning($"[{name}] ApplyHit: no hitEntries on Tashkeel.", this);
            return;
        }

        if (hitIndex < 0 || hitIndex >= tashkeel.hitEntries.Count)
        {
            Debug.LogWarning($"[{name}] ApplyHit: hitIndex {hitIndex} out of range (count={tashkeel.hitEntries.Count}).", this);
            return;
        }

        HitEntry entry = tashkeel.hitEntries[hitIndex];
        if (entry == null)
        {
            Debug.LogWarning($"[{name}] ApplyHit: HitEntry at {hitIndex} is null.", this);
            return;
        }

        EnsureHitBuffer();
        Vector3 center = GetHitDetectWorldCenter();
        int hitCount = Physics.OverlapSphereNonAlloc(
            center,
            hitDetectRadius,
            _hitBuffer,
            enemyLayer);

        _hitDamageables.Clear();
        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _hitBuffer[i];
            if (col == null)
                continue;

            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable == null || damageable.GetGameObject == gameObject)
                continue;

            if (!_hitDamageables.Add(damageable))
                continue;

            if (entry.damage > 0f)
                damageable.TakeDamage(entry.damage);

            if (entry.knockBack > 0f)
            {
                Vector3 dir = damageable.GetTransform.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude >= 0.0001f)
                    damageable.TakeKnockBack(entry.knockBack, dir);
            }

            SpawnHitFx(entry, col);
        }

        if (_hitDamageables.Count > 0)
            AttackFeelManager.Instance?.PlayFeel(entry);
    }

    void SpawnHitFx(HitEntry entry, Collider col)
    {
        if (entry == null || entry.hitFxPrefab == null || col == null)
            return;

        if (!entry.hitFxPrefab.TryGetComponent(out PoolableObject poolPrefab))
        {
            Debug.LogWarning($"[{name}] Hit FX prefab '{entry.hitFxPrefab.name}' needs a PoolableObject / PoolableParticleSystem.", this);
            return;
        }

        Vector3 victimCenter = col.bounds.center;
        Vector3 dir = victimCenter - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.forward;
        else
            dir.Normalize();

        Vector3 pos = victimCenter - dir * GetHorizontalRadius(col);

        if (entry.hitFxSpread > 0f)
        {
            Vector3 right = Vector3.Cross(Vector3.up, dir);
            if (right.sqrMagnitude < 0.0001f)
                right = Vector3.right;
            else
                right.Normalize();

            Vector2 spread = Random.insideUnitCircle * entry.hitFxSpread;
            pos += right * spread.x + Vector3.up * spread.y;
        }

        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        PoolableObject fx = ObjectPoolService.Instance.GetPoolableObject(poolPrefab);
        if (fx == null)
            return;

        fx.transform.SetParent(null, true);
        fx.transform.SetPositionAndRotation(pos, rot);
    }

    static float GetHorizontalRadius(Collider col)
    {
        Bounds bounds = col.bounds;
        return Mathf.Max(bounds.extents.x, bounds.extents.z);
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
        StopAttackDash();
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
        StopAttackDash();
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

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Target acquire (look / sticky)
        Gizmos.color = new Color(1f, 0.35f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, targetDetectRadius);

        // Hit damage sphere (local offset)
        Vector3 hitCenter = GetHitDetectWorldCenter();
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(hitCenter, hitDetectRadius);
        Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.15f);
        Gizmos.DrawSphere(hitCenter, hitDetectRadius);

        if (_currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _currentTarget.bounds.center);
        }
    }
#endif
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
                        new EventParameter { intParam = vfx.vfxIndex }
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
