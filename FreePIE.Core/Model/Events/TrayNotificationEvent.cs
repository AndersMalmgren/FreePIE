using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreePIE.Core.Model.Events
{
    public class TrayNotificationEvent 
    {
        public DateTime EventTime = DateTime.Now;

        //public BalloonIcon Icon { get; set; }

        public string Title { get; set; }
        public string message { get; set; }

        public TrayNotificationEvent()
        {

        }

        public TrayNotificationEvent(string message, string title = "")//, BalloonIcon icon = BalloonIcon.Info)
        {
            this.message = message;
            this.Title = title;
            //this.Icon = icon;
        }

        public override string ToString()
        {
            return message;
        }
    }
}
