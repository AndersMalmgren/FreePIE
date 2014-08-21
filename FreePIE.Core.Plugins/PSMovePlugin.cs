using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;


namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(PSMoveGlobal), IsIndexed = true)]
    public class PSMovePlugin : Plugin
    {
        private bool networkMove;
        private String ipAddress;
        private int port;

        public override object CreateGlobal()
        {
            return null;
        }

        public override string FriendlyName
        {
            get { return "PSMove"; }
        }

        public override Action Start()
        {
            return null;
        }

        public override void Stop() { 
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            switch (index)
            {
                case 0:
                    property.Name = "NetWorkMove";
                    property.Caption = "NetWork Connection";
                    property.DefaultValue = false;
                    property.HelpText = "Is the move data streamed from another computer?";
                    return true;
                case 1:
                    property.Name = "IP";
                    property.Caption = "IP Adress";
                    property.DefaultValue = "127.0.0.1";
                    property.HelpText = "IP address that the remote move is connected to (default 127.0.0.1)";
                    return true;
                case 2:
                    property.Name = "Port";
                    property.Caption = "Port";
                    property.DefaultValue = 5555;
                    property.HelpText = "Port number that the remote move is connected to (default 5555)";
                    return true;
            }
            return false;
        }


        public override bool SetProperties(Dictionary<string, object> properties)
        {
            networkMove = (bool)properties["NetWorkMove"];

            if (networkMove)
            {
                ipAddress = (String)properties["IP"];
                port = (int)properties["port"];
            }
            return false;
        }

        public override void DoBeforeNextExecute() {
        }

    }

    [Global(Name = "psmove")]
    public class PSMoveGlobal : UpdateblePluginGlobal<PSMovePlugin>
    {
        public PSMoveGlobal(PSMovePlugin plugin) : base(plugin) { }

    }
}