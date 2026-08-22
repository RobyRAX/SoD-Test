using System;
using System.Collections.Generic;
using RAXY.NotificationSystem;
using UnityEngine;

public class NotificationExtender : MonoBehaviour, INotificationPresetIdProvider
{
    public const string TASHKEEL_UNLOCK_NOTIF_ID = "Tashkeel_Unlock";

    public List<string> PresetIds
    {
        get
        {
            List<string> temp = new();
            temp.Add(TASHKEEL_UNLOCK_NOTIF_ID);
            return temp;
        }
    }
}

[Serializable]
public class TashkeelUnlockNotifReq : NotificationRequestBase
{
    public Action ActionOnClose;
}