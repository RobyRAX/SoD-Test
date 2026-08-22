using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RAXY.Quest
{
    [Serializable]
    public class WaitForSeconds : QuestAction
    {
        public float waitDuration;

        public override async UniTask ExecuteAsync(QuestActionContext ctx, CancellationToken ct = default)
        {
            await UniTask.WaitForSeconds(waitDuration);
        }
    }
}

