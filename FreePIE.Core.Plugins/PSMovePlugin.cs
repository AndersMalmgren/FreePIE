using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(PSMoveGlobal), IsIndexed = true)]
    public class PSMovePlugin : Plugin
    {

        public override object CreateGlobal()
        {
            // TODO
            return null;
        }

        public override string FriendlyName
        {
            get { return "PSMove"; }
        }

    }

    [Global(Name = "psmove")]
    public class PSMoveGlobal : UpdateblePluginGlobal<PSMovePlugin>
    {
         public PSMoveGlobal(PSMovePlugin plugin) : base(plugin) { }
    }
}