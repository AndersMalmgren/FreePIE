using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public virtual bool IsFileContent
        {
            get { return false; }
        }

        public virtual string Filename { get; private set; }
        public virtual string FilePath { get; set; }
        public virtual string FileContent { get; set; }
        
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

        protected string IconName
        {
            set
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri("pack://application:,,/Resources/" + value);
                bi.EndInit();
                Icon = bi;
            }
        }

        private ImageSource icon;
        public ImageSource Icon
        {
            get { return icon; }
            private set
            {
                icon = value;
                NotifyOfPropertyChange(() => Icon);
            }
        }

        public virtual void Saved()
        {
            
        }
    }
}
