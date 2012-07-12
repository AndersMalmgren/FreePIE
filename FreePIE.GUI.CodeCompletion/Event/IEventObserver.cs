using System.Collections.Generic;

namespace FreePIE.GUI.CodeCompletion.Event
{
    public interface IEventObserver<in TEvent, in TCancel, in TSubject>
    {
        void Preview(IEnumerable<TEvent> events, TCancel current, TSubject view);
        void Handle(IEnumerable<TEvent> events, TSubject view);
    }
}