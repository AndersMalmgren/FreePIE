using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace FreePIE.GUI.Views.Main
{
    public abstract class PanelViewModel : PropertyChangedBase
    {
        private string title;
        private bool isActive;
        private bool isVisible = true;

        public virtual string Title
        {
            get { return title; }
            set
            {
                title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public virtual string ContentId
        {
            get 
            { 
                return this.GetType().ToString(); 
            }
        }

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                NotifyOfPropertyChange(() => IsActive);
            }
        }

        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }
    }
}
