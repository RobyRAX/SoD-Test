using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.EventSequence
{
    public class EventSequencer : MonoBehaviour
    {
        [TitleGroup("Debug")]
        [ShowInInspector]
        [HideReferenceObjectPicker]
        public Dictionary<string, ActiveEventSequence> activeEventSequenceDict;

        [TitleGroup("Debug")]
        [ShowInInspector]
        [HideReferenceObjectPicker]
        List<ISequencedEventListener> listeners = new();

        [TitleGroup("Debug")]
        [ShowInInspector]
        Dictionary<string, float> timeModifierDict = new();

        [TitleGroup("Debug")]
        [ShowInInspector]
        public float TotalTimeModifier { get; protected set; } = 1f;

        public void Add_TimeModifier(string modId, float modifier)
        {
            if (timeModifierDict.ContainsKey(modId))
            {
                timeModifierDict[modId] = modifier;
                Refresh_TimeModifier();
                return;
            }

            timeModifierDict.Add(modId, modifier);
            Refresh_TimeModifier();
        }

        public void Remove_TimeModifier(string modId)
        {
            if (!timeModifierDict.ContainsKey(modId))
                return;

            timeModifierDict.Remove(modId);
            Refresh_TimeModifier();
        }

        public void Refresh_TimeModifier()
        {
            float result = 1f;

            foreach (float modifier in timeModifierDict.Values)
                result *= modifier;

            TotalTimeModifier = Mathf.Clamp(result, 0.05f, 5f);
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void RegisterListener(ISequencedEventListener listener)
        {
            if (!listeners.Contains(listener))
                listeners.Add(listener);
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void UnregisterListener(ISequencedEventListener listener)
        {
            listeners.Remove(listener);
        }

        public void OnEventTriggered(EventEntry entry)
        {
            for (int i = 0; i < listeners.Count; i++)
                listeners[i].ReactToTriggeredEvent(entry);
        }

        protected virtual void Awake()
        {
            activeEventSequenceDict = new Dictionary<string, ActiveEventSequence>();
            TotalTimeModifier = 1f;

            var foundListeners = GetComponents<ISequencedEventListener>();
            foreach (var listener in foundListeners)
                RegisterListener(listener);
        }

        public void StartSequence(EventSequenceEntry sequenceEntry, float maxTime)
        {
            if (sequenceEntry == null)
                return;

            if (activeEventSequenceDict == null)
                activeEventSequenceDict = new Dictionary<string, ActiveEventSequence>();

            if (activeEventSequenceDict.TryGetValue(sequenceEntry.sequenceId, out ActiveEventSequence seq))
                seq.Stop();

            var newActiveSeq = new ActiveEventSequence(sequenceEntry, maxTime, this);
            activeEventSequenceDict[sequenceEntry.sequenceId] = newActiveSeq;
        }

        public void StopSequence(string sequenceId)
        {
            if (activeEventSequenceDict != null &&
                activeEventSequenceDict.TryGetValue(sequenceId, out ActiveEventSequence seq))
            {
                seq.Stop();
            }
        }

        public void StopAllSequence()
        {
            if (activeEventSequenceDict == null)
                return;

            foreach (var seq in activeEventSequenceDict.Values.ToArray())
                seq.Stop();
        }

        public void OnSequenceEnded(string sequenceId)
        {
            activeEventSequenceDict?.Remove(sequenceId);
        }
    }
}
