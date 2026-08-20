using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.EventSequence
{
    [HideReferenceObjectPicker]
    [Serializable]
    public class ActiveEventSequence
    {
        public ActiveEventSequence() { }

        public ActiveEventSequence(EventSequenceEntry entry, float maxTime, EventSequencer owner)
        {
            Start(entry, maxTime, owner);
        }

        public EventSequenceEntry seqEntry;
        public EventSequencer SequencerOwner { get; private set; }

        [TitleGroup("Status")]
        [ShowInInspector]
        bool _isStarted;

        [TitleGroup("Status")]
        [ShowInInspector]
        bool _isEnded;

        [TitleGroup("Status")]
        [ShowInInspector]
        bool _isPaused;

        [TitleGroup("Status")]
        [ShowInInspector]
        public float MaxTime { get; private set; }

        [TitleGroup("Status")]
        [ShowInInspector]
        public float CurrentTime { get; private set; }

        [TitleGroup("Status")]
        [ShowInInspector]
        public float CurrentNormalizedTime => MaxTime <= 0 ? 0 : CurrentTime / MaxTime;

        [TitleGroup("Status")]
        [ShowInInspector]
        List<bool> eventTriggeredFlags;

        Coroutine _tickRoutine;

        public void Start(EventSequenceEntry entry, float maxTime, EventSequencer owner)
        {
            seqEntry = entry;
            SequencerOwner = owner;
            MaxTime = maxTime;
            CurrentTime = 0f;

            if (seqEntry == null || seqEntry.eventEntries == null)
                return;

            eventTriggeredFlags = new List<bool>(seqEntry.eventEntries.Count);
            for (int i = 0; i < seqEntry.eventEntries.Count; i++)
                eventTriggeredFlags.Add(false);

            _isStarted = true;
            _isEnded = false;

            if (_tickRoutine != null)
                SequencerOwner.StopCoroutine(_tickRoutine);

            _tickRoutine = SequencerOwner.StartCoroutine(TickCo());
        }

        public void Stop()
        {
            _isStarted = false;
            _isEnded = true;

            if (_tickRoutine != null && SequencerOwner != null)
            {
                SequencerOwner.StopCoroutine(_tickRoutine);
                _tickRoutine = null;
            }

            if (seqEntry != null && SequencerOwner != null)
                SequencerOwner.OnSequenceEnded(seqEntry.sequenceId);
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }

        IEnumerator TickCo()
        {
            while (_isStarted)
            {
                if (_isPaused)
                {
                    yield return null;
                    continue;
                }

                CurrentTime += Time.deltaTime * SequencerOwner.TotalTimeModifier;

                CheckEvent();

                if (CurrentTime >= MaxTime)
                {
                    Stop();
                    yield break;
                }

                yield return null;
            }
        }

        void CheckEvent()
        {
            for (int i = 0; i < seqEntry.eventEntries.Count; i++)
            {
                if (eventTriggeredFlags[i])
                    continue;

                var eventEntry = seqEntry.eventEntries[i];
                float triggerTime = eventEntry.GetTime();

                if (CurrentTime >= triggerTime)
                {
                    eventTriggeredFlags[i] = true;
                    SequencerOwner.OnEventTriggered(eventEntry);
                }
            }
        }
    }
}
