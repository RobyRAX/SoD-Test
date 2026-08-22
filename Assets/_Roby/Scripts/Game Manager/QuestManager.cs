using System.Collections.Generic;
using RAXY.Event;
using RAXY.Quest;
using Sirenix.OdinInspector;
using UnityEngine;

public class QuestManager : QuestManagerBase, IQuestDatabase
{
    [TitleGroup("Event SO")]
    public StringEventSO TakeQuestEventSO;

    [TitleGroup("Quest Entry")]
    [SerializeField]
    List<QuestSO> quests;

    public List<QuestSO> Quests => quests;

    public QuestSO GetQuest(string questId)
    {
        return Quests.Find(x => x.QuestId == questId);
    }

    void Start()
    {
        SetQuestDatabase(this);
        InitQuestManager();

        TakeQuestEventSO?.Subscribe(OnTakeQuestEventFiredHandler);
    }

    void OnDestroy()
    {
        TakeQuestEventSO?.Unsubscribe(OnTakeQuestEventFiredHandler);
    }

    void OnTakeQuestEventFiredHandler(string questId)
    {
        TakeQuest(questId);
    }
}
