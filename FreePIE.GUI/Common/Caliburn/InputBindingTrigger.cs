using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Caliburn.Micro;

namespace FreePIE.GUI.Common.Caliburn
{
    public class InputBindingTrigger : TriggerBase<FrameworkElement>, ICommand
    {
        public InputBindingTrigger()
        {
            GlobalInputBindings = new BindableCollection<InputBinding>();
            LocalInputBindings = new BindableCollection<InputBinding>();
        }

        public static readonly DependencyProperty LocalInputBindingsProperty =
            DependencyProperty.Register("LocalInputBindings", typeof(BindableCollection<InputBinding>), typeof(InputBindingTrigger), new PropertyMetadata(default(BindableCollection<InputBinding>)));

        public BindableCollection<InputBinding> LocalInputBindings
        {
            get { return (BindableCollection<InputBinding>)GetValue(LocalInputBindingsProperty); }
            set { SetValue(LocalInputBindingsProperty, value); }
        }

        public BindableCollection<InputBinding> GlobalInputBindings
        {
            get { return (BindableCollection<InputBinding>)GetValue(GlobalInputBindingProperty); }
            set { SetValue(GlobalInputBindingProperty, value); }
        }

        public static readonly DependencyProperty GlobalInputBindingProperty =
            DependencyProperty.Register("GlobalInputBinding", typeof(BindableCollection<InputBinding>), typeof(InputBindingTrigger), new UIPropertyMetadata(null));

        protected override void OnAttached()
        {
            foreach (var binding in GlobalInputBindings.Union(LocalInputBindings))
                binding.Command = this;

            AssociatedObject.Loaded += delegate
            {
                var window = GetWindow(AssociatedObject);

                foreach (var binding in GlobalInputBindings)
                    window.InputBindings.Add(binding);

                foreach (var binding in LocalInputBindings)
                    AssociatedObject.InputBindings.Add(binding);
            };

            base.OnAttached();
        }

        private Window GetWindow(FrameworkElement frameworkElement)
        {
            if (frameworkElement is Window)
                return frameworkElement as Window;

            var parent = frameworkElement.Parent as FrameworkElement;

            return GetWindow(parent);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            InvokeActions(parameter);
        }
    }
}
