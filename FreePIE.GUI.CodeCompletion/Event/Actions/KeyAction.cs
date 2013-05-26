using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace FreePIE.GUI.CodeCompletion.Event.Actions
{
    public abstract class KeyAction : IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>
    {
        protected KeyAction(IEnumerable<Key> modifiers, Key key)
        {
            this.Modifiers = modifiers;
            this.Key = key;
            this.IsTargetSource = x => true;
        }

        protected KeyAction()
        {
            this.IsTargetSource = x => true;
            this.Modifiers = new Caliburn.Micro.BindableCollection<Key>();
        }

        private bool IsTriggered(IPopupEvent @event)
        {
            if (@event.Type != EventType.KeyPress)
                return false;

            var keyArgs = @event.EventArgs as KeyEventArgs;

            return IsTargetSource(@event.Source) && Key == keyArgs.Key && Modifiers.All(keyArgs.KeyboardDevice.IsKeyDown);
        }

        protected abstract void DoAct(CompletionPopupView view, KeyEventArgs args);

        protected virtual bool ShouldSwallow(CompletionPopupView view, KeyEventArgs args)
        {
            return false;
        }

        protected virtual bool IsTriggeredAddon(IPopupEvent @event, CompletionPopupView view)
        {
            return true;
        }

        public Key Key { get; set; }

        public Predicate<EventSource> IsTargetSource { get; set; }

        [TypeConverter(typeof (KeyActionListConverter))]
        public IEnumerable<Key> Modifiers { get; set; }

        public void Preview(IEnumerable<IPopupEvent> events, ICancellablePopupEvent current, CompletionPopupView view)
        {
            if (!IsTriggered(current) || !IsTriggeredAddon(current, view))
                return;

            var keyArgs = current.EventArgs as KeyEventArgs;

            DoAct(view, keyArgs);

            if (ShouldSwallow(view, keyArgs))
                current.Cancel();
        }

        public void Handle(IEnumerable<IPopupEvent> events, CompletionPopupView view)
        { }
    }

    public class KeyActionListConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof (string))
                return true;

            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture,
                                           object value)
        {
            var val = value as string;

            if (val == null)
                throw new InvalidOperationException("Can only convert from strings.");

            var values = val.Split(' ');

            return values.Select(modifier => (Key) Enum.Parse(typeof (Key), modifier));
        }
    }
}
