namespace RAXY.EventSequence
{
    public interface ISequencedEventListener
    {
        void ReactToTriggeredEvent(EventEntry entry);
    }
}
