using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RAXY.Quest;
using UnityEngine;

[Serializable]
public class SetTextOverlay : QuestAction
{
    public string text = "Text Here";
    public Color color = Color.white;
    public float fadeDuration = 0;

    public override async UniTask ExecuteAsync(QuestActionContext ctx, CancellationToken ct = default)
    {
        TextOverlayManager.Instance?.SetTextOverlay(text, color, fadeDuration, true);
        await UniTask.WaitForSeconds(fadeDuration);
    }
}
