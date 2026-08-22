using System;
using Cysharp.Threading.Tasks;
using RAXY.NotificationSystem;
using UnityEngine;
using UnityEngine.UI;

public class TashkeelUnlockNotifInstance : NotificationBaseUI
{
    public Button closeBtn;
    Action onCloseAction;

    Animation animationComp;

    public override void Setup(NotificationRequestBase req)
    {
        base.Setup(req);

        animationComp = GetComponent<Animation>();

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(OnClick);

        if (req is TashkeelUnlockNotifReq tashkeelReq)
        {
            onCloseAction = tashkeelReq.ActionOnClose;
        }
    }

    void OnClick()
    {
        OnClickAsync().Forget();
    }

    async UniTask OnClickAsync()
    {
        animationComp.Play("Tashkeel Notif Exit");
        await UniTask.WaitForSeconds(0.5f);

        Close();
        onCloseAction?.Invoke();
    }
}
