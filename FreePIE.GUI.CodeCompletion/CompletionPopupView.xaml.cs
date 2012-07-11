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
        public IList<IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>> Observers { get; private set; }
        private readonly FixedSizeStack<IPopupEvent> events;

        public CompletionPopupView()
        {
            InitializeComponent();
            Observers = new List<IEventObserver<IPopupEvent, ICancellablePopupEvent, CompletionPopupView>>();
            events = new FixedSizeStack<IPopupEvent>(15);
            AddObservers();

            CompletionElements.PreviewKeyDown += (sender, args) => Publish(new KeyEvent(args, EventSource.Popup));
            CompletionElements.ItemClicked += (sender, args) => Publish(new ItemClickedEvent(args.Arg2, (ICompletionItem)args.Arg1));
        }

        private void AddObservers()
        {
            Observers.Add(new CustomKeyAction(x => PopupActions.Show(this), Enumerable.Empty<Key>(), Key.OemPeriod));
            Observers.Add(new CustomKeyAction(x => PopupActions.Hide(this), Enumerable.Empty<Key>(), Key.Escape));
            
            Observers.Add(new CustomKeyAction
                              {
                                  Action = x => PopupActions.ForceShow(this),
                                  Key = Key.Space,
                                  Modifiers = new[] { Key.LeftCtrl },
                                  ShouldSwallow = true
                              });

            Observers.Add(new CustomKeyAction(x => PopupActions.Show(this), Enumerable.Empty<Key>(), Key.OemSemicolon));
            Observers.Add(new InsertionAction());
            Observers.Add(new InsertOnItemClicked());
            
            Observers.Add(new ElementChangedKeyAction { Key = Key.Up, ShouldSwallow = true, IsTargetSource = IsEditor});
            Observers.Add(new ElementChangedKeyAction { Key = Key.Down, ShouldSwallow = true, IsTargetSource = IsEditor });
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
            
            EventHandler selectionChanged = (sender, args) => view.OnEditorSelectionChanged();
            KeyEventHandler keyDown = (sender, args) => view.Publish(new KeyEvent(args, EventSource.Editor));

            if (target != null)
            {
                target.SelectionChanged += selectionChanged;
                target.PreviewKeyDown += keyDown;
            }

            if (oldTarget != null)
            {
                oldTarget.SelectionChanged -= selectionChanged;
                target.PreviewKeyDown -= keyDown;
            }
        }

        private void Publish(ICancellablePopupEvent @event)
        {
            foreach(var observer in Observers)
                observer.Preview(events, @event, this);

            if (@event.IsCancelled)
                return;

            events.Push(@event);

            foreach (var observer in Observers)
                observer.Handle(events, this);
        }

        public void PerformElementChanged(KeyEventArgs args)
        {
            if (CompletionElements.Items.Count <= 0) 
                return;

            FocusFirstElement();
            CompletionElements.RaiseEvent(args);
        }

        public void FocusFirstElement()
        {
            CompletionElements.Focus();
            (CompletionElements.ItemContainerGenerator.ContainerFromIndex(0) as UIElement).Focus();
        }

        private void OnEditorSelectionChanged()
        {
            UpdatePlacementRectangle();
        }

        private void UpdatePlacementRectangle()
        {
            var binding = BindingOperations.GetMultiBindingExpression(this, PlacementRectangleProperty);
            if (binding != null)
                binding.UpdateTarget();
        }
    }
}