using System.Windows;

namespace FreePIE.GUI.Result
{
    public class MessageBoxResult : Result
    {
        private readonly string text;
        private readonly MessageBoxButton buttons;
        private readonly string caption;

        public MessageBoxResult(string caption, string text) : this(caption, text, MessageBoxButton.OK) { }

        public MessageBoxResult(string caption, string text, MessageBoxButton buttons)
        {
            this.text = text;
            this.buttons = buttons;
            this.caption = caption;
        }

        public System.Windows.MessageBoxResult Result { get; private set; }

        public override void Execute(Caliburn.Micro.ActionExecutionContext context)
        {
            Result = MessageBox.Show(text, caption, buttons);
            base.Execute(context);
        }
    }
}
