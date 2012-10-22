using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FreePIE.GUI.Views.Script.Output
{
    public class WatchViewModel : PropertyChangedBase
    {
        private DateTime lastUpdate;
        private static TimeSpan interval = TimeSpan.FromMilliseconds(20);

        public WatchViewModel()
        {
            lastUpdate = DateTime.Now;
        }

        public string Name { get; set; }

        private object value;
        public object Value
        {
            get { return value; }
            set 
            {
                if (DateTime.Now - lastUpdate > interval)
                {
                    lastUpdate = DateTime.Now;
                    this.value = value;
                    NotifyOfPropertyChange(() => Value);
                }
            }
        }
    }
}
