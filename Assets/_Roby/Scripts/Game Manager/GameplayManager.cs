using RAXY.Event;
using RAXY.InputSystem;
using RAXY.Narrative;
using RAXY.NotificationSystem;
using RAXY.Utility;
using UnityEngine;

public class GameplayManager : Singleton<GameplayManager>
{
    public PlayerUnitController playerUnit;
    public Canvas onScreenControlCanvas;
    public SwitchTashkeelUI switchTashkeelUI;
    public InputActionEventSO interactEventSO;
    public EventSO unlockEq1EventSO;
    public EventSO unlockEq2EventSO;
    public NarrativeTrigger onNotifEq1ClosedNarrativeTrigger;

    bool _brainInputLockedByDialogue;

    void Start()
    {
        switchTashkeelUI.Setup(playerUnit);

        unlockEq1EventSO?.Subscribe(OnEq1UnlockHandler);
        unlockEq2EventSO?.Subscribe(OnEq2UnlockHandler);

        interactEventSO?.Subscribe(OnInteractTriggeredHandler);
        BindFullscreenDialogueEvents(true);
    }

    protected override void OnDestroy()
    {
        BindFullscreenDialogueEvents(false);
        base.OnDestroy();

        unlockEq1EventSO?.Unsubscribe(OnEq1UnlockHandler);
        unlockEq2EventSO?.Unsubscribe(OnEq2UnlockHandler);
        interactEventSO?.Unsubscribe(OnInteractTriggeredHandler);
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

    void OnFullscreenDialogueStartHandler(FullscreenDialogueDataSO data, string collectionId)
    {
        if (_brainInputLockedByDialogue)
            return;

        if (playerUnit?.Brain is not ActiveUnitBrain brain)
            return;

        onScreenControlCanvas?.gameObject.SetActive(false);
        brain.UnsubscribeToEventSO();
        brain.ResetAllInput();
        _brainInputLockedByDialogue = true;
    }

    void OnFullscreenDialogueEndHandler(FullscreenDialogueDataSO data, string collectionId)
    {
        if (!_brainInputLockedByDialogue)
            return;

        if (playerUnit?.Brain is not ActiveUnitBrain brain)
            return;

        onScreenControlCanvas?.gameObject.SetActive(true);
        brain.SubscribeToEventSO();
        _brainInputLockedByDialogue = false;
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
