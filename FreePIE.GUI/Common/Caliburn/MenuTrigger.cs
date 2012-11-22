using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Markup;
using Caliburn.Micro;

namespace FreePIE.GUI.Common.Caliburn
{
    public class MenuTrigger : MarkupExtension
    {
        static MenuTrigger()
        {
            ActionMessage.EnforceGuardsDuringInvocation = true;
        }

        [ConstructorArgument("modifiers")]
        public ModifierKeys Modifiers { get; set; }

        [ConstructorArgument("key")]
        public Key Key { get; set; }

        [ConstructorArgument("methodName")]
        public string MethodName { get; set; }

        public MenuTrigger(ModifierKeys modifiers, Key key, string methodName)
        {
            Modifiers = modifiers;
            Key = key;
            MethodName = methodName;
        }

        public MenuTrigger()
        { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var trigger = new InputBindingTrigger();

            trigger.GlobalInputBindings.Add(new KeyBinding { Modifiers = Modifiers, Key = Key });
            trigger.LocalInputBindings.Add(new MouseBinding { Gesture = new MouseGesture() { MouseAction = MouseAction.LeftClick } });

            trigger.Actions.Add(new ActionMessage { MethodName = MethodName });

            return trigger;
        }
    }
}
