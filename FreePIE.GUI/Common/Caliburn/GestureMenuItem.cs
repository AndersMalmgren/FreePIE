using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace FreePIE.GUI.Common.Caliburn
{
    public class GestureMenuItem : MenuItem
    {
        public override void EndInit()
        {
            Interaction.GetTriggers(this).Add(ConstructTrigger());

            if(string.IsNullOrEmpty(InputGestureText))
                InputGestureText = BuildInputGestureText(Modifiers, Key);

            base.EndInit();
        }

        private static readonly IEnumerable<ModifierKeys> ModifierKeysValues = Enum.GetValues(typeof(ModifierKeys)).Cast<ModifierKeys>().Except(new [] { ModifierKeys.None });

        private static readonly IDictionary<ModifierKeys, string> Translation = new Dictionary<ModifierKeys, string>
        {
            { ModifierKeys.Control, "Ctrl" }
        };

        private static string BuildInputGestureText(ModifierKeys modifiers, Key key)
        {
            var result = new StringBuilder();

            foreach (var val in ModifierKeysValues)
                if ((modifiers & val) == val)
                    result.Append((Translation.ContainsKey(val) ? Translation[val] : val.ToString()) + " + ");

            result.Append(key);

            return result.ToString();
        }

        private TriggerBase<FrameworkElement> ConstructTrigger()
        {
            var trigger = new InputBindingTrigger();

            trigger.GlobalInputBindings.Add(new KeyBinding { Modifiers = Modifiers, Key = Key });

            var command = new ActionMessageCommand { MethodName = Name };
            Command = command;
            trigger.Actions.Add(command);

            return trigger;
        }

        public static readonly DependencyProperty ModifiersProperty =
            DependencyProperty.Register("Modifiers", typeof(ModifierKeys), typeof(GestureMenuItem), new PropertyMetadata(default(ModifierKeys)));

        public ModifierKeys Modifiers
        {
            get { return (ModifierKeys)GetValue(ModifiersProperty); }
            set { SetValue(ModifiersProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(Key), typeof(GestureMenuItem), new PropertyMetadata(default(Key)));

        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }
    }
}
