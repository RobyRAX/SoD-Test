using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.EventSequence
{
    [Serializable]
    [HideReferenceObjectPicker]
    public class EventSequenceEntry
    {
        public string sequenceId;

        [ListDrawerSettings(ListElementLabelName = "Label")]
        [HideReferenceObjectPicker]
        public List<EventEntry> eventEntries;
    }

    [Serializable]
    [HideReferenceObjectPicker]
    public class TimeEntry
    {
        [SuffixLabel("seconds")]
        public float time;
    }

    [Serializable]
    [HideReferenceObjectPicker]
    public class EventEntry
    {
        public string eventTag;

        public TimeEntry timeEntry;

        [TitleGroup("Parameters")]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Label", DefaultExpandedState = true)]
        [HideReferenceObjectPicker]
        public EventParameter[] parameters;

#if UNITY_EDITOR
        string Label
        {
            get
            {
                if (timeEntry == null)
                    return eventTag;

                return $"{eventTag} - Time: {timeEntry.time}";
            }
        }
#endif
    }

    [Serializable]
    [HideReferenceObjectPicker]
    public class EventParameter
    {
        public int intParam;
        public float floatParam;
        public string stringParam;
        public bool boolParam;

#if UNITY_EDITOR
        string Label => $"| Int: {intParam} | Float: {floatParam} | String: '{stringParam}' | Bool: {boolParam}";
#endif
    }

    public static class EventEntrySortExtensions
    {
        public static void SortByEventTime(this List<EventEntry> entries)
        {
            if (entries == null)
                return;

            entries.Sort((a, b) => GetTime(a).CompareTo(GetTime(b)));
        }

        public static float GetTime(this EventEntry entry)
        {
            if (entry?.timeEntry == null)
                return 0f;

            return entry.timeEntry.time;
        }
    }
}
