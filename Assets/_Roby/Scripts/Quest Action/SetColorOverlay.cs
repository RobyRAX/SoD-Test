using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RAXY.Narrative;
using RAXY.Quest;
using UnityEngine;

[Serializable]
public class SetColorOverlay : QuestAction
{
    public Color color = Color.white;
    public float fadeDuration;

    public override async UniTask ExecuteAsync(QuestActionContext ctx, CancellationToken ct = default)
    {
        ColorOverlayManager.Instance?.SetColorOverlay(color, fadeDuration, true);
        await UniTask.WaitForSeconds(fadeDuration);
    }
}
