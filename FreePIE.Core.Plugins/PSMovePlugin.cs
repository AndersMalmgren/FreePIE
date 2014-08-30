using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.SensorFusion;


namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(PSMoveGlobal), IsIndexed = true)]
    public class PSMovePlugin : Plugin
    {
        /*private bool networkMove;
        private String ipAddress;
        private int port;*/
        private Dictionary<int, PSMoveGlobalHolder> holders;

        public override object CreateGlobal()
        {
            holders = new Dictionary<int, PSMoveGlobalHolder>();
            return new GlobalIndexer<PSMoveGlobal>(Create);
        }

        public PSMoveGlobal Create(int index)
        {
            var holder = new PSMoveGlobalHolder(index);
            holders[index] = holder;
            return holder.Global;
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
            // Disconnect moves
            foreach (var holder in holders.Values)
            {
                if (holder.isConnected())
                {
                    holder.disconnect();
                }
            }
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            /*switch (index)
            {
                case 0:
                    property.Name = "NetWorkMove";
                    property.Caption = "NetWork Move";
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
            }*/
            return false;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            /*networkMove = (bool)properties["NetWorkMove"];

            if (networkMove)
            {
                ipAddress = (String)properties["IP"];
                port = (int)properties["port"];
            }
            */
            return false;
        }

        public override void DoBeforeNextExecute() {
            foreach (var holder in holders.Values)
            {
                // Update the data (provisionally non-threaded update here)
                holder.Update();
                // Trigger the python event
                holder.OnUpdate();
            }
        }

    }

    public class PSMoveGlobalHolder : IUpdatable
    {
        IntPtr move;
        private readonly Quaternion quaternion;
        public float q0, q1, q2, q3;

        public PSMoveGlobalHolder(int index)
        {
            Index = index;
            quaternion = new Quaternion();
            connect();
            Global = new PSMoveGlobal(this);
        }

        public bool connect() 
        {
            move = PSMove.PSMoveAPI.psmove_connect_by_id(Index);
            // Enable inner psmoveapi IMU fusion
            PSMove.PSMoveAPI.psmove_enable_orientation(move, 1);

            return isConnected();
        }

        public bool isConnected()
        {
            return (move != null);
        }

        public void disconnect() 
        {
            PSMove.PSMoveAPI.psmove_disconnect(move);
        }

        public void resetOrientation()
        {
            PSMove.PSMoveAPI.psmove_reset_orientation(move);
        }

        public void Update()
        {
            while (PSMove.PSMoveAPI.psmove_poll(move) != 0)
            {
                // TODO Buttons Update
                // PSMove.PSMoveAPI.psmove_get_buttons();

                // Orientation Update
                PSMove.PSMoveAPI.psmove_get_orientation(move, ref q0, ref q1, ref q2, ref q3);
                quaternion.Update(q0, q1, q2, q3);
                

                // TODO Position Update
                
                // Set led and rumble updates back to the controller
                PSMove.PSMoveAPI.psmove_set_rumble(move, Rumble);
                PSMove.PSMoveAPI.psmove_update_leds(move);   
            }
        }

        public PSMoveGlobal Global { get; private set; }
        public int Index { get; private set; }
        public Action OnUpdate { get; set; }
        public bool GlobalHasUpdateListener { get; set; }

        public double Yaw { get { return quaternion.Yaw; } }
        public double Pitch { get { return quaternion.Pitch; } }
        public double Roll { get { return quaternion.Roll; } }
        public char Rumble { get; set; }
    }


    [Global(Name = "psmove")]
    public class PSMoveGlobal : UpdateblePluginGlobal<PSMoveGlobalHolder>
    {
        public PSMoveGlobal(PSMoveGlobalHolder plugin) : base(plugin) { }

        public double yaw { get { return plugin.Yaw; } }
        public double pitch { get { return plugin.Pitch; } }
        public double roll { get { return plugin.Roll; } }

        public float q0 { get { return plugin.q0; } }
        public float q1 { get { return plugin.q1; } }
        public float q2 { get { return plugin.q2; } }
        public float q3 { get { return plugin.q3; } }

        public void resetOrientation()
        {
            plugin.resetOrientation();
        }

    }
}