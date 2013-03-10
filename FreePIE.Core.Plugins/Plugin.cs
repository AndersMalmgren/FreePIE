using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    public interface IUpdatable
    {
        Action OnUpdate { get; set; }
        bool GlobalHasUpdateListener { get; set; }
    }

    /// <summary>
    /// External plugins not part of this assembly should not reference and use this class, they should only reference the Contracts asasembly
    /// </summary>
    public abstract class Plugin : IPlugin, IUpdatable
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

        public bool GlobalHasUpdateListener { get; set; }

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

    public abstract class UpdateblePluginGlobal<TPlugin> where TPlugin : IUpdatable
    {
        protected readonly TPlugin plugin;
        private event GlobalNoArgumentEvent onUpdate; 

        public UpdateblePluginGlobal(TPlugin plugin)
        {
            this.plugin = plugin;
            plugin.OnUpdate = OnUpdate;
        }

        private void OnUpdate()
        {
            if (onUpdate != null)
            {
                onUpdate();
            }
        }

        private void UpdateHasUpdateListener()
        {
            plugin.GlobalHasUpdateListener = onUpdate != null;
        }

        public event GlobalNoArgumentEvent update
        {
            add
            {
                onUpdate += value;
                UpdateHasUpdateListener();
            }
            remove 
            { 
                onUpdate -= value;
                UpdateHasUpdateListener();
            }
        }
    }
}
