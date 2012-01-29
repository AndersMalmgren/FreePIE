using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    /// <summary>
    /// External plugins not part of this assembly should not reference and use this class, they should only reference the Contracts asasembly
    /// </summary>
    public abstract class Plugin : IOPlugin
    {
        public abstract object CreateGlobal();
        public abstract string FriendlyName { get; }

        public Action OnUpdate { get; set; }

        public virtual Action Start() 
        { 
            return null;
        }

        protected virtual void OnStarted(object sender, EventArgs e)
        {
            if(Started != null)
            {
                Started(sender, e);
            }
        }

        public virtual void Stop() { }

        public event EventHandler Started;
        public virtual bool GetProperty(int index, IPluginProperty property)
        {
            return false;
        }

        public virtual bool SetProperties(Dictionary<string, object> properties)
        {
            return false;
        }

        public virtual void DoBeforeNextExecute() { }
    }

    public abstract class UpdateblePluginGlobal
    {
        public UpdateblePluginGlobal(Plugin plugin)
        {
            plugin.OnUpdate = OnUpdate;            
        }

        private void OnUpdate()
        {
            if (Update != null)
            {
                Update(this, new EventArgs());
            }
        }

        public event EventHandler Update;
    }
}
