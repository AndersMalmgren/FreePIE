using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FreePIE.GUI.Views.Script.Output
{
    public class WatchViewModel : PropertyChangedBase
    {

        public string Name { get; set; }

        private object value;
        public object Value
        {
            get { return value; }
            set
            {
                this.value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }
    }
}
