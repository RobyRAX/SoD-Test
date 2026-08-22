using System;
using System.Collections.Generic;
using RAXY.Event;
using RAXY.InputSystem;
using RAXY.Narrative;
using RAXY.NotificationSystem;
using RAXY.Utility;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : Singleton<GameplayManager>
{
    public PlayerUnitController playerUnit;
    public Canvas onScreenControlCanvas;
    public SwitchTashkeelUI switchTashkeelUI;
    public Button interactBtn;
    public InputActionEventSO interactEventSO;
    public EventSO unlockEq1EventSO;
    public EventSO unlockEq2EventSO;
    public NarrativeTrigger onNotifEq1ClosedNarrativeTrigger;

    bool _brainInputLockedByDialogue;
    bool _brainInputLockedByNotification;

    bool IsBrainInputLocked =>
        _brainInputLockedByDialogue || _brainInputLockedByNotification;

    void Start()
    {
        switchTashkeelUI.Setup(playerUnit);

        unlockEq1EventSO?.Subscribe(OnEq1UnlockHandler);
        unlockEq2EventSO?.Subscribe(OnEq2UnlockHandler);

        interactEventSO?.Subscribe(OnInteractTriggeredHandler);
        BindFullscreenDialogueEvents(true);
        BindFullscreenNotificationEvents(true);

        playerUnit.Interactor.OnInteractableUpdated -= InteractableUpdatedHandler;
        playerUnit.Interactor.OnInteractableUpdated += InteractableUpdatedHandler;
    }

    private void InteractableUpdatedHandler(List<string> list, int index)
    {
        interactBtn.gameObject.SetActive(list.Count > 0);
    }

    protected override void OnDestroy()
    {
        BindFullscreenDialogueEvents(false);
        BindFullscreenNotificationEvents(false);
        base.OnDestroy();

        unlockEq1EventSO?.Unsubscribe(OnEq1UnlockHandler);
        unlockEq2EventSO?.Unsubscribe(OnEq2UnlockHandler);
        interactEventSO?.Unsubscribe(OnInteractTriggeredHandler);

        playerUnit.Interactor.OnInteractableUpdated -= InteractableUpdatedHandler;
    }

    void BindFullscreenDialogueEvents(bool bind)
    {
        var hub = NarrativeHubManager.Instance;
        if (hub == null)
            return;

        if (bind)
        {
            hub.OnFullscreenDialogueStart += OnFullscreenDialogueStartHandler;
            hub.OnFullscreenDialogueEnd += OnFullscreenDialogueEndHandler;
        }
        else
        {
            hub.OnFullscreenDialogueStart -= OnFullscreenDialogueStartHandler;
            hub.OnFullscreenDialogueEnd -= OnFullscreenDialogueEndHandler;
        }
    }

    void BindFullscreenNotificationEvents(bool bind)
    {
        var manager = NotificationManager.Instance;
        if (manager == null)
            return;

        if (bind)
        {
            manager.OnFullscreenNotificationShown += OnFullscreenNotificationShownHandler;
            manager.OnFullscreenNotificationClosed += OnFullscreenNotificationClosedHandler;
        }
        else
        {
            manager.OnFullscreenNotificationShown -= OnFullscreenNotificationShownHandler;
            manager.OnFullscreenNotificationClosed -= OnFullscreenNotificationClosedHandler;
        }
    }

    void OnFullscreenDialogueStartHandler(FullscreenDialogueDataSO data, string collectionId)
    {
        if (_brainInputLockedByDialogue)
            return;

        LockBrainInput();
        _brainInputLockedByDialogue = true;
    }

    void OnFullscreenDialogueEndHandler(FullscreenDialogueDataSO data, string collectionId)
    {
        if (!_brainInputLockedByDialogue)
            return;

        _brainInputLockedByDialogue = false;
        UnlockBrainInputIfIdle();
    }

    void OnFullscreenNotificationShownHandler()
    {
        if (_brainInputLockedByNotification)
            return;

        LockBrainInput();
        _brainInputLockedByNotification = true;
    }

    void OnFullscreenNotificationClosedHandler()
    {
        if (!_brainInputLockedByNotification)
            return;

        _brainInputLockedByNotification = false;
        UnlockBrainInputIfIdle();
    }

    void LockBrainInput()
    {
        if (playerUnit?.Brain is not ActiveUnitBrain brain)
            return;

        if (!IsBrainInputLocked)
        {
            onScreenControlCanvas?.gameObject.SetActive(false);
            brain.UnsubscribeToEventSO();
            brain.ResetAllInput();
        }
    }

    void UnlockBrainInputIfIdle()
    {
        if (IsBrainInputLocked)
            return;

        if (playerUnit?.Brain is not ActiveUnitBrain brain)
            return;

        onScreenControlCanvas?.gameObject.SetActive(true);
        brain.SubscribeToEventSO();
    }

    void OnInteractTriggeredHandler(InputContext inputCtx)
    {
        if (inputCtx.BoolValue == false)
            playerUnit.Interactor.Interact();
    }

    void OnEq1UnlockHandler()
    {
        TashkeelUnlockNotifReq newReq = new();
        newReq.presetId = NotificationExtender.TASHKEEL_UNLOCK_NOTIF_ID;
        newReq.Message = "You've Unlocked Compass Tashkeel";
        newReq.Icon = playerUnit.eq1Tashkeel.tashkeelIcon;
        newReq.ActionOnClose = () => onNotifEq1ClosedNarrativeTrigger.Trigger();

        NotificationManager.Instance.ShowNotification(newReq);
        playerUnit.UnlockEquipment_1();
    }

    void OnEq2UnlockHandler()
    {
        playerUnit.UnlockEquipment_2();
        
    }
}
