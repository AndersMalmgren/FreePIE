using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Caliburn.Micro;
using FreePIE.GUI.CodeCompletion.Controls;
using FreePIE.GUI.CodeCompletion.Data;
using FreePIE.GUI.CodeCompletion.Event;
using FreePIE.GUI.CodeCompletion.Event.Actions;
using FreePIE.GUI.CodeCompletion.Event.Events;

namespace FreePIE.GUI.CodeCompletion
{
    public partial class CompletionPopupView
    {
        private readonly FixedSizeStack<IPopupEvent> events;

        public CompletionPopupView()
        {
            InitializeComponent();
            events = new FixedSizeStack<IPopupEvent>(15);

            CompletionItems.PreviewKeyDown += (sender, args) => Publish(new CancellableKeyEvent(args, EventSource.Popup));
            CompletionItems.ItemClicked += (sender, args) => Publish(new ItemClickedEvent(args.Arg2, (ICompletionItem)args.Arg1));

            Opened += (obj, args) => Publish(new PopupStateChanged(PopupState.Open));
            Closed += (obj, args) => Publish(new PopupStateChanged(PopupState.Closed));
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue as CompletionPopupViewModel != null)
                AddObservers(Model.Observers);
        }

        private void AddObservers(IList<IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>> observers )
        {

            
            observers.Add(new CustomKeyAction
                              {
                                  Action = x =>
                                               {
                                                   x.InvalidatePosition();
                                                   x.IsOpen = true;
                                               },
                                  Key = Key.Space,
                                  Modifiers = new[] { Key.LeftCtrl },
                                  ShouldSwallowKeyPress = true
                              });

            observers.Add(new SelectionChangedHideAction());
            observers.Add(new InsertionAction(Key.Enter) { ShouldSwallow = true });
            observers.Add(new InsertionAction(Key.OemPeriod));
            observers.Add(new InsertOnItemClicked());
            observers.Add(new PositionAction());
            observers.Add(new CustomKeyAction(x => PopupActions.Hide(this), Enumerable.Empty<Key>(), Key.Escape));
            observers.Add(new ElementChangedKeyAction { Key = Key.Up, IsTargetSource = IsEditor});
            observers.Add(new ElementChangedKeyAction { Key = Key.Down, IsTargetSource = IsEditor });
        }

        private bool IsEditor(EventSource source)
        {
            return source == EventSource.Editor;
        }

        [TypeConverter(typeof(EditorAdapterConverter))]
        public EditorAdapterBase Target
        {
            get { return (EditorAdapterBase)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public CompletionPopupViewModel Model
        {
            get { return this.DataContext as CompletionPopupViewModel; }
        }

        [TypeConverter(typeof(EditorAdapterConverter))]
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof (EditorAdapterBase), typeof (CompletionPopupView), new PropertyMetadata(default(EditorAdapterBase), OnTargetChanged));

        private static void OnTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || obj == null)
                return;

            EditorAdapterBase target = e.NewValue as EditorAdapterBase;
            EditorAdapterBase oldTarget = e.OldValue as EditorAdapterBase;
            CompletionPopupView view = obj as CompletionPopupView;

            EventHandler selectionChanged = (sender, args) => view.Publish(new SelectionChangedEvent(target.CaretIndex));
            KeyEventHandler previewKeyDown = (sender, args) => view.Publish(new CancellableKeyEvent(args, EventSource.Editor));
            KeyEventHandler keyUp = (sender, args) => view.Publish(new KeyUpEvent(args, EventSource.Editor));
            KeyEventHandler keyDown = (sender, args) => view.Publish(new KeyEvent(args, EventSource.Editor));
            TextCompositionEventHandler previewTextInput = (sender, args) => view.Publish(new CancellableInputEvent(args));

            if (target != null)
            {
                target.PreviewTextInput += previewTextInput;
                target.SelectionChanged += selectionChanged;
                target.PreviewKeyDown += previewKeyDown;
                target.KeyDown += keyDown;
                target.KeyUp += keyUp;
            }

            if (oldTarget != null)
            {
                oldTarget.SelectionChanged -= selectionChanged;
                target.PreviewKeyDown -= previewKeyDown;
                target.KeyDown -= keyDown;
                target.KeyUp -= keyUp;
            }
        }

        private void Publish(IPopupEvent @event)
        {
            System.Diagnostics.Debug.WriteLine("publishing:" + @event.Type);

            events.Push(@event);

            foreach(var observer in Model.Observers)
                observer.Handle(events, this);
        }

        private void Publish(ICancellablePopupEvent @event)
        {
            foreach(var observer in Model.Observers)
                observer.Preview(events, @event, this);

            if (@event.IsCancelled && !@event.IsTransient)
                return;

            events.Push(@event);

            foreach (var observer in Model.Observers)
                observer.Handle(events, this);
        }

        internal void InvalidatePosition()
        {
            Publish(new PositionInvalidatedEvent());
        }
    }
}