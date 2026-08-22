using System;
using RAXY.NotificationSystem;
using UnityEngine;
using UnityEngine.UI;

public class TashkeelUnlockNotifInstance : NotificationBaseUI
{
    public Button closeBtn;
    Action onCloseAction;

    public override void Setup(NotificationRequestBase req)
    {
        base.Setup(req);

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(OnClick);

        if (req is TashkeelUnlockNotifReq tashkeelReq)
        {
            onCloseAction = tashkeelReq.ActionOnClose;
        }
    }

    void OnClick()
    {
        Close();
        onCloseAction?.Invoke();
    }
}
