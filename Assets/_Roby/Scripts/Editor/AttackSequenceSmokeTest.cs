#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class AttackSequenceSmokeTest
{
    [MenuItem("RAXY/SoD/Smoke Test Attack Sequence Data")]
    public static void Run()
    {
        var tashkeel = AssetDatabase.LoadAssetAtPath<TashkeelSO>(
            "Assets/_Roby/_Content_0.0.0/Tashkeel/Staff/Staff - Tashkeel.asset");

        if (tashkeel == null)
        {
            Debug.LogError("[SmokeTest] Staff Tashkeel missing");
            return;
        }

        if (tashkeel.attackActions == null || tashkeel.attackActions.Count == 0)
        {
            Debug.LogError("[SmokeTest] No attackActions");
            return;
        }

        for (int i = 0; i < tashkeel.attackActions.Count; i++)
        {
            var action = tashkeel.attackActions[i];
            float len = action.animation?.AnimationClip != null
                ? action.animation.AnimationClip.length
                : 1f;
            var seq = AttackEventSequenceBuilder.Build(action, len);
            Debug.Log($"[SmokeTest] [{i}] {action.attackId} len={len:F2} events={seq.eventEntries.Count}");
            foreach (var e in seq.eventEntries)
                Debug.Log($"  - {e.eventTag} @ {e.timeEntry.time:F3}s");
        }

        var player = GameObject.Find("Shameel_CharController");
        var combat = player != null ? player.GetComponent<UnitCombat>() : null;
        Debug.Log($"[SmokeTest] player combat={(combat != null)} tashkeel={(combat != null && combat.Tashkeel != null ? combat.Tashkeel.name : "null")}");
    }
}
#endif
