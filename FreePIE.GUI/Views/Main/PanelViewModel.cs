using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using FreePIE.GUI.Common.Resources;

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

        public virtual string Filename { get { return null; } }
        public virtual string FilePath { get; set; }
        public virtual string FileContent { get { return string.Empty; } }
        
        public virtual string ContentId
        {
            get 
            { 
                return GetType().ToString(); 
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
                Icon = ResourceHelper.Load(value);
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

        public ICommand CloseCommand { get; set; }

        public void Close()
        {
            if (CloseCommand.CanExecute(this))
            {
                CloseCommand.Execute(this);
            }
        }
    }
}
